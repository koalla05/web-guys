using InstantWellness.Application.Orders.Responses;
using MediatR;

namespace InstantWellness.Application.Orders.Commands;

public record CreateOrderCommand(
    double Latitude,
    double Longitude,
    decimal Subtotal,
    DateTime? Timestamp = null) : IRequest<OrderResponse>;
