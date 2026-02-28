using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using InstantWellness.Application.Orders.Commands;
using InstantWellness.Application.Orders.Responses;
using InstantWellness.Domain;
using InstantWellness.Domain.Interfaces;
using MediatR;

namespace InstantWellness.Application.Orders.Handlers;

public class ImportOrdersHandler : IRequestHandler<ImportOrdersCommand, ImportOrdersResult>
{
    private readonly IOrderRepository _orderRepository;
    private readonly InstantWellness.Domain.Interfaces.ITaxCalculationService _taxService;

    public ImportOrdersHandler(IOrderRepository orderRepository, InstantWellness.Domain.Interfaces.ITaxCalculationService taxService)
    {
        _orderRepository = orderRepository;
        _taxService = taxService;
    }

    public async Task<ImportOrdersResult> Handle(ImportOrdersCommand request, CancellationToken cancellationToken)
    {
        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
            MissingFieldFound = null,
            HeaderValidated = null,
            BadDataFound = null
        };

        var errors = new List<string>();
        var ordersToImport = new List<Order>();
        var lineNumber = 1;

        try
        {
            using var reader = new StreamReader(request.CsvStream);
            using var csv = new CsvReader(reader, config);

            await csv.ReadAsync();
            csv.ReadHeader();
            var headers = csv.HeaderRecord ?? [];

            // Flexible column mapping - support various header names
            var latIndex = FindColumnIndex(headers, "latitude", "lat");
            var lonIndex = FindColumnIndex(headers, "longitude", "lon", "lng");
            var subtotalIndex = FindColumnIndex(headers, "subtotal", "amount", "price");
            var timestampIndex = FindColumnIndex(headers, "timestamp", "date", "datetime", "created_at");

            if (latIndex == -1 || lonIndex == -1 || subtotalIndex == -1)
            {
                return new ImportOrdersResult(0, 0, ["CSV must contain latitude, longitude, and subtotal columns"]);
            }

            while (await csv.ReadAsync())
            {
                lineNumber++;
                try
                {
                    var latStr = GetField(csv, headers, latIndex);
                    var lonStr = GetField(csv, headers, lonIndex);
                    var subtotalStr = GetField(csv, headers, subtotalIndex);
                    var timestampStr = timestampIndex >= 0 ? GetField(csv, headers, timestampIndex) : null;

                    if (string.IsNullOrWhiteSpace(latStr) || string.IsNullOrWhiteSpace(lonStr) || string.IsNullOrWhiteSpace(subtotalStr))
                    {
                        errors.Add($"Line {lineNumber}: Missing required fields");
                        continue;
                    }

                    var latitude = double.Parse(latStr.Trim(), CultureInfo.InvariantCulture);
                    var longitude = double.Parse(lonStr.Trim(), CultureInfo.InvariantCulture);
                    var subtotal = decimal.Parse(subtotalStr.Trim(), CultureInfo.InvariantCulture);
                    var timestamp = ParseTimestamp(timestampStr);

                    var taxResult = await _taxService.GetTaxForCoordinatesAsync(latitude, longitude, cancellationToken);
                    var compositeRate = taxResult?.CompositeTaxRate ?? 0m;
                    var taxAmount = subtotal * compositeRate;

                    var order = new Order
                    {
                        Id = Guid.NewGuid(),
                        Latitude = latitude,
                        Longitude = longitude,
                        Subtotal = subtotal,
                        Timestamp = timestamp,
                        CompositeTaxRate = compositeRate,
                        TaxAmount = taxAmount,
                        StateRate = taxResult?.StateRate ?? 0m,
                        CountyRate = taxResult?.CountyRate ?? 0m,
                        CityRate = taxResult?.CityRate ?? 0m,
                        SpecialRates = taxResult?.SpecialRates ?? 0m,
                        State = taxResult?.State,
                        County = taxResult?.County,
                        City = taxResult?.City,
                        SpecialJurisdiction = taxResult?.SpecialJurisdiction
                    };

                    ordersToImport.Add(order);
                }
                catch (Exception ex)
                {
                    errors.Add($"Line {lineNumber}: {ex.Message}");
                }
            }

            if (ordersToImport.Count > 0)
            {
                await _orderRepository.AddRangeAsync(ordersToImport, cancellationToken);
            }

            var failedCount = errors.Count;
            return new ImportOrdersResult(ordersToImport.Count, failedCount, errors);
        }
        catch (Exception ex)
        {
            errors.Insert(0, $"CSV parsing failed: {ex.Message}");
            return new ImportOrdersResult(0, lineNumber, errors);
        }
    }

    private static int FindColumnIndex(string[] headers, params string[] names)
    {
        for (var i = 0; i < headers.Length; i++)
        {
            var h = headers[i].Trim().ToLowerInvariant().Replace(" ", "_").Replace("-", "_");
            if (names.Any(n => h.Contains(n, StringComparison.OrdinalIgnoreCase)))
                return i;
        }
        return -1;
    }

    private static string GetField(CsvReader csv, string[] headers, int index)
    {
        if (index < 0 || index >= headers.Length) return string.Empty;
        return csv.GetField(index) ?? string.Empty;
    }

    private static DateTime ParseTimestamp(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return DateTime.UtcNow;
        if (DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt))
            return dt.Kind == DateTimeKind.Unspecified ? DateTime.SpecifyKind(dt, DateTimeKind.Utc) : dt.ToUniversalTime();
        return DateTime.UtcNow;
    }
}
