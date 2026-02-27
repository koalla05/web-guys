using InstantWellness.Application;
using InstantWellness.Application.Orders.Commands;
using InstantWellness.Application.Orders.Queries;
using InstantWellness.Infrastructure;
using MediatR;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();

builder.Services.AddApplication();
builder.Services.AddInfrastructure();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors();

app.MapControllers();

// POST /orders - Manual order creation
app.MapPost("/orders", async (CreateOrderRequest request, IMediator mediator, CancellationToken ct) =>
{
    var command = new CreateOrderCommand(
        request.Latitude,
        request.Longitude,
        request.Subtotal,
        request.Timestamp);
    var result = await mediator.Send(command, ct);
    return Results.Created($"/orders/{result.Id}", result);
})
.WithName("CreateOrder")
.WithOpenApi();

// POST /orders/import - CSV import (handled by OrdersController for proper Swagger file upload)

// GET /orders - List with pagination and filters
app.MapGet("/orders", async (
    IMediator mediator,
    int page = 1,
    int pageSize = 10,
    double? minLat = null,
    double? maxLat = null,
    double? minLon = null,
    double? maxLon = null,
    DateTime? fromDate = null,
    DateTime? toDate = null,
    CancellationToken ct = default) =>
{
    if (page < 1) page = 1;
    if (pageSize < 1) pageSize = 10;
    if (pageSize > 100) pageSize = 100;

    var query = new GetOrdersQuery(page, pageSize, minLat, maxLat, minLon, maxLon, fromDate, toDate);
    var result = await mediator.Send(query, ct);
    return Results.Ok(result);
})
.WithName("GetOrders")
.WithOpenApi();

app.Run();

// Request DTOs
public record CreateOrderRequest(
    double Latitude,
    double Longitude,
    decimal Subtotal,
    DateTime? Timestamp = null);
