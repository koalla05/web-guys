using InstantWellness.Application.Orders.Responses;
using InstantWellness.Application.Tax;
using InstantWellness.Domain;
using InstantWellness.Domain.Interfaces;
using MediatR;

namespace InstantWellness.Application.Orders.Handlers;

public class CreateOrderHandler : IRequestHandler<Commands.CreateOrderCommand, OrderResponse>
{
    private readonly IOrderRepository _orderRepository;
    private readonly ITaxService _taxService;

    public CreateOrderHandler(IOrderRepository orderRepository, ITaxService taxService)
    {
        _orderRepository = orderRepository;
        _taxService = taxService;
    }

    public async Task<OrderResponse> Handle(Commands.CreateOrderCommand request, CancellationToken cancellationToken)
    {
        // Calculate tax based on location
        var taxCalculation = await _taxService.CalculateTaxAsync(
            request.Latitude, 
            request.Longitude, 
            request.Subtotal, 
            cancellationToken);

        var order = new Order
        {
            Id = Guid.NewGuid(),
            Latitude = request.Latitude,
            Longitude = request.Longitude,
            Subtotal = request.Subtotal,
            Timestamp = request.Timestamp ?? DateTime.UtcNow,
            CompositeTaxRate = taxCalculation.CompositeTaxRate,
            TaxAmount = taxCalculation.TaxAmount,
            StateRate = taxCalculation.StateRate,
            CountyRate = taxCalculation.CountyRate,
            CityRate = taxCalculation.CityRate,
            SpecialRates = taxCalculation.SpecialRates,
            State = taxCalculation.State,
            County = taxCalculation.County,
            City = taxCalculation.City,
            SpecialJurisdiction = string.Join(", ", taxCalculation.SpecialJurisdictions)
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
        order.CompositeTaxRate ?? 0,
        order.TaxAmount ?? 0,
        order.TotalAmount,
        order.County,
        order.StateRate ?? 0,
        order.CountyRate ?? 0,
        order.CityRate ?? 0,
        order.SpecialRates ?? 0,
        order.State,
        order.City,
        order.SpecialJurisdiction);
}
