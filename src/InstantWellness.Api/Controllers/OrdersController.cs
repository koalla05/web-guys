using InstantWellness.Api.Requests;
using InstantWellness.Application.Orders.Commands;
using InstantWellness.Application.Orders.Queries;
using InstantWellness.Application.Orders.Responses;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace InstantWellness.Api.Controllers;

[ApiController]
[Route("orders")]
public class OrdersController : ControllerBase
{
    private readonly IMediator _mediator;

    public OrdersController(IMediator mediator) => _mediator = mediator;

    [HttpPost]
    [ProducesResponseType(typeof(OrderResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request, CancellationToken ct)
    {
        var command = new CreateOrderCommand(
            request.Latitude,
            request.Longitude,
            request.Subtotal,
            request.Timestamp);
        var result = await _mediator.Send(command, ct);
        return Created($"/orders/{result.Id}", result);
    }

    [HttpPost("import")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(ImportOrdersResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ImportOrders(IFormFile? file, CancellationToken ct)
    {
        if (file == null || file.Length == 0)
            return BadRequest("No CSV file provided.");

        if (!file.FileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
            return BadRequest("File must be a CSV");

        await using var stream = file.OpenReadStream();
        var command = new ImportOrdersCommand(stream);
        var result = await _mediator.Send(command, ct);
        return Ok(result);
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<OrderResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOrders(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? orderIdSearch = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        [FromQuery] string? county = null,
        [FromQuery] decimal? minAmount = null,
        [FromQuery] decimal? maxAmount = null,
        [FromQuery] decimal? minTaxRate = null,
        [FromQuery] decimal? maxTaxRate = null,
        [FromQuery] double? minLat = null,
        [FromQuery] double? maxLat = null,
        [FromQuery] double? minLon = null,
        [FromQuery] double? maxLon = null,
        CancellationToken ct = default)
    {
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 10;
        if (pageSize > 100) pageSize = 100;

        var query = new GetOrdersQuery(
            page, pageSize, orderIdSearch, fromDate, toDate,
            county, minAmount, maxAmount, minTaxRate, maxTaxRate,
            minLat, maxLat, minLon, maxLon);
        var result = await _mediator.Send(query, ct);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(OrderDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetOrderById(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetOrderByIdQuery(id), ct);
        if (result == null)
            return NotFound();
        return Ok(result);
    }
}
