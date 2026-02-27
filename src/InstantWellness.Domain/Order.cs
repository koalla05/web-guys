namespace InstantWellness.Domain;

public class Order
{
    public Guid Id { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public decimal Subtotal { get; set; }
    public DateTime Timestamp { get; set; }

    public decimal CompositeTaxRate { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount => Subtotal + TaxAmount;

    public decimal StateRate { get; set; }
    public decimal LocalRate { get; set; }      // county_rate + city_rate
    public decimal SpecialRates { get; set; }
    public string? State { get; set; }
    public string? County { get; set; }
    public string? City { get; set; }
    public string? SpecialJurisdiction { get; set; }
}
