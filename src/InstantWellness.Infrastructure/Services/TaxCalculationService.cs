using System.Globalization;
using System.Text.Json;
using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.Configuration.Attributes;
using InstantWellness.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace InstantWellness.Infrastructure.Services;

public class TaxCalculationOptions
{
    public string TaxRatesCsvPath { get; set; } = "tax_rates.csv";
    public bool UseReverseGeocoding { get; set; } = true;
    /// <summary>When true, calls data-pipeline/calculate_tax.py (Python) instead of C# logic.</summary>
    public bool UsePythonTaxCalculation { get; set; } = true;
    public string PythonPath { get; set; } = "python3";
    public string PythonScriptPath { get; set; } = "";
}

public class TaxCalculationService : ITaxCalculationService
{
    private readonly IHttpClientFactory? _httpClientFactory;
    private readonly ILogger<TaxCalculationService> _logger;
    private readonly TaxCalculationOptions _options;
    private readonly Lazy<List<TaxRateRow>> _rates;

    public TaxCalculationService(
        IOptions<TaxCalculationOptions> options,
        ILogger<TaxCalculationService> logger,
        IHttpClientFactory? httpClientFactory = null)
    {
        _options = options.Value;
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _rates = new Lazy<List<TaxRateRow>>(LoadRates);
    }

    private List<TaxRateRow> LoadRates()
    {
        var path = _options.TaxRatesCsvPath;
        if (!Path.IsPathRooted(path))
            path = Path.Combine(AppContext.BaseDirectory, path);
        if (!File.Exists(path))
        {
            _logger.LogWarning("Tax rates CSV not found at {Path}. Tax calculation will use NY state-only fallback.", path);
            return [];
        }

        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
            MissingFieldFound = null,
            BadDataFound = null,
            HeaderValidated = null
        };

