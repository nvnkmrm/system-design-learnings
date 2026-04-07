using EcomPostgresDb.Domain.Entities;
using EcomPostgresDb.Infrastructure.Persistence.Configurations;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace EcomPostgresDb.Infrastructure.Persistence;

/// <summary>
/// Application DbContext with PostgreSQL-specific configurations.
///
/// PostgreSQL features wired here:
///   - UseNpgsql() — Npgsql provider
///   - HasPostgresEnum() — native PostgreSQL enum types
///   - UseSnakeCaseNamingConvention() — snake_case column names (idiomatic PostgreSQL)
///   - Domain event dispatch on SaveChanges
/// </summary>
public sealed class ApplicationDbContext : DbContext
{
    private readonly IMediator _mediator;

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, IMediator mediator)
        : base(options)
    {
        _mediator = mediator;
    }

    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<CustomerAddress> CustomerAddresses => Set<CustomerAddress>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductVariant> ProductVariants => Set<ProductVariant>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<CartItem> CartItems => Set<CartItem>();
    public DbSet<Review> Reviews => Set<Review>();
    public DbSet<Coupon> Coupons => Set<Coupon>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Apply all IEntityTypeConfiguration<T> classes found in this assembly
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        // PostgreSQL: use snake_case naming (products, product_variants, etc.)
        // Handled by UseSnakeCaseNamingConvention() in UseNpgsql registration

        base.OnModelCreating(modelBuilder);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Dispatch domain events before committing (Outbox-lite pattern)
        var entitiesWithEvents = ChangeTracker
            .Entries<BaseEntity>()
            .Where(e => e.Entity.DomainEvents.Any())
            .Select(e => e.Entity)
            .ToList();

        var result = await base.SaveChangesAsync(cancellationToken);

        foreach (var entity in entitiesWithEvents)
        {
            foreach (var domainEvent in entity.DomainEvents)
                await _mediator.Publish((INotification)domainEvent, cancellationToken);
            entity.ClearDomainEvents();
        }

        return result;
    }
}
