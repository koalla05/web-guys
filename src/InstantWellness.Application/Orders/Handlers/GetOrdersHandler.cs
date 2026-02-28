using InstantWellness.Application.Orders.Queries;
using InstantWellness.Application.Orders.Responses;
using InstantWellness.Application.Tax;
using InstantWellness.Domain;
using InstantWellness.Domain.Interfaces;
using MediatR;

namespace InstantWellness.Application.Orders.Handlers;

public class GetOrdersHandler : IRequestHandler<GetOrdersQuery, PagedResult<OrderResponse>>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IOrderTaxCalculator _taxCalculator;

    public GetOrdersHandler(IOrderRepository orderRepository, IOrderTaxCalculator taxCalculator)
    {
        _orderRepository = orderRepository;
        _taxCalculator = taxCalculator;
    }

    public async Task<PagedResult<OrderResponse>> Handle(GetOrdersQuery request, CancellationToken cancellationToken)
    {
        var (items, totalCount) = await _orderRepository.GetPagedAsync(
            request.Page,
            request.PageSize,
            request.OrderIdSearch,
            request.FromDate,
            request.ToDate,
            request.County,
            request.MinAmount,
            request.MaxAmount,
            request.MinTaxRate,
            request.MaxTaxRate,
            request.MinLat,
            request.MaxLat,
            request.MinLon,
            request.MaxLon,
            cancellationToken);

        // Lazy evaluation: ensure all orders have tax calculated
        var ordersList = items.ToList();
        await _taxCalculator.EnsureTaxCalculatedAsync(ordersList, cancellationToken);

        var responses = ordersList.Select(MapToResponse).ToList();
        var totalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize);

        return new PagedResult<OrderResponse>(
            responses,
            totalCount,
            request.Page,
            request.PageSize,
            totalPages);
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
