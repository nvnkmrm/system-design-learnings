using EcomPostgresDb.Domain.Entities;

namespace EcomPostgresDb.Domain.Interfaces;

/// <summary>
/// Generic repository interface — Repository Pattern.
/// Infrastructure provides the concrete implementation.
/// Domain only depends on this abstraction.
/// </summary>
public interface IRepository<T> where T : BaseEntity
{
    Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IEnumerable<T>> GetAllAsync(CancellationToken ct = default);
    Task AddAsync(T entity, CancellationToken ct = default);
    void Update(T entity);
    void Remove(T entity);
}

public interface ICustomerRepository : IRepository<Customer>
{
    Task<Customer?> GetByEmailAsync(string email, CancellationToken ct = default);
    Task<bool> EmailExistsAsync(string email, CancellationToken ct = default);
}

public interface IProductRepository : IRepository<Product>
{
    Task<Product?> GetBySlugAsync(string slug, CancellationToken ct = default);
    Task<IEnumerable<Product>> SearchAsync(string term, CancellationToken ct = default);
    Task<IEnumerable<Product>> GetByCategoryAsync(Guid categoryId, CancellationToken ct = default);
    Task<IEnumerable<Product>> GetLowStockAsync(CancellationToken ct = default);
}

public interface ICategoryRepository : IRepository<Category>
{
    Task<Category?> GetBySlugAsync(string slug, CancellationToken ct = default);
    Task<IEnumerable<Category>> GetWithSubCategoriesAsync(CancellationToken ct = default);
}

public interface IOrderRepository : IRepository<Order>
{
    Task<Order?> GetByOrderNumberAsync(string orderNumber, CancellationToken ct = default);
    Task<IEnumerable<Order>> GetByCustomerAsync(Guid customerId, CancellationToken ct = default);
    Task<Order?> GetWithItemsAsync(Guid orderId, CancellationToken ct = default);
}

public interface ICouponRepository : IRepository<Coupon>
{
    Task<Coupon?> GetByCodeAsync(string code, CancellationToken ct = default);
}

/// <summary>
/// Unit of Work — coordinates multiple repositories in a single transaction.
/// </summary>
public interface IUnitOfWork
{
    ICustomerRepository Customers { get; }
    IProductRepository Products { get; }
    ICategoryRepository Categories { get; }
    IOrderRepository Orders { get; }
    ICouponRepository Coupons { get; }

    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
