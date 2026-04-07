using EcomPostgresDb.Application.Features.Orders.Commands;
using EcomPostgresDb.Application.Features.Orders.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EcomPostgresDb.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class OrdersController(IMediator mediator) : ControllerBase
{
    private Guid CurrentCustomerId =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? Guid.Empty.ToString());

    /// <summary>Place a new order (checkout).</summary>
    [HttpPost]
    [ProducesResponseType(typeof(object), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> PlaceOrder([FromBody] PlaceOrderCommand command, CancellationToken ct)
    {
        // Enforce customer can only order for themselves
        var cmd = command with { CustomerId = CurrentCustomerId };
        var result = await mediator.Send(cmd, ct);
        if (!result.IsSuccess) return BadRequest(new { error = result.Error });
        return CreatedAtAction(nameof(GetByOrderNumber),
            new { orderNumber = result.Value }, new { orderNumber = result.Value });
    }

    /// <summary>Get order details by order number.</summary>
    [HttpGet("{orderNumber}")]
    [ProducesResponseType(typeof(OrderDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByOrderNumber(string orderNumber, CancellationToken ct)
    {
        var result = await mediator.Send(new GetOrderByNumberQuery(orderNumber), ct);
        return result.IsSuccess ? Ok(result.Value) : NotFound(result.Error);
    }

    /// <summary>Get all orders for the current customer.</summary>
    [HttpGet("my")]
    [ProducesResponseType(typeof(IEnumerable<OrderSummaryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyOrders(CancellationToken ct)
    {
        var result = await mediator.Send(new GetCustomerOrdersQuery(CurrentCustomerId), ct);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }

    /// <summary>Cancel an order.</summary>
    [HttpPost("{id:guid}/cancel")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Cancel(Guid id, [FromQuery] string reason = "Cancelled by customer", CancellationToken ct = default)
    {
        var result = await mediator.Send(new CancelOrderCommand(id, reason), ct);
        return result.IsSuccess ? NoContent() : BadRequest(new { error = result.Error });
    }

    /// <summary>Ship an order (Admin only).</summary>
    [HttpPost("{id:guid}/ship")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Ship(Guid id, [FromQuery] string trackingNumber, CancellationToken ct)
    {
        var result = await mediator.Send(new ShipOrderCommand(id, trackingNumber), ct);
        return result.IsSuccess ? NoContent() : BadRequest(new { error = result.Error });
    }
}
