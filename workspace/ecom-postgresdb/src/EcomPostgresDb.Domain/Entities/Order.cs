using EcomPostgresDb.Domain.Enums;
using EcomPostgresDb.Domain.ValueObjects;
using EcomPostgresDb.Domain.Events;

namespace EcomPostgresDb.Domain.Entities;

/// <summary>
/// Order aggregate root — the most critical boundary in e-commerce.
///
/// Business rules enforced here:
///   - Items can only be added while status is Pending
///   - Cancellation is only allowed before Shipped
///   - Total is computed from OrderItems (not stored redundantly)
///
/// PostgreSQL features:
///   - JSONB: ShippingAddress snapshot (frozen at order time)
///   - Partial index: on (CustomerId, Status) WHERE Status != 'Delivered'
/// </summary>
public sealed class Order : BaseEntity
{
    public string OrderNumber { get; private set; } = default!;
    public Guid CustomerId { get; private set; }
    public OrderStatus Status { get; private set; } = OrderStatus.Pending;

    /// <summary>
    /// Address snapshot stored as JSONB — preserves address at time of order,
    /// even if the customer later changes their address.
    /// </summary>
    public Address ShippingAddress { get; private set; } = default!;
    public Address BillingAddress { get; private set; } = default!;

    public string Currency { get; private set; } = "USD";
    public string? Notes { get; private set; }
    public string? TrackingNumber { get; private set; }
    public DateTime? ShippedAt { get; private set; }
    public DateTime? DeliveredAt { get; private set; }

    // Navigation
    public Customer Customer { get; private set; } = default!;
    private readonly List<OrderItem> _items = [];
    public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();
    public Payment? Payment { get; private set; }

    // Computed total — never stored redundantly
    public Money Total => _items.Aggregate(
        Money.Zero(Currency),
        (acc, item) => acc.Add(item.LineTotal));

    public Money SubTotal => Total;
    public decimal TaxAmount { get; private set; }
    public decimal DiscountAmount { get; private set; }

    private Order() { }

    public static Order Create(Guid customerId, Address shippingAddress, Address billingAddress, string currency = "USD")
    {
        var order = new Order
        {
            OrderNumber = GenerateOrderNumber(),
            CustomerId = customerId,
            ShippingAddress = shippingAddress,
            BillingAddress = billingAddress,
            Currency = currency
        };
        order.AddDomainEvent(new OrderCreatedEvent(order.Id, customerId));
        return order;
    }

    public void AddItem(Guid productId, string productName, string sku, Money unitPrice, int quantity)
    {
        if (Status != OrderStatus.Pending)
            throw new InvalidOperationException("Items can only be added to pending orders.");

        var existing = _items.FirstOrDefault(i => i.ProductId == productId);
        if (existing is not null)
        {
            existing.IncreaseQuantity(quantity);
            return;
        }

        _items.Add(OrderItem.Create(Id, productId, productName, sku, unitPrice, quantity));
        SetUpdatedAt();
    }

    public void Confirm()
    {
        if (Status != OrderStatus.Pending)
            throw new InvalidOperationException("Only pending orders can be confirmed.");
        if (!_items.Any())
            throw new InvalidOperationException("Cannot confirm an empty order.");

        Status = OrderStatus.Confirmed;
        SetUpdatedAt();
        AddDomainEvent(new OrderConfirmedEvent(Id, CustomerId, Total));
    }

    public void Ship(string trackingNumber)
    {
        if (Status != OrderStatus.Processing)
            throw new InvalidOperationException("Order must be in Processing status to ship.");

        Status = OrderStatus.Shipped;
        TrackingNumber = trackingNumber;
        ShippedAt = DateTime.UtcNow;
        SetUpdatedAt();
        AddDomainEvent(new OrderShippedEvent(Id, CustomerId, trackingNumber));
    }

    public void Deliver()
    {
        if (Status != OrderStatus.Shipped)
            throw new InvalidOperationException("Order must be shipped before delivery.");
        Status = OrderStatus.Delivered;
        DeliveredAt = DateTime.UtcNow;
        SetUpdatedAt();
    }

    public void Cancel(string reason)
    {
        if (Status is OrderStatus.Shipped or OrderStatus.Delivered)
            throw new InvalidOperationException("Cannot cancel a shipped or delivered order.");

        Status = OrderStatus.Cancelled;
        Notes = reason;
        SetUpdatedAt();
        AddDomainEvent(new OrderCancelledEvent(Id, CustomerId, reason));
    }

    public void ApplyDiscount(decimal amount)
    {
        if (amount < 0) throw new ArgumentException("Discount cannot be negative.");
        DiscountAmount = amount;
        SetUpdatedAt();
    }

    private static string GenerateOrderNumber() =>
        $"ORD-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..8].ToUpperInvariant()}";
}
