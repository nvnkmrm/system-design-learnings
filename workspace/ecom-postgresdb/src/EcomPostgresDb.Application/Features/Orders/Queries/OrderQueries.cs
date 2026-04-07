using EcomPostgresDb.Application.Common.Models;
using EcomPostgresDb.Domain.Enums;
using EcomPostgresDb.Domain.Interfaces;
using MediatR;

namespace EcomPostgresDb.Application.Features.Orders.Queries;

// ─── DTOs ─────────────────────────────────────────────────────────────────────

public record OrderSummaryDto(
    Guid Id,
    string OrderNumber,
    OrderStatus Status,
    decimal Total,
    string Currency,
    int ItemCount,
    DateTime CreatedAt);

public record OrderDetailDto(
    Guid Id,
    string OrderNumber,
    OrderStatus Status,
    decimal SubTotal,
    decimal TaxAmount,
    decimal DiscountAmount,
    decimal GrandTotal,
    string Currency,
    string? TrackingNumber,
    DateTime CreatedAt,
    DateTime? ShippedAt,
    IEnumerable<OrderItemDto> Items);

public record OrderItemDto(
    Guid ProductId,
    string ProductName,
    string Sku,
    decimal UnitPrice,
    int Quantity,
    decimal LineTotal);

// ─── Queries ──────────────────────────────────────────────────────────────────

public record GetOrderByNumberQuery(string OrderNumber) : IRequest<Result<OrderDetailDto>>;

public sealed class GetOrderByNumberQueryHandler(IUnitOfWork uow)
    : IRequestHandler<GetOrderByNumberQuery, Result<OrderDetailDto>>
{
    public async Task<Result<OrderDetailDto>> Handle(GetOrderByNumberQuery query, CancellationToken ct)
    {
        var order = await uow.Orders.GetByOrderNumberAsync(query.OrderNumber, ct);
        if (order is null) return Result.Failure<OrderDetailDto>("Order not found.");

        var items = order.Items.Select(i => new OrderItemDto(
            i.ProductId, i.ProductName, i.Sku,
            i.UnitPrice.Amount, i.Quantity, i.LineTotal.Amount));

        var grandTotal = order.Total.Amount - order.DiscountAmount;

        return Result.Success(new OrderDetailDto(
            order.Id, order.OrderNumber, order.Status,
            order.SubTotal.Amount, order.TaxAmount, order.DiscountAmount,
            grandTotal, order.Currency,
            order.TrackingNumber, order.CreatedAt, order.ShippedAt, items));
    }
}

public record GetCustomerOrdersQuery(Guid CustomerId) : IRequest<Result<IEnumerable<OrderSummaryDto>>>;

public sealed class GetCustomerOrdersQueryHandler(IUnitOfWork uow)
    : IRequestHandler<GetCustomerOrdersQuery, Result<IEnumerable<OrderSummaryDto>>>
{
    public async Task<Result<IEnumerable<OrderSummaryDto>>> Handle(GetCustomerOrdersQuery query, CancellationToken ct)
    {
        var orders = await uow.Orders.GetByCustomerAsync(query.CustomerId, ct);
        var dtos = orders.Select(o => new OrderSummaryDto(
            o.Id, o.OrderNumber, o.Status,
            o.Total.Amount, o.Currency, o.Items.Count, o.CreatedAt));
        return Result.Success(dtos);
    }
}
