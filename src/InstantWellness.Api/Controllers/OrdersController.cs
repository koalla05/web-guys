using InstantWellness.Application.Orders.Commands;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace InstantWellness.Api.Controllers;

[ApiController]
[Route("orders")]
public class OrdersController : ControllerBase
{
    private readonly IMediator _mediator;

    public OrdersController(IMediator mediator) => _mediator = mediator;

    /// <summary>
    /// Import orders from a CSV file. Use the "file" field to upload.
    /// </summary>
    [HttpPost("import")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(InstantWellness.Application.Orders.Responses.ImportOrdersResult), StatusCodes.Status200OK)]
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
}
