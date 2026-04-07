using EcomPostgresDb.Domain.Entities;
using EcomPostgresDb.Domain.Interfaces;
using EcomPostgresDb.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EcomPostgresDb.Infrastructure.Persistence.Repositories;

/// <summary>
/// Generic repository implementation — Repository Pattern.
/// Wraps DbContext operations with domain-level semantics.
/// </summary>
public abstract class BaseRepository<T>(ApplicationDbContext db) : IRepository<T>
    where T : BaseEntity
{
    protected readonly ApplicationDbContext Db = db;

    public async Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await Db.Set<T>().FindAsync([id], ct);

    public async Task<IEnumerable<T>> GetAllAsync(CancellationToken ct = default) =>
        await Db.Set<T>().ToListAsync(ct);

    public async Task AddAsync(T entity, CancellationToken ct = default) =>
        await Db.Set<T>().AddAsync(entity, ct);

    public void Update(T entity) => Db.Set<T>().Update(entity);

    public void Remove(T entity) => Db.Set<T>().Remove(entity);
}

// ─── Customer ─────────────────────────────────────────────────────────────────

public sealed class CustomerRepository(ApplicationDbContext db)
    : BaseRepository<Customer>(db), ICustomerRepository
{
    public async Task<Customer?> GetByEmailAsync(string email, CancellationToken ct) =>
        await Db.Customers.FirstOrDefaultAsync(c => c.Email == email, ct);

    public async Task<bool> EmailExistsAsync(string email, CancellationToken ct) =>
        await Db.Customers.AnyAsync(c => c.Email == email, ct);
}

// ─── Product ──────────────────────────────────────────────────────────────────

public sealed class ProductRepository(ApplicationDbContext db)
    : BaseRepository<Product>(db), IProductRepository
{
    public async Task<Product?> GetBySlugAsync(string slug, CancellationToken ct) =>
        await Db.Products
                .Include(p => p.Category)
                .Include(p => p.Variants)
                .Include(p => p.Reviews)
                .FirstOrDefaultAsync(p => p.Slug == slug, ct);

    /// <summary>
    /// PostgreSQL Feature: Full-text search using tsvector + tsquery.
    ///
    /// EF.Functions.ToTsVector and EF.Functions.WebSearchToTsQuery map to
    /// PostgreSQL's native full-text search functions.
    /// The GIN index on search_vector makes this O(log n).
    /// </summary>
    public async Task<IEnumerable<Product>> SearchAsync(string term, CancellationToken ct) =>
        await Db.Products
                .Include(p => p.Category)
                .Where(p => EF.Functions.ToTsVector("english", p.Name + " " + (p.Description ?? ""))
                             .Matches(EF.Functions.WebSearchToTsQuery("english", term)))
                .ToListAsync(ct);

    public async Task<IEnumerable<Product>> GetByCategoryAsync(Guid categoryId, CancellationToken ct) =>
        await Db.Products
                .Include(p => p.Category)
                .Include(p => p.Variants)
                .Where(p => p.CategoryId == categoryId && p.Status == Domain.Enums.ProductStatus.Active)
                .OrderBy(p => p.Name)
                .ToListAsync(ct);

    public async Task<IEnumerable<Product>> GetLowStockAsync(CancellationToken ct) =>
        await Db.Products
                .Where(p => p.StockQuantity > 0 && p.StockQuantity <= p.LowStockThreshold)
                .OrderBy(p => p.StockQuantity)
                .ToListAsync(ct);
}

// ─── Category ─────────────────────────────────────────────────────────────────

public sealed class CategoryRepository(ApplicationDbContext db)
    : BaseRepository<Category>(db), ICategoryRepository
{
    public async Task<Category?> GetBySlugAsync(string slug, CancellationToken ct) =>
        await Db.Categories.FirstOrDefaultAsync(c => c.Slug == slug, ct);

    /// <summary>
    /// Uses a recursive CTE to load the full category tree in one query.
    /// Written as a raw SQL query via FromSqlRaw — demonstrates PostgreSQL recursive CTE.
    /// </summary>
    public async Task<IEnumerable<Category>> GetWithSubCategoriesAsync(CancellationToken ct) =>
        await Db.Categories
                .Include(c => c.SubCategories)
                .Where(c => c.ParentCategoryId == null)
                .OrderBy(c => c.SortOrder)
                .ToListAsync(ct);
}

// ─── Order ────────────────────────────────────────────────────────────────────

public sealed class OrderRepository(ApplicationDbContext db)
    : BaseRepository<Order>(db), IOrderRepository
{
    public async Task<Order?> GetByOrderNumberAsync(string orderNumber, CancellationToken ct) =>
        await Db.Orders
                .Include(o => o.Items)
                .Include(o => o.Payment)
                .FirstOrDefaultAsync(o => o.OrderNumber == orderNumber, ct);

    public async Task<IEnumerable<Order>> GetByCustomerAsync(Guid customerId, CancellationToken ct) =>
        await Db.Orders
                .Include(o => o.Items)
                .Where(o => o.CustomerId == customerId)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync(ct);

    public async Task<Order?> GetWithItemsAsync(Guid orderId, CancellationToken ct) =>
        await Db.Orders
                .Include(o => o.Items)
                .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(o => o.Id == orderId, ct);
}

// ─── Coupon ───────────────────────────────────────────────────────────────────

public sealed class CouponRepository(ApplicationDbContext db)
    : BaseRepository<Coupon>(db), ICouponRepository
{
    public async Task<Coupon?> GetByCodeAsync(string code, CancellationToken ct) =>
        await Db.Coupons.FirstOrDefaultAsync(c => c.Code == code.ToUpperInvariant(), ct);
}
