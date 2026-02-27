namespace InstantWellness.Api.Requests;

public record CreateOrderRequest(
    double Latitude,
    double Longitude,
    decimal Subtotal,
    DateTime? Timestamp = null);
