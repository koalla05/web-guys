using InstantWellness.Application.Orders.Queries;
using InstantWellness.Application.Orders.Responses;
using InstantWellness.Domain;
using InstantWellness.Domain.Interfaces;
using MediatR;

namespace InstantWellness.Application.Orders.Handlers;

public class GetOrdersHandler : IRequestHandler<GetOrdersQuery, PagedResult<OrderResponse>>
{
    private readonly IOrderRepository _orderRepository;

    public GetOrdersHandler(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<PagedResult<OrderResponse>> Handle(GetOrdersQuery request, CancellationToken cancellationToken)
    {
        var (items, totalCount) = await _orderRepository.GetPagedAsync(
            request.Page,
            request.PageSize,
            request.MinLat,
            request.MaxLat,
            request.MinLon,
            request.MaxLon,
            request.FromDate,
            request.ToDate,
            cancellationToken);

        var responses = items.Select(MapToResponse).ToList();
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
        order.CompositeTaxRate,
        order.TaxAmount,
        order.TotalAmount);
}
