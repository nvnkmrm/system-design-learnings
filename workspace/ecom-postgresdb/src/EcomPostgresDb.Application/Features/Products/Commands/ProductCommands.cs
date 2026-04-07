using EcomPostgresDb.Application.Common.Models;
using EcomPostgresDb.Domain.Entities;
using EcomPostgresDb.Domain.Interfaces;
using EcomPostgresDb.Domain.ValueObjects;
using FluentValidation;
using MediatR;

namespace EcomPostgresDb.Application.Features.Products.Commands;

// ─── Create Product ──────────────────────────────────────────────────────────

public record CreateProductCommand(
    string Name,
    string Slug,
    string Sku,
    decimal Price,
    string Currency,
    Guid CategoryId,
    string? Description,
    Dictionary<string, string>? Attributes,
    int InitialStock) : IRequest<Result<Guid>>;

public sealed class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Slug).NotEmpty().MaximumLength(220).Matches("^[a-z0-9-]+$");
        RuleFor(x => x.Sku).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Price).GreaterThan(0);
        RuleFor(x => x.Currency).NotEmpty().Length(3);
        RuleFor(x => x.CategoryId).NotEmpty();
        RuleFor(x => x.InitialStock).GreaterThanOrEqualTo(0);
    }
}

public sealed class CreateProductCommandHandler(IUnitOfWork uow)
    : IRequestHandler<CreateProductCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateProductCommand cmd, CancellationToken ct)
    {
        var existing = await uow.Products.GetBySlugAsync(cmd.Slug, ct);
        if (existing is not null)
            return Result.Failure<Guid>($"A product with slug '{cmd.Slug}' already exists.");

        var money = new Money(cmd.Price, cmd.Currency);
        var product = Product.Create(cmd.Name, cmd.Slug, cmd.Sku, money, cmd.CategoryId,
            cmd.Description, cmd.Attributes);

        if (cmd.InitialStock > 0)
            product.AdjustStock(cmd.InitialStock);

        await uow.Products.AddAsync(product, ct);
        await uow.SaveChangesAsync(ct);
        return Result.Success(product.Id);
    }
}

// ─── Update Product ───────────────────────────────────────────────────────────

public record UpdateProductCommand(
    Guid Id,
    string Name,
    string Slug,
    decimal Price,
    string Currency,
    string? Description,
    Dictionary<string, string>? Attributes) : IRequest<Result<bool>>;

public sealed class UpdateProductCommandHandler(IUnitOfWork uow)
    : IRequestHandler<UpdateProductCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(UpdateProductCommand cmd, CancellationToken ct)
    {
        var product = await uow.Products.GetByIdAsync(cmd.Id, ct);
        if (product is null) return Result.Failure<bool>("Product not found.");

        product.UpdatePrice(new Money(cmd.Price, cmd.Currency));
        if (cmd.Attributes is not null)
            product.SetAttributes(cmd.Attributes);

        uow.Products.Update(product);
        await uow.SaveChangesAsync(ct);
        return Result.Success(true);
    }
}

// ─── Adjust Stock ─────────────────────────────────────────────────────────────

public record AdjustStockCommand(Guid ProductId, int Delta) : IRequest<Result<int>>;

public sealed class AdjustStockCommandHandler(IUnitOfWork uow)
    : IRequestHandler<AdjustStockCommand, Result<int>>
{
    public async Task<Result<int>> Handle(AdjustStockCommand cmd, CancellationToken ct)
    {
        var product = await uow.Products.GetByIdAsync(cmd.ProductId, ct);
        if (product is null) return Result.Failure<int>("Product not found.");

        product.AdjustStock(cmd.Delta);
        uow.Products.Update(product);
        await uow.SaveChangesAsync(ct);
        return Result.Success(product.StockQuantity);
    }
}
