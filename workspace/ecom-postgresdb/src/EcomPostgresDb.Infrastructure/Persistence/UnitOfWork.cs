using EcomPostgresDb.Application.Features.Cart.Commands;
using EcomPostgresDb.Domain.Entities;
using EcomPostgresDb.Domain.Interfaces;
using EcomPostgresDb.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;

namespace EcomPostgresDb.Infrastructure.Persistence;

/// <summary>
/// Unit of Work — coordinates all repositories within a single DbContext transaction.
/// Implements ICartDirectAccess so the Application layer can manage cart items
/// without a dedicated full repository.
/// </summary>
public sealed class UnitOfWork(
    ApplicationDbContext db,
    CustomerRepository customers,
    ProductRepository products,
    CategoryRepository categories,
    OrderRepository orders,
    CouponRepository coupons)
    : IUnitOfWork, ICartDirectAccess
{
    public ICustomerRepository Customers { get; } = customers;
    public IProductRepository Products { get; } = products;
    public ICategoryRepository Categories { get; } = categories;
    public IOrderRepository Orders { get; } = orders;
    public ICouponRepository Coupons { get; } = coupons;

    public Task<int> SaveChangesAsync(CancellationToken ct = default) =>
        db.SaveChangesAsync(ct);

    // ─── ICartDirectAccess implementation ────────────────────────────────────
    public async Task AddCartItemAsync(CartItem item, CancellationToken ct) =>
        await db.CartItems.AddAsync(item, ct);

    public Task<CartItem?> GetCartItemAsync(Guid cartItemId, CancellationToken ct) =>
        db.CartItems.FindAsync([cartItemId], ct).AsTask();

    public async Task<IEnumerable<CartItem>> GetCartItemsByCustomerAsync(Guid customerId, CancellationToken ct) =>
        await db.CartItems
                .Include(c => c.Product)
                .Include(c => c.Variant)
                .Where(c => c.CustomerId == customerId)
                .ToListAsync(ct);

    public void RemoveCartItem(CartItem item) => db.CartItems.Remove(item);
}
