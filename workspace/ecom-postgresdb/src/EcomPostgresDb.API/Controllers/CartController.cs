using EcomPostgresDb.Application.Features.Cart.Commands;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EcomPostgresDb.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class CartController(IMediator mediator) : ControllerBase
{
    private Guid CurrentCustomerId =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? Guid.Empty.ToString());

    /// <summary>Add item to cart.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    public async Task<IActionResult> AddToCart([FromBody] AddToCartCommand command, CancellationToken ct)
    {
        var cmd = command with { CustomerId = CurrentCustomerId };
        var result = await mediator.Send(cmd, ct);
        if (!result.IsSuccess) return BadRequest(new { error = result.Error });
        return StatusCode(StatusCodes.Status201Created, new { cartItemId = result.Value });
    }
}
