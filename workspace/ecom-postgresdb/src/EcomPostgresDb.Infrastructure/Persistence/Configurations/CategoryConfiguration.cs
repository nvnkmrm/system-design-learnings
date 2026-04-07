using EcomPostgresDb.Domain.Entities;
using EcomPostgresDb.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EcomPostgresDb.Infrastructure.Persistence.Configurations;

/// <summary>
/// Category entity — self-referencing hierarchy.
/// Demonstrates:
///   - Recursive self-join FK
///   - Composite index on (ParentCategoryId, SortOrder)
/// </summary>
public sealed class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("categories");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).HasColumnType("uuid").HasDefaultValueSql("gen_random_uuid()");

        builder.Property(c => c.Name).IsRequired().HasMaxLength(200);
        builder.Property(c => c.Slug).IsRequired().HasMaxLength(220);
        builder.Property(c => c.Description).HasMaxLength(1000);
        builder.Property(c => c.CreatedAt).HasDefaultValueSql("now()");
        builder.Property(c => c.UpdatedAt).HasDefaultValueSql("now()");

        builder.HasIndex(c => c.Slug).IsUnique();

        // Self-referencing: parent → children
        builder.HasOne(c => c.ParentCategory)
               .WithMany(c => c.SubCategories)
               .HasForeignKey(c => c.ParentCategoryId)
               .IsRequired(false)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(c => new { c.ParentCategoryId, c.SortOrder });
    }
}
