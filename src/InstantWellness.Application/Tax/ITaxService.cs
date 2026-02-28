using InstantWellness.Application.Tax.Models;

namespace InstantWellness.Application.Tax;

public interface ITaxService
{
    Task<TaxCalculationResult> CalculateTaxAsync(double latitude, double longitude, decimal subtotal, CancellationToken cancellationToken = default);
    TaxRate? GetTaxRate(string state, string county, string? city);
}
