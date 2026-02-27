using InstantWellness.Application.Orders.Responses;
using MediatR;

namespace InstantWellness.Application.Orders.Queries;

public record GetOrderByIdQuery(Guid Id) : IRequest<OrderDetailResponse?>;
