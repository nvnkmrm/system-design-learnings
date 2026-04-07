using EcomPostgresDb.Application.Common.Interfaces;
using EcomPostgresDb.Domain.Interfaces;
using EcomPostgresDb.Infrastructure.Persistence;
using EcomPostgresDb.Infrastructure.Persistence.Repositories;
using EcomPostgresDb.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EcomPostgresDb.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is missing.");

        // ── PostgreSQL via Npgsql ──────────────────────────────────────────────
        // UseSnakeCaseNamingConvention() — maps C# PascalCase to snake_case
        // (e.g. OrderItems → order_items, CustomerId → customer_id)
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(connectionString, npgsql =>
            {
                npgsql.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName);
                npgsql.EnableRetryOnFailure(maxRetryCount: 3);
            })
            .UseSnakeCaseNamingConvention());

        // ── Repositories ──────────────────────────────────────────────────────
        services.AddScoped<CustomerRepository>();
        services.AddScoped<ProductRepository>();
        services.AddScoped<CategoryRepository>();
        services.AddScoped<OrderRepository>();
        services.AddScoped<CouponRepository>();

        // ── Unit of Work ──────────────────────────────────────────────────────
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // ── Auth services ─────────────────────────────────────────────────────
        services.AddSingleton<IPasswordHasher, BcryptPasswordHasher>();
        services.AddScoped<ITokenService, JwtTokenService>();

        return services;
    }
}
