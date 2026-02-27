namespace InstantWellness.Application.Orders.Responses;

public record OrderResponse(
    Guid Id,
    double Latitude,
    double Longitude,
    decimal Subtotal,
    DateTime Timestamp,
    decimal CompositeTaxRate,
    decimal TaxAmount,
    decimal TotalAmount);
