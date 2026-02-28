using System.Globalization;
using System.Text.Json;
using CsvHelper;
using CsvHelper.Configuration;
using InstantWellness.Application.Geocoding;
using InstantWellness.Application.Tax;
using InstantWellness.Application.Tax.Models;

namespace InstantWellness.Infrastructure.Tax;

public class TaxService : ITaxService
{
    private readonly List<TaxRate> _taxRates;
    private readonly IGeocodingService _geocodingService;

    public TaxService(IGeocodingService geocodingService, string csvFilePath)
    {
        _geocodingService = geocodingService;
        _taxRates = LoadTaxRates(csvFilePath);
    }

    public async Task<TaxCalculationResult> CalculateTaxAsync(
        double latitude, 
        double longitude, 
        decimal subtotal, 
        CancellationToken cancellationToken = default)
    {
        var locationInfo = await _geocodingService.ReverseGeocodeAsync(latitude, longitude, cancellationToken);

        if (locationInfo == null || 
            string.IsNullOrWhiteSpace(locationInfo.State) ||
            NormalizeStateName(locationInfo.State) != "ny")
        {
            // Default to NY state-only rate if location not found or not in NY
            return CreateDefaultTaxCalculation(subtotal);
        }

        var taxRate = GetTaxRate(locationInfo.State, locationInfo.County ?? "", locationInfo.City);

        if (taxRate == null)
        {
            return CreateDefaultTaxCalculation(subtotal);
        }

        var taxAmount = subtotal * taxRate.CompositeTaxRate;
        var totalAmount = subtotal + taxAmount;

        var specialRatesSum = taxRate.SpecialRates.Sum(sr => sr.Rate);

        var jurisdictions = new List<string>();
        if (taxRate.StateRate > 0) jurisdictions.Add($"New York State ({taxRate.StateRate:P2})");
        if (taxRate.CountyRate > 0) jurisdictions.Add($"{taxRate.County} County ({taxRate.CountyRate:P2})");
        if (taxRate.CityRate > 0 && !string.IsNullOrEmpty(taxRate.City) && taxRate.City != "0")
        {
            jurisdictions.Add($"{taxRate.City} City ({taxRate.CityRate:P2})");
        }
        foreach (var specialRate in taxRate.SpecialRates)
        {
            jurisdictions.Add($"{specialRate.Name} ({specialRate.Rate:P2})");
        }

        return new TaxCalculationResult
        {
            CompositeTaxRate = taxRate.CompositeTaxRate,
            TaxAmount = Math.Round(taxAmount, 2),
            TotalAmount = Math.Round(totalAmount, 2),
            StateRate = taxRate.StateRate,
            CountyRate = taxRate.CountyRate,
            CityRate = taxRate.CityRate,
            SpecialRates = specialRatesSum,
            Jurisdictions = jurisdictions,
            SpecialJurisdictions = taxRate.SpecialRates.Select(sr => $"{sr.Name} ({sr.Rate:P2})").ToList(),
            State = taxRate.State,
            County = taxRate.County,
            City = !string.IsNullOrEmpty(taxRate.City) && taxRate.City != "0" ? taxRate.City : null
        };
    }

    public TaxRate? GetTaxRate(string state, string county, string? city)
    {
        if (string.IsNullOrWhiteSpace(state) || string.IsNullOrWhiteSpace(county))
        {
            return null;
        }

        // Normalize inputs
        state = NormalizeStateName(state);
        county = NormalizeString(county);
        city = NormalizeCityName(city);

        // First, try to find exact city match within the county
        if (!string.IsNullOrEmpty(city))
        {
            var cityRate = _taxRates.FirstOrDefault(tr =>
                NormalizeStateName(tr.State) == state &&
                CountyMatches(tr.County, county) &&
                !string.IsNullOrEmpty(tr.City) &&
                tr.City != "0" &&
                NormalizeCityName(tr.City) == city);

            if (cityRate != null)
            {
                return cityRate;
            }
        }

        // If no city match, return county-level rate (city = "0")
        var countyRate = _taxRates.FirstOrDefault(tr =>
            NormalizeStateName(tr.State) == state &&
            CountyMatches(tr.County, county) &&
            (tr.City == "0" || string.IsNullOrEmpty(tr.City)));

        return countyRate;
    }

