namespace InstantWellness.Application.Orders.Responses;

public record OrderResponse(
    Guid Id,
    double Latitude,
    double Longitude,
    decimal Subtotal,
    DateTime Timestamp,
    decimal CompositeTaxRate,
    decimal TaxAmount,
    decimal TotalAmount,
    string? County = null,
    decimal StateRate = 0,
    decimal CountyRate = 0,
    decimal CityRate = 0,
    decimal SpecialRates = 0,
    string? State = null,
    string? City = null,
    string? Jurisdictions = null);
