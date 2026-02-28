namespace InstantWellness.Application.Tax.Models;

public class TaxRate
{
    public string State { get; set; } = string.Empty;
    public string County { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string ReportingCode { get; set; } = string.Empty;
    public List<string> SpecialDistricts { get; set; } = new();
    public decimal CompositeTaxRate { get; set; }
    public decimal StateRate { get; set; }
    public decimal CountyRate { get; set; }
    public decimal CityRate { get; set; }
    public List<SpecialRate> SpecialRates { get; set; } = new();
}

public class SpecialRate
{
    public string Name { get; set; } = string.Empty;
    public decimal Rate { get; set; }
}
