namespace InstantWellness.Domain;

public class Order
{
    public Guid Id { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public decimal Subtotal { get; set; }
    public DateTime Timestamp { get; set; }

    // Nullable tax fields - calculated lazily on demand
    public decimal? CompositeTaxRate { get; set; }
    public decimal? TaxAmount { get; set; }
    public decimal TotalAmount => Subtotal + (TaxAmount ?? 0);

    public decimal? StateRate { get; set; }
    public decimal? CountyRate { get; set; }
    public decimal? CityRate { get; set; }
    public decimal? SpecialRates { get; set; }
    public string? State { get; set; }
    public string? County { get; set; }
    public string? City { get; set; }
    public string? SpecialJurisdiction { get; set; }

    public bool HasTaxCalculated() => CompositeTaxRate.HasValue;
}
