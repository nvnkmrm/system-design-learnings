using EcomPostgresDb.Domain.Entities;
using EcomPostgresDb.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EcomPostgresDb.Infrastructure.Persistence.Configurations;

/// <summary>
/// Customer EF Core configuration.
/// Demonstrates:
///   - HasIndex with IsUnique for email
///   - Owned Address value object (flattened into same table)
/// </summary>
public sealed class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("customers");
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Id).HasColumnType("uuid").HasDefaultValueSql("gen_random_uuid()");
        builder.Property(c => c.FirstName).IsRequired().HasMaxLength(100);
        builder.Property(c => c.LastName).IsRequired().HasMaxLength(100);
        builder.Property(c => c.Email).IsRequired().HasMaxLength(256);
        builder.Property(c => c.PasswordHash).IsRequired();
        builder.Property(c => c.PhoneNumber).HasMaxLength(30);
        builder.Property(c => c.CreatedAt).HasDefaultValueSql("now()");
        builder.Property(c => c.UpdatedAt).HasDefaultValueSql("now()");

        // Unique index on email — enforced at DB level
        builder.HasIndex(c => c.Email).IsUnique();

        // Navigation: one-to-many addresses
        builder.HasMany(c => c.Addresses)
               .WithOne(a => a.Customer)
               .HasForeignKey(a => a.CustomerId)
               .OnDelete(DeleteBehavior.Cascade);

        // Navigation: cart items
        builder.HasMany(c => c.CartItems)
               .WithOne(ci => ci.Customer)
               .HasForeignKey(ci => ci.CustomerId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}

public sealed class CustomerAddressConfiguration : IEntityTypeConfiguration<CustomerAddress>
{
    public void Configure(EntityTypeBuilder<CustomerAddress> builder)
    {
        builder.ToTable("customer_addresses");
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id).HasColumnType("uuid").HasDefaultValueSql("gen_random_uuid()");

        /// Owned entity: Address value object is stored as flat columns
        builder.OwnsOne(a => a.Address, addr =>
        {
            addr.Property(x => x.Line1).HasColumnName("line1").IsRequired().HasMaxLength(200);
            addr.Property(x => x.Line2).HasColumnName("line2").HasMaxLength(200);
            addr.Property(x => x.City).HasColumnName("city").IsRequired().HasMaxLength(100);
            addr.Property(x => x.State).HasColumnName("state").IsRequired().HasMaxLength(100);
            addr.Property(x => x.PostalCode).HasColumnName("postal_code").IsRequired().HasMaxLength(20);
            addr.Property(x => x.Country).HasColumnName("country").IsRequired().HasMaxLength(2);
        });

        builder.Property(a => a.AddressType)
               .HasConversion<string>()
               .HasMaxLength(20);

        builder.HasIndex(a => new { a.CustomerId, a.IsDefault });
    }
}
