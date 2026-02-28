using InstantWellness.Application.Orders.Responses;
using MediatR;

namespace InstantWellness.Application.Orders.Queries;

public record GetOrdersQuery(
    int Page = 1,
    int PageSize = 10,
    string? OrderIdSearch = null,
    DateTime? FromDate = null,
    DateTime? ToDate = null,
    string? County = null,
    decimal? MinAmount = null,
    decimal? MaxAmount = null,
    decimal? MinTaxRate = null,
    decimal? MaxTaxRate = null,
    double? MinLat = null,
    double? MaxLat = null,
    double? MinLon = null,
    double? MaxLon = null) : IRequest<PagedResult<OrderResponse>>;
