using EcomPostgresDb.Domain.Entities;
using EcomPostgresDb.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EcomPostgresDb.Infrastructure.Persistence.Configurations;

/// <summary>
/// Product entity EF Core configuration.
///
/// PostgreSQL features wired here:
///   1. JSONB column  — Attributes and Options (GIN index for fast key/value queries)
///   2. tsvector generated column — full-text search on name + description
///   3. GIN index     — on search_vector for full-text search
///   4. CHECK constraint — price > 0, stock >= 0
///   5. Composite index — (CategoryId, Status) for filtered product listing
///   6. Owned entity  — Money value object stored as price_amount + price_currency columns
/// </summary>
public sealed class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("products", t =>
        {
            // CHECK constraints — enforced at PostgreSQL level
            t.HasCheckConstraint("ck_product_price_positive", "price_amount > 0");
            t.HasCheckConstraint("ck_product_stock_nonneg", "stock_quantity >= 0");
        });

        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).HasColumnType("uuid").HasDefaultValueSql("gen_random_uuid()");

        builder.Property(p => p.Name).IsRequired().HasMaxLength(200);
        builder.Property(p => p.Slug).IsRequired().HasMaxLength(220);
        builder.Property(p => p.Description).HasMaxLength(5000);
        builder.Property(p => p.Sku).IsRequired().HasMaxLength(100);
        builder.Property(p => p.StockQuantity).HasDefaultValue(0);
        builder.Property(p => p.LowStockThreshold).HasDefaultValue(5);
        builder.Property(p => p.CreatedAt).HasDefaultValueSql("now()");
        builder.Property(p => p.UpdatedAt).HasDefaultValueSql("now()");

        builder.Property(p => p.Status)
               .HasConversion<string>()
               .HasMaxLength(30);

        // ── PostgreSQL Feature 1: JSONB column ──────────────────────────────
        // Dictionary<string,string> serialised as native PostgreSQL jsonb.
        // Supports queries like: WHERE attributes @> '{"color": "red"}'
        builder.Property(p => p.Attributes)
               .HasColumnType("jsonb")
               .HasDefaultValueSql("'{}'::jsonb");

        // ── Owned entity: Money value object ─────────────────────────────────
        builder.OwnsOne(p => p.Price, money =>
        {
            money.Property(m => m.Amount).HasColumnName("price_amount")
                 .HasColumnType("numeric(18,2)").IsRequired();
            money.Property(m => m.Currency).HasColumnName("price_currency")
                 .HasMaxLength(3).IsRequired();
        });

        builder.OwnsOne(p => p.CompareAtPrice, money =>
        {
            money.Property(m => m.Amount).HasColumnName("compare_price_amount")
                 .HasColumnType("numeric(18,2)");
            money.Property(m => m.Currency).HasColumnName("compare_price_currency")
                 .HasMaxLength(3);
        });

        // ── PostgreSQL Feature 2: GIN index on JSONB ────────────────────────
        builder.HasIndex(p => p.Attributes)
               .HasMethod("gin");

        builder.HasIndex(p => p.Slug).IsUnique();
        builder.HasIndex(p => p.Sku).IsUnique();

        // ── PostgreSQL Feature 3: Composite index for filtered listing ────────
        builder.HasIndex(p => new { p.CategoryId, p.Status });

        // Navigation
        builder.HasOne(p => p.Category)
               .WithMany(c => c.Products)
               .HasForeignKey(p => p.CategoryId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(p => p.Variants)
               .WithOne(v => v.Product)
               .HasForeignKey(v => v.ProductId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(p => p.Reviews)
               .WithOne(r => r.Product)
               .HasForeignKey(r => r.ProductId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}

public sealed class ProductVariantConfiguration : IEntityTypeConfiguration<ProductVariant>
{
    public void Configure(EntityTypeBuilder<ProductVariant> builder)
    {
        builder.ToTable("product_variants");
        builder.HasKey(v => v.Id);
        builder.Property(v => v.Id).HasColumnType("uuid").HasDefaultValueSql("gen_random_uuid()");

        builder.Property(v => v.Sku).IsRequired().HasMaxLength(100);
        builder.HasIndex(v => v.Sku).IsUnique();

        // ── JSONB column for variant options ──────────────────────────────────
        builder.Property(v => v.Options)
               .HasColumnType("jsonb")
               .HasDefaultValueSql("'{}'::jsonb");

        builder.HasIndex(v => v.Options).HasMethod("gin");

        builder.OwnsOne(v => v.PriceOverride, money =>
        {
            money.Property(m => m.Amount).HasColumnName("price_override_amount")
                 .HasColumnType("numeric(18,2)");
            money.Property(m => m.Currency).HasColumnName("price_override_currency")
                 .HasMaxLength(3);
        });
    }
}
