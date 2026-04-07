using EcomPostgresDb.Domain.ValueObjects;

namespace EcomPostgresDb.Domain.Entities;

/// <summary>
/// Product variant (e.g. size/colour combinations).
/// Each variant has its own SKU, price override, and stock.
/// Uses JSONB Options column for flexible variant attributes.
/// </summary>
public sealed class ProductVariant : BaseEntity
{
    public Guid ProductId { get; private set; }
    public string Sku { get; private set; } = default!;
    public Money? PriceOverride { get; private set; }
    public int StockQuantity { get; private set; }

    /// <summary>JSONB: {"size":"L","color":"blue"}</summary>
    public Dictionary<string, string> Options { get; private set; } = [];

    public string? ImageUrl { get; private set; }
    public bool IsActive { get; private set; } = true;

    // Navigation
    public Product Product { get; private set; } = default!;

    private ProductVariant() { }

    public static ProductVariant Create(Guid productId, string sku, Dictionary<string, string> options,
        int stock, Money? priceOverride = null) =>
        new()
        {
            ProductId = productId,
            Sku = sku.Trim().ToUpperInvariant(),
            Options = options,
            StockQuantity = stock,
            PriceOverride = priceOverride
        };

    public void AdjustStock(int delta)
    {
        var newQty = StockQuantity + delta;
        if (newQty < 0) throw new InvalidOperationException("Insufficient variant stock.");
        StockQuantity = newQty;
        SetUpdatedAt();
    }
}
