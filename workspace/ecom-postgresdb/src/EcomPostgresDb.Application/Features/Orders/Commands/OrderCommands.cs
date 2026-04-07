using EcomPostgresDb.Application.Common.Models;
using EcomPostgresDb.Domain.Entities;
using EcomPostgresDb.Domain.Enums;
using EcomPostgresDb.Domain.Interfaces;
using EcomPostgresDb.Domain.ValueObjects;
using FluentValidation;
using MediatR;

namespace EcomPostgresDb.Application.Features.Orders.Commands;

// ─── DTOs ─────────────────────────────────────────────────────────────────────

public record OrderItemRequest(Guid ProductId, int Quantity, Guid? VariantId = null);

public record AddressRequest(
    string Line1, string? Line2, string City,
    string State, string PostalCode, string Country);

// ─── Place Order ──────────────────────────────────────────────────────────────

public record PlaceOrderCommand(
    Guid CustomerId,
    AddressRequest ShippingAddress,
    AddressRequest BillingAddress,
    List<OrderItemRequest> Items,
    string Currency = "USD",
    string? CouponCode = null) : IRequest<Result<string>>;

public sealed class PlaceOrderCommandValidator : AbstractValidator<PlaceOrderCommand>
{
    public PlaceOrderCommandValidator()
    {
        RuleFor(x => x.CustomerId).NotEmpty();
        RuleFor(x => x.Items).NotEmpty().WithMessage("Order must have at least one item.");
        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.ProductId).NotEmpty();
            item.RuleFor(i => i.Quantity).GreaterThan(0);
        });
        RuleFor(x => x.ShippingAddress).NotNull();
        RuleFor(x => x.BillingAddress).NotNull();
    }
}

public sealed class PlaceOrderCommandHandler(IUnitOfWork uow)
    : IRequestHandler<PlaceOrderCommand, Result<string>>
{
    public async Task<Result<string>> Handle(PlaceOrderCommand cmd, CancellationToken ct)
    {
        var customer = await uow.Customers.GetByIdAsync(cmd.CustomerId, ct);
        if (customer is null) return Result.Failure<string>("Customer not found.");

        var sa = cmd.ShippingAddress;
        var ba = cmd.BillingAddress;
        var shippingAddr = new Address(sa.Line1, sa.Line2, sa.City, sa.State, sa.PostalCode, sa.Country);
        var billingAddr = new Address(ba.Line1, ba.Line2, ba.City, ba.State, ba.PostalCode, ba.Country);

        var order = Order.Create(cmd.CustomerId, shippingAddr, billingAddr, cmd.Currency);

        foreach (var item in cmd.Items)
        {
            var product = await uow.Products.GetByIdAsync(item.ProductId, ct);
            if (product is null) return Result.Failure<string>($"Product {item.ProductId} not found.");
            if (product.StockQuantity < item.Quantity)
                return Result.Failure<string>($"Insufficient stock for '{product.Name}'.");

            order.AddItem(product.Id, product.Name, product.Sku, product.Price, item.Quantity);
            product.AdjustStock(-item.Quantity);
            uow.Products.Update(product);
        }

        // Apply coupon if provided
        if (!string.IsNullOrWhiteSpace(cmd.CouponCode))
        {
            var coupon = await uow.Coupons.GetByCodeAsync(cmd.CouponCode, ct);
            if (coupon is not null && coupon.IsValid(order.Total))
            {
                var discount = coupon.CalculateDiscount(order.Total.Amount);
                order.ApplyDiscount(discount);
                coupon.IncrementUsage();
                uow.Coupons.Update(coupon);
            }
        }

        order.Confirm();
        await uow.Orders.AddAsync(order, ct);
        await uow.SaveChangesAsync(ct);
        return Result.Success(order.OrderNumber);
    }
}

// ─── Ship Order ───────────────────────────────────────────────────────────────

public record ShipOrderCommand(Guid OrderId, string TrackingNumber) : IRequest<Result<bool>>;

public sealed class ShipOrderCommandHandler(IUnitOfWork uow)
    : IRequestHandler<ShipOrderCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(ShipOrderCommand cmd, CancellationToken ct)
    {
        var order = await uow.Orders.GetWithItemsAsync(cmd.OrderId, ct);
        if (order is null) return Result.Failure<bool>("Order not found.");

        order.Ship(cmd.TrackingNumber);
        uow.Orders.Update(order);
        await uow.SaveChangesAsync(ct);
        return Result.Success(true);
    }
}

// ─── Cancel Order ─────────────────────────────────────────────────────────────

public record CancelOrderCommand(Guid OrderId, string Reason) : IRequest<Result<bool>>;

public sealed class CancelOrderCommandHandler(IUnitOfWork uow)
    : IRequestHandler<CancelOrderCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(CancelOrderCommand cmd, CancellationToken ct)
    {
        var order = await uow.Orders.GetWithItemsAsync(cmd.OrderId, ct);
        if (order is null) return Result.Failure<bool>("Order not found.");

        // Restore stock on cancel
        foreach (var item in order.Items)
        {
            var product = await uow.Products.GetByIdAsync(item.ProductId, ct);
            if (product is not null)
            {
                product.AdjustStock(item.Quantity);
                uow.Products.Update(product);
            }
        }

        order.Cancel(cmd.Reason);
        uow.Orders.Update(order);
        await uow.SaveChangesAsync(ct);
        return Result.Success(true);
    }
}
