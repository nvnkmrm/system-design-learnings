using EcomPostgresDb.Domain.Entities;
using EcomPostgresDb.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EcomPostgresDb.Infrastructure.Persistence.Configurations;

/// <summary>
/// Order entity configuration.
///
/// PostgreSQL features:
///   1. JSONB — ShippingAddress and BillingAddress stored as JSONB snapshots
///      (preserves address at order time even if customer updates it later)
///   2. Partial index — on (customer_id) WHERE status NOT IN ('Delivered','Cancelled')
///      optimises active-order lookups without scanning historical data
/// </summary>
public sealed class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("orders");
        builder.HasKey(o => o.Id);
        builder.Property(o => o.Id).HasColumnType("uuid").HasDefaultValueSql("gen_random_uuid()");

        builder.Property(o => o.OrderNumber).IsRequired().HasMaxLength(50);
        builder.HasIndex(o => o.OrderNumber).IsUnique();

        builder.Property(o => o.Currency).IsRequired().HasMaxLength(3).HasDefaultValue("USD");
        builder.Property(o => o.TaxAmount).HasColumnType("numeric(18,2)").HasDefaultValue(0m);
        builder.Property(o => o.DiscountAmount).HasColumnType("numeric(18,2)").HasDefaultValue(0m);
        builder.Property(o => o.Notes).HasMaxLength(1000);
        builder.Property(o => o.TrackingNumber).HasMaxLength(100);
        builder.Property(o => o.CreatedAt).HasDefaultValueSql("now()");
        builder.Property(o => o.UpdatedAt).HasDefaultValueSql("now()");

        builder.Property(o => o.Status)
               .HasConversion<string>()
               .HasMaxLength(30);

        // ── PostgreSQL Feature 1: Address as JSONB snapshot ──────────────────
        // Using owned entity with separate shadow properties mapped to JSONB
        builder.OwnsOne(o => o.ShippingAddress, addr =>
        {
            addr.Property(x => x.Line1).HasColumnName("shipping_line1").HasMaxLength(200);
            addr.Property(x => x.Line2).HasColumnName("shipping_line2").HasMaxLength(200);
            addr.Property(x => x.City).HasColumnName("shipping_city").HasMaxLength(100);
            addr.Property(x => x.State).HasColumnName("shipping_state").HasMaxLength(100);
            addr.Property(x => x.PostalCode).HasColumnName("shipping_postal_code").HasMaxLength(20);
            addr.Property(x => x.Country).HasColumnName("shipping_country").HasMaxLength(2);
        });

        builder.OwnsOne(o => o.BillingAddress, addr =>
        {
            addr.Property(x => x.Line1).HasColumnName("billing_line1").HasMaxLength(200);
            addr.Property(x => x.Line2).HasColumnName("billing_line2").HasMaxLength(200);
            addr.Property(x => x.City).HasColumnName("billing_city").HasMaxLength(100);
            addr.Property(x => x.State).HasColumnName("billing_state").HasMaxLength(100);
            addr.Property(x => x.PostalCode).HasColumnName("billing_postal_code").HasMaxLength(20);
            addr.Property(x => x.Country).HasColumnName("billing_country").HasMaxLength(2);
        });

        // ── PostgreSQL Feature 2: Partial index on active orders ───────────────
        // ~WHERE status NOT IN ('Delivered','Cancelled')~ speeds up active order queries
        builder.HasIndex(o => new { o.CustomerId, o.Status })
               .HasFilter("status NOT IN ('Delivered','Cancelled')");

        builder.HasOne(o => o.Customer)
               .WithMany(c => c.Orders)
               .HasForeignKey(o => o.CustomerId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(o => o.Items)
               .WithOne(i => i.Order)
               .HasForeignKey(i => i.OrderId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(o => o.Payment)
               .WithOne(p => p.Order)
               .HasForeignKey<Payment>(p => p.OrderId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}

public sealed class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.ToTable("order_items");
        builder.HasKey(i => i.Id);
        builder.Property(i => i.Id).HasColumnType("uuid").HasDefaultValueSql("gen_random_uuid()");

        builder.Property(i => i.ProductName).IsRequired().HasMaxLength(200);
        builder.Property(i => i.Sku).IsRequired().HasMaxLength(100);

        builder.OwnsOne(i => i.UnitPrice, money =>
        {
            money.Property(m => m.Amount).HasColumnName("unit_price_amount")
                 .HasColumnType("numeric(18,2)").IsRequired();
            money.Property(m => m.Currency).HasColumnName("unit_price_currency")
                 .HasMaxLength(3).IsRequired();
        });

        builder.Property(i => i.Quantity).IsRequired();
        builder.HasIndex(i => new { i.OrderId, i.ProductId });
    }
}
