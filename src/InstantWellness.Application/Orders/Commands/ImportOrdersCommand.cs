using InstantWellness.Application.Orders.Responses;
using MediatR;

namespace InstantWellness.Application.Orders.Commands;

public record ImportOrdersCommand(Stream CsvStream) : IRequest<ImportOrdersResult>;
