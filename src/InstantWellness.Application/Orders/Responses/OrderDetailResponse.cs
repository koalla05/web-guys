namespace InstantWellness.Application.Orders.Responses;

public record OrderDetailResponse(
    Guid Id,
    double Latitude,
    double Longitude,
    decimal Subtotal,
    DateTime Timestamp,
    TaxRateBreakdown TaxRateBreakdown,
    AmountCalculation AmountCalculation,
    AppliedJurisdictions AppliedJurisdictions);

public record TaxRateBreakdown(
    decimal StateRate,
    decimal CountyRate,
    decimal CityRate,
    decimal SpecialRates,
    decimal CompositeTaxRate);

public record AmountCalculation(
    decimal Subtotal,
    decimal TaxAmount,
    decimal TotalAmount);

public record AppliedJurisdictions(
    string? State,
    string? County,
    string? City,
    string? Special);