        using var reader = new StreamReader(path);
        using var csv = new CsvReader(reader, config);
        var rows = csv.GetRecords<TaxRateRow>().ToList();
        _logger.LogInformation("Loaded {Count} tax rate rows from {Path}", rows.Count, path);
        return rows;
    }

    public TaxLookupResult? GetTaxForAddress(string county, string? city = null)
    {
        var normalizedCounty = NormalizeCounty(county);
        var normalizedCity = string.IsNullOrWhiteSpace(city) ? "0" : city.Trim();

        var row = FindRate("NY", normalizedCounty, normalizedCity)
                  ?? FindRate("NY", normalizedCounty, "0")
                  ?? FindRate("NY", "New York State only", "0");

        return row is null ? null : MapToResult(row);
    }

    public async Task<TaxLookupResult?> GetTaxForCoordinatesAsync(double latitude, double longitude, CancellationToken cancellationToken = default)
    {
        if (_options.UsePythonTaxCalculation)
        {
            var pythonResult = await CallPythonTaxScriptAsync(latitude, longitude, cancellationToken);
            if (pythonResult is not null)
                return pythonResult;
            _logger.LogWarning("Python tax script failed. Falling back to C# implementation.");
        }

        return await GetTaxFromCSharpAsync(latitude, longitude, cancellationToken);
    }

    private async Task<TaxLookupResult?> CallPythonTaxScriptAsync(double latitude, double longitude, CancellationToken cancellationToken)
    {
        var scriptPath = ResolvePythonScriptPath();
        if (string.IsNullOrEmpty(scriptPath) || !File.Exists(scriptPath))
        {
            _logger.LogDebug("Python tax script not found at {Path}.", scriptPath ?? "(empty)");
            return null;
        }

        try
        {
            var latStr = latitude.ToString("F10", CultureInfo.InvariantCulture);
            var lonStr = longitude.ToString("F10", CultureInfo.InvariantCulture);
            var startInfo = new System.Diagnostics.ProcessStartInfo
            {
                FileName = _options.PythonPath,
                ArgumentList = { scriptPath, "--", latStr, lonStr, "1" },
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                WorkingDirectory = Path.GetDirectoryName(scriptPath) ?? ""
            };

            using var process = System.Diagnostics.Process.Start(startInfo);
            if (process is null) return null;

            var outputTask = process.StandardOutput.ReadToEndAsync(cancellationToken);
            var errorTask = process.StandardError.ReadToEndAsync(cancellationToken);
            await process.WaitForExitAsync(cancellationToken);

            var output = await outputTask;
            var error = await errorTask;
            if (process.ExitCode != 0)
            {
                _logger.LogDebug("Python tax script exited {Code}: {Error}", process.ExitCode, error.Trim());
                return null;
            }

            var doc = JsonDocument.Parse(output);
            var root = doc.RootElement;
            if (root.TryGetProperty("error", out _))
                return null;

            return new TaxLookupResult(
                root.TryGetProperty("state", out var s) ? s.GetString() ?? "NY" : "NY",
                root.TryGetProperty("county", out var c) ? c.GetString() ?? "" : "",
                root.TryGetProperty("city", out var ci) ? ci.GetString() ?? "" : "",
                root.TryGetProperty("special_jurisdiction", out var sj) ? sj.GetString() : null,
                root.TryGetProperty("state_rate", out var sr) ? (decimal)sr.GetDouble() : 0m,
                root.TryGetProperty("county_rate", out var cr) ? (decimal)cr.GetDouble() : 0m,
                root.TryGetProperty("city_rate", out var cir) ? (decimal)cir.GetDouble() : 0m,
                root.TryGetProperty("special_rates", out var spr) ? (decimal)spr.GetDouble() : 0m,
                root.TryGetProperty("composite_tax_rate", out var comp) ? (decimal)comp.GetDouble() : 0m);
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Failed to run Python tax script.");
            return null;
        }
    }

    private string ResolvePythonScriptPath()
    {
        if (!string.IsNullOrWhiteSpace(_options.PythonScriptPath))
        {
            var p = _options.PythonScriptPath;
            if (!Path.IsPathRooted(p))
                p = Path.Combine(Directory.GetCurrentDirectory(), p);
            return p;
        }
        var cwd = Directory.GetCurrentDirectory();
        var baseDir = AppContext.BaseDirectory;
        var candidates = new[]
        {
            Path.GetFullPath(Path.Combine(cwd, "..", "..", "data-pipeline", "calculate_tax.py")),
            Path.GetFullPath(Path.Combine(cwd, "data-pipeline", "calculate_tax.py")),
            Path.GetFullPath(Path.Combine(baseDir, "..", "..", "..", "..", "data-pipeline", "calculate_tax.py"))
        };
        return candidates.FirstOrDefault(File.Exists) ?? "";
    }

    private async Task<TaxLookupResult?> GetTaxFromCSharpAsync(double latitude, double longitude, CancellationToken cancellationToken)
    {
        if (!_options.UseReverseGeocoding || _httpClientFactory is null)
        {
            _logger.LogDebug("Reverse geocoding disabled or no HttpClient. Using NY state-only fallback.");
            return GetTaxForAddress("New York State only");
        }

        try
        {
            var httpClient = _httpClientFactory.CreateClient("Nominatim");
            var url = $"reverse?lat={latitude.ToString("F6", CultureInfo.InvariantCulture)}&lon={longitude.ToString("F6", CultureInfo.InvariantCulture)}&format=json&addressdetails=1";
            var response = await httpClient.GetAsync(url, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogDebug("Nominatim returned {StatusCode} for {Lat},{Lon}. Using NY state-only fallback.", response.StatusCode, latitude, longitude);
                return GetTaxForAddress("New York State only");
            }

            var json = await response.Content.ReadAsStringAsync(cancellationToken);
            var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            string? county = null;
            string? city = null;

            if (root.TryGetProperty("address", out var addr))
            {
                county = GetAddressComponent(addr, "county", "state_district", "state");
                city = GetAddressComponent(addr, "city", "town", "village", "municipality");
            }

            if (string.IsNullOrWhiteSpace(county))
            {
                _logger.LogDebug("Reverse geocoding returned no county for {Lat},{Lon}. Using NY state-only.", latitude, longitude);
                return GetTaxForAddress("New York State only");
            }

            var result = GetTaxForAddress(county, city);
            if (result is not null)
                _logger.LogDebug("Resolved tax for {Lat},{Lon} -> {County}, {City}", latitude, longitude, county, city ?? "(county-level)");
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Reverse geocoding failed for {Lat},{Lon}. Using NY state-only fallback.", latitude, longitude);
            return GetTaxForAddress("New York State only");
        }
    }

    private static string? GetAddressComponent(JsonElement addr, params string[] keys)
    {
        foreach (var key in keys)
        {
            if (addr.TryGetProperty(key, out var val))
            {
                var s = val.GetString()?.Trim();
                if (!string.IsNullOrEmpty(s)) return s;
            }
        }
        return null;
    }

    private TaxRateRow? FindRate(string state, string county, string city)
    {
        var rows = _rates.Value;
        var countyMatch = rows.Where(r =>
            string.Equals(r.State, state, StringComparison.OrdinalIgnoreCase) &&
            string.Equals(r.County, county, StringComparison.OrdinalIgnoreCase));

        var exact = countyMatch.FirstOrDefault(r => string.Equals(r.City, city, StringComparison.OrdinalIgnoreCase));
        if (exact is not null) return exact;

        if (city != "0")
        {
            var countyLevel = countyMatch.FirstOrDefault(r => r.City == "0");
            if (countyLevel is not null) return countyLevel;
        }

        return countyMatch.FirstOrDefault();
    }

    private static string NormalizeCounty(string county)
    {
        if (string.IsNullOrWhiteSpace(county)) return "New York State only";

        var c = county.Trim();
        c = c.Replace(" County", "", StringComparison.OrdinalIgnoreCase);

        var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["Kings"] = "Kings (Brooklyn)",
            ["Brooklyn"] = "Brooklyn",
            ["New York"] = "New York (Manhattan)",
            ["Manhattan"] = "Manhattan",
            ["Richmond"] = "Richmond (Staten Island)",
            ["Staten Island"] = "Staten Island"
        };

        return map.TryGetValue(c, out var mapped) ? mapped : c;
    }

    private static TaxLookupResult MapToResult(TaxRateRow row)
    {
        var specialRates = ParseSpecialRates(row.SpecialRates);
        var specialJurisdiction = string.IsNullOrEmpty(row.SpecialDistricts) || row.SpecialDistricts == "[]"
            ? null
            : row.SpecialDistricts.Trim('"');

        return new TaxLookupResult(
            row.State ?? "NY",
            row.County ?? "",
            row.City == "0" ? "" : (row.City ?? ""),
            specialJurisdiction,
            decimal.Parse(row.StateRate ?? "0", CultureInfo.InvariantCulture),
            decimal.Parse(row.CountyRate ?? "0", CultureInfo.InvariantCulture),
            decimal.Parse(row.CityRate ?? "0", CultureInfo.InvariantCulture),
            specialRates,
            decimal.Parse(row.CompositeTaxRate ?? "0", CultureInfo.InvariantCulture));
    }

    private static decimal ParseSpecialRates(string? json)
    {
        if (string.IsNullOrWhiteSpace(json) || json == "[]") return 0;
        try
        {
            var arr = JsonSerializer.Deserialize<JsonElement>(json);
            if (arr.ValueKind != JsonValueKind.Array) return 0;
            decimal sum = 0;
            foreach (var item in arr.EnumerateArray())
            {
                if (item.TryGetProperty("rate", out var rate))
                    sum += rate.GetDecimal();
            }
            return sum;
        }
        catch
        {
            return 0;
        }
    }

    private class TaxRateRow
    {
        [Name("state")] public string? State { get; set; }
        [Name("county")] public string? County { get; set; }
        [Name("city")] public string? City { get; set; }
        [Name("special_districts")] public string? SpecialDistricts { get; set; }
        [Name("composite_tax_rate")] public string? CompositeTaxRate { get; set; }
        [Name("state_rate")] public string? StateRate { get; set; }
        [Name("county_rate")] public string? CountyRate { get; set; }
        [Name("city_rate")] public string? CityRate { get; set; }
        [Name("special_rates")] public string? SpecialRates { get; set; }
    }
}
