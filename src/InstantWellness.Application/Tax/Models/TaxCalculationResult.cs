namespace InstantWellness.Application.Tax.Models;

public class TaxCalculationResult
{
    public decimal CompositeTaxRate { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal StateRate { get; set; }
    public decimal CountyRate { get; set; }
    public decimal CityRate { get; set; }
    public decimal SpecialRates { get; set; }
    public List<string> Jurisdictions { get; set; } = new();
    public string? State { get; set; }
    public string? County { get; set; }
    public string? City { get; set; }
}
