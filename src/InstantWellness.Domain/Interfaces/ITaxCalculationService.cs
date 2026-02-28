namespace InstantWellness.Domain.Interfaces;

public record TaxLookupResult(
    string State,
    string County,
    string City,
    string? SpecialJurisdiction,
    decimal StateRate,
    decimal CountyRate,
    decimal CityRate,
    decimal SpecialRates,
    decimal CompositeTaxRate);

public interface ITaxCalculationService
{
    /// <summary>
    /// Gets tax rate for a location by latitude/longitude (reverse geocodes to county/city).
    /// </summary>
    Task<TaxLookupResult?> GetTaxForCoordinatesAsync(double latitude, double longitude, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets tax rate for a location by county and optional city (direct lookup in tax_rates.csv).
    /// </summary>
    TaxLookupResult? GetTaxForAddress(string county, string? city = null);
}
