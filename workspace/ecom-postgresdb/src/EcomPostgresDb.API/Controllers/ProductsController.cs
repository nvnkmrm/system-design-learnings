using EcomPostgresDb.Application.Common.Models;
using EcomPostgresDb.Application.Features.Products.Commands;
using EcomPostgresDb.Application.Features.Products.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EcomPostgresDb.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class ProductsController(IMediator mediator) : ControllerBase
{
    /// <summary>Search products using PostgreSQL full-text search.</summary>
    [HttpGet("search")]
    [ProducesResponseType(typeof(PagedResult<ProductSummaryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Search(
        [FromQuery] string term,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await mediator.Send(new SearchProductsQuery(term, page, pageSize), ct);
        return result.IsSuccess ? Ok(result.Value) : NotFound(result.Error);
    }

    /// <summary>Get product details by slug.</summary>
    [HttpGet("{slug}")]
    [ProducesResponseType(typeof(ProductDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetBySlug(string slug, CancellationToken ct)
    {
        var result = await mediator.Send(new GetProductBySlugQuery(slug), ct);
        return result.IsSuccess ? Ok(result.Value) : NotFound(result.Error);
    }

    /// <summary>Get products by category.</summary>
    [HttpGet("category/{categoryId:guid}")]
    [ProducesResponseType(typeof(PagedResult<ProductSummaryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByCategory(
        Guid categoryId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await mediator.Send(new GetProductsByCategoryQuery(categoryId, page, pageSize), ct);
        return result.IsSuccess ? Ok(result.Value) : NotFound(result.Error);
    }

    /// <summary>Create a new product. (Admin only)</summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] CreateProductCommand command, CancellationToken ct)
    {
        var result = await mediator.Send(command, ct);
        if (!result.IsSuccess) return Conflict(result.Error);
        return CreatedAtAction(nameof(GetBySlug), new { slug = command.Slug }, result.Value);
    }

    /// <summary>Update product price and attributes. (Admin only)</summary>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateProductCommand command, CancellationToken ct)
    {
        if (id != command.Id) return BadRequest("Route id does not match body id.");
        var result = await mediator.Send(command, ct);
        return result.IsSuccess ? NoContent() : NotFound(result.Error);
    }

    /// <summary>Adjust product stock quantity. (Admin only)</summary>
    [HttpPatch("{id:guid}/stock")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
    public async Task<IActionResult> AdjustStock(Guid id, [FromQuery] int delta, CancellationToken ct)
    {
        var result = await mediator.Send(new AdjustStockCommand(id, delta), ct);
        return result.IsSuccess ? Ok(new { stockQuantity = result.Value }) : NotFound(result.Error);
    }
}
