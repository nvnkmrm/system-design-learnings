using EcomPostgresDb.Domain.Enums;
using EcomPostgresDb.Domain.ValueObjects;

namespace EcomPostgresDb.Domain.Entities;

/// <summary>
/// Category entity with self-referencing hierarchy (adjacency list).
/// PostgreSQL recursive CTE is used for tree queries in repositories.
/// </summary>
public sealed class Category : BaseEntity
{
    public string Name { get; private set; } = default!;
    public string Slug { get; private set; } = default!;
    public string? Description { get; private set; }
    public Guid? ParentCategoryId { get; private set; }
    public int SortOrder { get; private set; }
    public bool IsActive { get; private set; } = true;

    // Navigation
    public Category? ParentCategory { get; private set; }
    private readonly List<Category> _subCategories = [];
    private readonly List<Product> _products = [];

    public IReadOnlyCollection<Category> SubCategories => _subCategories.AsReadOnly();
    public IReadOnlyCollection<Product> Products => _products.AsReadOnly();

    private Category() { }

    public static Category Create(string name, string slug, string? description = null, Guid? parentId = null, int sortOrder = 0) =>
        new()
        {
            Name = name.Trim(),
            Slug = slug.Trim().ToLowerInvariant(),
            Description = description,
            ParentCategoryId = parentId,
            SortOrder = sortOrder
        };

    public void Update(string name, string slug, string? description, int sortOrder)
    {
        Name = name.Trim();
        Slug = slug.Trim().ToLowerInvariant();
        Description = description;
        SortOrder = sortOrder;
        SetUpdatedAt();
    }
}
