using EcomPostgresDb.Application.Common.Models;
using EcomPostgresDb.Domain.Entities;
using EcomPostgresDb.Domain.Interfaces;
using EcomPostgresDb.Domain.ValueObjects;
using FluentValidation;
using MediatR;

namespace EcomPostgresDb.Application.Features.Cart.Commands;

// ─── Add to Cart ──────────────────────────────────────────────────────────────

public record AddToCartCommand(
    Guid CustomerId,
    Guid ProductId,
    int Quantity,
    Guid? VariantId = null) : IRequest<Result<Guid>>;

public sealed class AddToCartCommandValidator : AbstractValidator<AddToCartCommand>
{
    public AddToCartCommandValidator()
    {
        RuleFor(x => x.CustomerId).NotEmpty();
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.Quantity).GreaterThan(0);
    }
}

public sealed class AddToCartCommandHandler(IUnitOfWork uow)
    : IRequestHandler<AddToCartCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(AddToCartCommand cmd, CancellationToken ct)
    {
        var product = await uow.Products.GetByIdAsync(cmd.ProductId, ct);
        if (product is null) return Result.Failure<Guid>("Product not found.");
        if (product.StockQuantity < cmd.Quantity) return Result.Failure<Guid>("Insufficient stock.");

        var customer = await uow.Customers.GetByIdAsync(cmd.CustomerId, ct);
        if (customer is null) return Result.Failure<Guid>("Customer not found.");

        // Determine price (variant override or product price)
        var unitPrice = product.Price;
        if (cmd.VariantId.HasValue)
        {
            var variant = product.Variants.FirstOrDefault(v => v.Id == cmd.VariantId.Value);
            if (variant?.PriceOverride is not null) unitPrice = variant.PriceOverride;
        }

        var cartItem = CartItem.Create(
            cmd.CustomerId, cmd.ProductId, product.Name,
            product.Sku, unitPrice, cmd.Quantity, cmd.VariantId);

        // Use DbContext directly via UoW for cart (no dedicated repo needed)
        var db = uow as ICartDirectAccess;
        if (db is not null) await db.AddCartItemAsync(cartItem, ct);

        await uow.SaveChangesAsync(ct);
        return Result.Success(cartItem.Id);
    }
}

// ─── Remove from Cart ─────────────────────────────────────────────────────────

public record RemoveFromCartCommand(Guid CartItemId, Guid CustomerId) : IRequest<Result<bool>>;

/// <summary>
/// Internal interface so Application can add cart items via UoW
/// without a full cart repository.
/// </summary>
public interface ICartDirectAccess
{
    Task AddCartItemAsync(CartItem item, CancellationToken ct);
    Task<CartItem?> GetCartItemAsync(Guid cartItemId, CancellationToken ct);
    Task<IEnumerable<CartItem>> GetCartItemsByCustomerAsync(Guid customerId, CancellationToken ct);
    void RemoveCartItem(CartItem item);
}
