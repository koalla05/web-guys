using InstantWellness.Application.Orders.Queries;
using InstantWellness.Application.Orders.Responses;
using InstantWellness.Domain;
using InstantWellness.Domain.Interfaces;
using MediatR;

namespace InstantWellness.Application.Orders.Handlers;

public class GetOrderByIdHandler : IRequestHandler<GetOrderByIdQuery, OrderDetailResponse?>
{
    private readonly IOrderRepository _orderRepository;

    public GetOrderByIdHandler(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<OrderDetailResponse?> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdAsync(request.Id, cancellationToken);
        if (order == null)
            return null;

        return MapToDetailResponse(order);
    }

    private static OrderDetailResponse MapToDetailResponse(Order order) => new(
        order.Id,
        order.Latitude,
        order.Longitude,
        order.Subtotal,
        order.Timestamp,
        new TaxRateBreakdown(
            order.StateRate,
            order.CountyRate,
            order.CityRate,
            order.SpecialRates,
            order.CompositeTaxRate),
        new AmountCalculation(
            order.Subtotal,
            order.TaxAmount,
            order.TotalAmount),
        new AppliedJurisdictions(
            order.State,
            order.County,
            order.City,
            order.SpecialJurisdiction));
}
