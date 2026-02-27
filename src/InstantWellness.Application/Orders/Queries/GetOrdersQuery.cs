using InstantWellness.Application.Orders.Responses;
using MediatR;

namespace InstantWellness.Application.Orders.Queries;

public record GetOrdersQuery(
    int Page = 1,
    int PageSize = 10,
    double? MinLat = null,
    double? MaxLat = null,
    double? MinLon = null,
    double? MaxLon = null,
    DateTime? FromDate = null,
    DateTime? ToDate = null) : IRequest<PagedResult<OrderResponse>>;
