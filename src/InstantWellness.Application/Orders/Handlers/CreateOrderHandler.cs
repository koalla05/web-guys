using InstantWellness.Application.Orders.Responses;
using InstantWellness.Domain;
using InstantWellness.Domain.Interfaces;
using MediatR;

namespace InstantWellness.Application.Orders.Handlers;

public class CreateOrderHandler : IRequestHandler<Commands.CreateOrderCommand, OrderResponse>
{
    private readonly IOrderRepository _orderRepository;
    private readonly ITaxCalculationService _taxService;

    public CreateOrderHandler(IOrderRepository orderRepository, ITaxCalculationService taxService)
    {
        _orderRepository = orderRepository;
        _taxService = taxService;
    }

    public async Task<OrderResponse> Handle(Commands.CreateOrderCommand request, CancellationToken cancellationToken)
    {
        var taxResult = await _taxService.GetTaxForCoordinatesAsync(request.Latitude, request.Longitude, cancellationToken);
        var compositeRate = taxResult?.CompositeTaxRate ?? 0m;
        var taxAmount = request.Subtotal * compositeRate;

        var order = new Order
        {
            Id = Guid.NewGuid(),
            Latitude = request.Latitude,
            Longitude = request.Longitude,
            Subtotal = request.Subtotal,
            Timestamp = request.Timestamp ?? DateTime.UtcNow,
            CompositeTaxRate = compositeRate,
            TaxAmount = taxAmount,
            StateRate = taxResult?.StateRate ?? 0m,
            CountyRate = taxResult?.CountyRate ?? 0m,
            CityRate = taxResult?.CityRate ?? 0m,
            SpecialRates = taxResult?.SpecialRates ?? 0m,
            State = taxResult?.State,
            County = taxResult?.County,
            City = taxResult?.City,
            SpecialJurisdiction = taxResult?.SpecialJurisdiction
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
        order.TotalAmount,
        order.County);
}
