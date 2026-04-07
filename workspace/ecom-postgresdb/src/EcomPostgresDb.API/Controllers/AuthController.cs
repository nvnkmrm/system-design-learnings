using EcomPostgresDb.Application.Features.Customers.Commands;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace EcomPostgresDb.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class AuthController(IMediator mediator) : ControllerBase
{
    /// <summary>Register a new customer account.</summary>
    [HttpPost("register")]
    [ProducesResponseType(typeof(object), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register([FromBody] RegisterCustomerCommand command, CancellationToken ct)
    {
        var result = await mediator.Send(command, ct);
        if (!result.IsSuccess) return Conflict(new { error = result.Error });
        return CreatedAtAction(nameof(Register), new { id = result.Value });
    }

    /// <summary>Authenticate and receive a JWT token.</summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginCustomerCommand command, CancellationToken ct)
    {
        var result = await mediator.Send(command, ct);
        if (!result.IsSuccess) return Unauthorized(new { error = result.Error });
        return Ok(new { token = result.Value });
    }
}
