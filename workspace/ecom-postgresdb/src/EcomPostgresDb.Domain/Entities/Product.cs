using EcomPostgresDb.Domain.Enums;
using EcomPostgresDb.Domain.ValueObjects;
using EcomPostgresDb.Domain.Events;

namespace EcomPostgresDb.Domain.Entities;

/// <summary>
/// Product aggregate root.
/// 
/// PostgreSQL features used:
///   - JSONB column: Attributes (flexible product specs like color, size, material)
///   - Full-text search: tsvector generated column on Name + Description
///   - GIN index: on Attributes for fast JSONB queries
///   - CHECK constraint: Price > 0, StockQuantity >= 0
/// </summary>
public sealed class Product : BaseEntity
{
    public string Name { get; private set; } = default!;
    public string Slug { get; private set; } = default!;
    public string? Description { get; private set; }
    public string Sku { get; private set; } = default!;
    public Money Price { get; private set; } = default!;
    public Money? CompareAtPrice { get; private set; }
    public int StockQuantity { get; private set; }
    public int LowStockThreshold { get; private set; } = 5;
    public Guid CategoryId { get; private set; }
    public ProductStatus Status { get; private set; } = ProductStatus.Draft;

    /// <summary>
    /// JSONB column — stores flexible product attributes (e.g. {"color":"red","size":"XL"}).
    /// Enables schema-free spec storage without adding columns per attribute.
    /// </summary>
    public Dictionary<string, string> Attributes { get; private set; } = [];

    public double? Weight { get; private set; }
    public string? ImageUrl { get; private set; }

    // Navigation
    public Category Category { get; private set; } = default!;
    private readonly List<ProductVariant> _variants = [];
    private readonly List<Review> _reviews = [];

    public IReadOnlyCollection<ProductVariant> Variants => _variants.AsReadOnly();
    public IReadOnlyCollection<Review> Reviews => _reviews.AsReadOnly();

    private Product() { }

    public static Product Create(
        string name, string slug, string sku,
        Money price, Guid categoryId,
        string? description = null,
        Dictionary<string, string>? attributes = null)
    {
        var product = new Product
        {
            Name = name.Trim(),
            Slug = slug.Trim().ToLowerInvariant(),
            Sku = sku.Trim().ToUpperInvariant(),
            Price = price,
            CategoryId = categoryId,
            Description = description,
            Attributes = attributes ?? []
        };
        product.AddDomainEvent(new ProductCreatedEvent(product.Id, product.Name));
        return product;
    }

    public void Publish()
    {
        if (StockQuantity <= 0)
            throw new InvalidOperationException("Cannot publish a product with no stock.");
        Status = ProductStatus.Active;
        SetUpdatedAt();
    }

    public void AdjustStock(int delta)
    {
        var newQty = StockQuantity + delta;
        if (newQty < 0) throw new InvalidOperationException("Insufficient stock.");
        StockQuantity = newQty;
        Status = StockQuantity == 0 ? ProductStatus.OutOfStock : ProductStatus.Active;
        SetUpdatedAt();
    }

    public void UpdatePrice(Money newPrice)
    {
        CompareAtPrice = Price;
        Price = newPrice;
        SetUpdatedAt();
    }

    public void SetAttributes(Dictionary<string, string> attributes)
    {
        Attributes = attributes;
        SetUpdatedAt();
    }

    public bool IsLowStock => StockQuantity > 0 && StockQuantity <= LowStockThreshold;
}
