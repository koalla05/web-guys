using InstantWellness.Application.Geocoding;
using InstantWellness.Application.Tax;
using Microsoft.AspNetCore.Mvc;

namespace InstantWellness.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class DebugController : ControllerBase
{
    private readonly IGeocodingService _geocodingService;
    private readonly ITaxService _taxService;

    public DebugController(IGeocodingService geocodingService, ITaxService taxService)
    {
        _geocodingService = geocodingService;
        _taxService = taxService;
    }

    [HttpGet("geocode")]
    public async Task<IActionResult> TestGeocode(
        [FromQuery] double lat,
        [FromQuery] double lon,
        CancellationToken ct)
    {
        var location = await _geocodingService.ReverseGeocodeAsync(lat, lon, ct);
        return Ok(location);
    }

    [HttpGet("tax-rate")]
    public IActionResult TestTaxRate(
        [FromQuery] string state,
        [FromQuery] string county,
        [FromQuery] string? city = null)
    {
        var taxRate = _taxService.GetTaxRate(state, county, city);
        return Ok(taxRate);
    }

    [HttpGet("tax-calc")]
    public async Task<IActionResult> TestTaxCalculation(
        [FromQuery] double lat,
        [FromQuery] double lon,
        [FromQuery] decimal subtotal,
        CancellationToken ct)
    {
        var result = await _taxService.CalculateTaxAsync(lat, lon, subtotal, ct);
        return Ok(result);
    }
}
