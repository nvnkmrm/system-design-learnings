using EcomPostgresDb.Domain.ValueObjects;

namespace EcomPostgresDb.Domain.Entities;

/// <summary>
/// Order line item — child of the Order aggregate.
/// Captures a product snapshot (name, SKU, price) at time of purchase.
/// </summary>
public sealed class OrderItem : BaseEntity
{
    public Guid OrderId { get; private set; }
    public Guid ProductId { get; private set; }
    public string ProductName { get; private set; } = default!;
    public string Sku { get; private set; } = default!;
    public Money UnitPrice { get; private set; } = default!;
    public int Quantity { get; private set; }

    // Navigation
    public Order Order { get; private set; } = default!;
    public Product Product { get; private set; } = default!;

    public Money LineTotal => UnitPrice.Multiply(Quantity);

    private OrderItem() { }

    internal static OrderItem Create(Guid orderId, Guid productId, string productName,
        string sku, Money unitPrice, int quantity)
    {
        if (quantity <= 0) throw new ArgumentException("Quantity must be positive.");
        return new OrderItem
        {
            OrderId = orderId,
            ProductId = productId,
            ProductName = productName,
            Sku = sku,
            UnitPrice = unitPrice,
            Quantity = quantity
        };
    }

    internal void IncreaseQuantity(int additional)
    {
        if (additional <= 0) throw new ArgumentException("Additional quantity must be positive.");
        Quantity += additional;
        SetUpdatedAt();
    }
}
