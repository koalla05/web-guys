using InstantWellness.Application.Orders.Responses;
using InstantWellness.Domain;
using InstantWellness.Domain.Interfaces;
using MediatR;

namespace InstantWellness.Application.Orders.Handlers;

public class CreateOrderHandler : IRequestHandler<Commands.CreateOrderCommand, OrderResponse>
{
    private readonly IOrderRepository _orderRepository;

    public CreateOrderHandler(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<OrderResponse> Handle(Commands.CreateOrderCommand request, CancellationToken cancellationToken)
    {
        var order = new Order
        {
            Id = Guid.NewGuid(),
            Latitude = request.Latitude,
            Longitude = request.Longitude,
            Subtotal = request.Subtotal,
            Timestamp = request.Timestamp ?? DateTime.UtcNow,
            CompositeTaxRate = 0, // Tax calculation skipped for now
            TaxAmount = 0
        };

        var savedOrder = await _orderRepository.AddAsync(order, cancellationToken);
        return MapToResponse(savedOrder);
    }

    private static OrderResponse MapToResponse(Order order) => new(
        order.Id,
        order.Latitude,
        order.Longitude,
        order.Subtotal,
        order.Timestamp,
        order.CompositeTaxRate,
        order.TaxAmount,
        order.TotalAmount);
}