    private static bool CountyMatches(string csvCounty, string apiCounty)
    {
        var normalizedCSV = NormalizeString(csvCounty);
        var normalizedAPI = NormalizeString(apiCounty);

        // Exact match
        if (normalizedCSV == normalizedAPI)
            return true;

        // Handle cases like "Kings (Brooklyn)" matching "Kings"
        // Extract the main county name before parentheses
        var parenIndex = normalizedCSV.IndexOf('(');
        if (parenIndex > 0)
        {
            var mainCountyName = normalizedCSV.Substring(0, parenIndex).Trim();
            if (mainCountyName == normalizedAPI)
                return true;
        }

        // Handle reverse: API might return "Kings (Brooklyn)" and CSV has "Kings"
        var apiParenIndex = normalizedAPI.IndexOf('(');
        if (apiParenIndex > 0)
        {
            var mainAPICountyName = normalizedAPI.Substring(0, apiParenIndex).Trim();
            if (mainAPICountyName == normalizedCSV)
                return true;
        }

        return false;
    }

    private static TaxCalculationResult CreateDefaultTaxCalculation(decimal subtotal)
    {
        // Default to NY state-only rate (4%)
        const decimal defaultStateRate = 0.04m;
        var taxAmount = subtotal * defaultStateRate;
        var totalAmount = subtotal + taxAmount;

        return new TaxCalculationResult
        {
            CompositeTaxRate = defaultStateRate,
            TaxAmount = Math.Round(taxAmount, 2),
            TotalAmount = Math.Round(totalAmount, 2),
            StateRate = defaultStateRate,
            CountyRate = 0,
            CityRate = 0,
            SpecialRates = 0,
            Jurisdictions = new List<string> { "New York State (4.00%)" },
            SpecialJurisdictions = new List<string>(),
            State = "NY",
            County = null,
            City = null
        };
    }

    private static List<TaxRate> LoadTaxRates(string csvFilePath)
    {
        var taxRates = new List<TaxRate>();

        using var reader = new StreamReader(csvFilePath);
        using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
        });

        csv.Read();
        csv.ReadHeader();

        while (csv.Read())
        {
            var specialDistrictsJson = csv.GetField<string>("special_districts") ?? "[]";
            var specialRatesJson = csv.GetField<string>("special_rates") ?? "[]";

            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var specialDistricts = JsonSerializer.Deserialize<List<string>>(specialDistrictsJson, jsonOptions) ?? new List<string>();
            var specialRates = JsonSerializer.Deserialize<List<SpecialRate>>(specialRatesJson, jsonOptions) ?? new List<SpecialRate>();

            var taxRate = new TaxRate
            {
                State = csv.GetField<string>("state") ?? "",
                County = csv.GetField<string>("county") ?? "",
                City = csv.GetField<string>("city") ?? "",
                ReportingCode = csv.GetField<string>("reporting_code") ?? "",
                SpecialDistricts = specialDistricts,
                CompositeTaxRate = csv.GetField<decimal>("composite_tax_rate"),
                StateRate = csv.GetField<decimal>("state_rate"),
                CountyRate = csv.GetField<decimal>("county_rate"),
                CityRate = csv.GetField<decimal>("city_rate"),
                SpecialRates = specialRates
            };

            taxRates.Add(taxRate);
        }

        return taxRates;
    }

    private static string NormalizeString(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;

        return input.Trim().ToLowerInvariant();
    }

    private static string NormalizeStateName(string? stateName)
    {
        if (string.IsNullOrWhiteSpace(stateName))
            return string.Empty;

        var normalized = stateName.Trim().ToLowerInvariant();

        // Map full state names to abbreviations
        return normalized switch
        {
            "new york" => "ny",
            "ny" => "ny",
            _ => normalized
        };
    }

    private static string NormalizeCityName(string? cityName)
    {
        if (string.IsNullOrWhiteSpace(cityName))
            return string.Empty;

        var normalized = cityName.Trim().ToLowerInvariant();

        // Remove common prefixes
        if (normalized.StartsWith("city of "))
        {
            normalized = normalized.Substring(8).Trim();
        }
        else if (normalized.StartsWith("town of "))
        {
            normalized = normalized.Substring(8).Trim();
        }
        else if (normalized.StartsWith("village of "))
        {
            normalized = normalized.Substring(11).Trim();
        }

        return normalized;
    }
}
