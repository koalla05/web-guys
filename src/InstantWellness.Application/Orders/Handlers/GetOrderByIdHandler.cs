using InstantWellness.Application.Orders.Queries;
using InstantWellness.Application.Orders.Responses;
using InstantWellness.Application.Tax;
using InstantWellness.Domain;
using InstantWellness.Domain.Interfaces;
using MediatR;

namespace InstantWellness.Application.Orders.Handlers;

public class GetOrderByIdHandler : IRequestHandler<GetOrderByIdQuery, OrderDetailResponse?>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IOrderTaxCalculator _taxCalculator;

    public GetOrderByIdHandler(IOrderRepository orderRepository, IOrderTaxCalculator taxCalculator)
    {
        _orderRepository = orderRepository;
        _taxCalculator = taxCalculator;
    }

    public async Task<OrderDetailResponse?> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdAsync(request.Id, cancellationToken);
        if (order == null)
            return null;

        // Lazy evaluation: ensure tax is calculated before returning
        order = await _taxCalculator.EnsureTaxCalculatedAsync(order, cancellationToken);

        return MapToDetailResponse(order);
    }

    private static OrderDetailResponse MapToDetailResponse(Order order) => new(
        order.Id,
        order.Latitude,
        order.Longitude,
        order.Subtotal,
        order.Timestamp,
        new TaxRateBreakdown(
            order.StateRate ?? 0,
            order.CountyRate ?? 0,
            order.CityRate ?? 0,
            order.SpecialRates ?? 0,
            order.CompositeTaxRate ?? 0),
        new AmountCalculation(
            order.Subtotal,
            order.TaxAmount ?? 0,
            order.TotalAmount),
        new AppliedJurisdictions(
            order.State,
            order.County,
            order.City,
            order.SpecialJurisdiction));
}
