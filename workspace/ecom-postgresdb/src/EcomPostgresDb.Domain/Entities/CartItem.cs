using EcomPostgresDb.Domain.ValueObjects;

namespace EcomPostgresDb.Domain.Entities;

/// <summary>
/// Shopping cart — transient state per customer.
/// Items mirror product + variant references with current prices.
/// </summary>
public sealed class CartItem : BaseEntity
{
    public Guid CustomerId { get; private set; }
    public Guid ProductId { get; private set; }
    public Guid? ProductVariantId { get; private set; }
    public string ProductName { get; private set; } = default!;
    public string Sku { get; private set; } = default!;
    public Money UnitPrice { get; private set; } = default!;
    public int Quantity { get; private set; }

    // Navigation
    public Customer Customer { get; private set; } = default!;
    public Product Product { get; private set; } = default!;
    public ProductVariant? Variant { get; private set; }

    public Money LineTotal => UnitPrice.Multiply(Quantity);

    private CartItem() { }

    public static CartItem Create(Guid customerId, Guid productId, string productName,
        string sku, Money unitPrice, int quantity, Guid? variantId = null)
    {
        if (quantity <= 0) throw new ArgumentException("Quantity must be positive.");
        return new CartItem
        {
            CustomerId = customerId,
            ProductId = productId,
            ProductName = productName,
            Sku = sku,
            UnitPrice = unitPrice,
            Quantity = quantity,
            ProductVariantId = variantId
        };
    }

    public void UpdateQuantity(int newQuantity)
    {
        if (newQuantity <= 0) throw new ArgumentException("Quantity must be positive.");
        Quantity = newQuantity;
        SetUpdatedAt();
    }
}
