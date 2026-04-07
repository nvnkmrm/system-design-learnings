using EcomPostgresDb.Domain.Entities;
using EcomPostgresDb.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EcomPostgresDb.Infrastructure.Persistence.Configurations;

public sealed class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.ToTable("payments");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).HasColumnType("uuid").HasDefaultValueSql("gen_random_uuid()");

        builder.Property(p => p.TransactionId).HasMaxLength(200);
        builder.Property(p => p.GatewayResponse).HasColumnType("text");
        builder.Property(p => p.CreatedAt).HasDefaultValueSql("now()");
        builder.Property(p => p.UpdatedAt).HasDefaultValueSql("now()");

        builder.Property(p => p.Status).HasConversion<string>().HasMaxLength(30);
        builder.Property(p => p.Method).HasConversion<string>().HasMaxLength(30);

        builder.OwnsOne(p => p.Amount, money =>
        {
            money.Property(m => m.Amount).HasColumnName("amount")
                 .HasColumnType("numeric(18,2)").IsRequired();
            money.Property(m => m.Currency).HasColumnName("currency")
                 .HasMaxLength(3).IsRequired();
        });

        builder.HasIndex(p => p.OrderId).IsUnique();
        builder.HasIndex(p => p.TransactionId);
    }
}

public sealed class CartItemConfiguration : IEntityTypeConfiguration<CartItem>
{
    public void Configure(EntityTypeBuilder<CartItem> builder)
    {
        builder.ToTable("cart_items");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).HasColumnType("uuid").HasDefaultValueSql("gen_random_uuid()");

        builder.Property(c => c.ProductName).IsRequired().HasMaxLength(200);
        builder.Property(c => c.Sku).IsRequired().HasMaxLength(100);
        builder.Property(c => c.CreatedAt).HasDefaultValueSql("now()");
        builder.Property(c => c.UpdatedAt).HasDefaultValueSql("now()");

        builder.OwnsOne(c => c.UnitPrice, money =>
        {
            money.Property(m => m.Amount).HasColumnName("unit_price_amount")
                 .HasColumnType("numeric(18,2)").IsRequired();
            money.Property(m => m.Currency).HasColumnName("unit_price_currency")
                 .HasMaxLength(3).IsRequired();
        });

        builder.HasIndex(c => new { c.CustomerId, c.ProductId });
    }
}

public sealed class ReviewConfiguration : IEntityTypeConfiguration<Review>
{
    public void Configure(EntityTypeBuilder<Review> builder)
    {
        builder.ToTable("reviews", t =>
        {
            t.HasCheckConstraint("ck_review_rating_range", "rating BETWEEN 1 AND 5");
        });

        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id).HasColumnType("uuid").HasDefaultValueSql("gen_random_uuid()");

        builder.Property(r => r.Title).HasMaxLength(200);
        builder.Property(r => r.Body).HasMaxLength(5000);
        builder.Property(r => r.Rating).IsRequired();
        builder.Property(r => r.CreatedAt).HasDefaultValueSql("now()");
        builder.Property(r => r.UpdatedAt).HasDefaultValueSql("now()");

        builder.Property(r => r.Status).HasConversion<string>().HasMaxLength(20);

        // Partial index — only approved reviews are queried for averages
        builder.HasIndex(r => r.ProductId)
               .HasFilter("status = 'Approved'");

        // Prevent duplicate reviews from same customer
        builder.HasIndex(r => new { r.ProductId, r.CustomerId }).IsUnique();
    }
}

public sealed class CouponConfiguration : IEntityTypeConfiguration<Coupon>
{
    public void Configure(EntityTypeBuilder<Coupon> builder)
    {
        builder.ToTable("coupons");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).HasColumnType("uuid").HasDefaultValueSql("gen_random_uuid()");

        builder.Property(c => c.Code).IsRequired().HasMaxLength(50);
        builder.Property(c => c.Description).IsRequired().HasMaxLength(500);
        builder.Property(c => c.DiscountValue).HasColumnType("numeric(10,2)");
        builder.Property(c => c.CreatedAt).HasDefaultValueSql("now()");
        builder.Property(c => c.UpdatedAt).HasDefaultValueSql("now()");

        builder.Property(c => c.DiscountType).HasConversion<string>().HasMaxLength(20);

        builder.HasIndex(c => c.Code).IsUnique();

        builder.OwnsOne(c => c.MinimumOrderAmount, money =>
        {
            money.Property(m => m.Amount).HasColumnName("min_order_amount")
                 .HasColumnType("numeric(18,2)");
            money.Property(m => m.Currency).HasColumnName("min_order_currency")
                 .HasMaxLength(3);
        });
    }
}
