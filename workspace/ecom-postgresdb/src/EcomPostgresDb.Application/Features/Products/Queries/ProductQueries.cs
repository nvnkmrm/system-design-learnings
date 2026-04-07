using EcomPostgresDb.Application.Common.Models;
using EcomPostgresDb.Domain.Enums;
using EcomPostgresDb.Domain.Interfaces;
using MediatR;

namespace EcomPostgresDb.Application.Features.Products.Queries;

// ─── DTOs ─────────────────────────────────────────────────────────────────────

public record ProductSummaryDto(
    Guid Id,
    string Name,
    string Slug,
    string Sku,
    decimal Price,
    string Currency,
    int StockQuantity,
    ProductStatus Status,
    string CategoryName,
    Dictionary<string, string> Attributes);

public record ProductDetailDto(
    Guid Id,
    string Name,
    string Slug,
    string Sku,
    decimal Price,
    decimal? CompareAtPrice,
    string Currency,
    int StockQuantity,
    ProductStatus Status,
    string? Description,
    string CategoryName,
    Dictionary<string, string> Attributes,
    IEnumerable<VariantDto> Variants,
    double? AverageRating,
    int ReviewCount);

public record VariantDto(
    Guid Id,
    string Sku,
    decimal? PriceOverride,
    int Stock,
    Dictionary<string, string> Options);

// ─── Queries ──────────────────────────────────────────────────────────────────

public record GetProductBySlugQuery(string Slug) : IRequest<Result<ProductDetailDto>>;

public sealed class GetProductBySlugQueryHandler(IUnitOfWork uow)
    : IRequestHandler<GetProductBySlugQuery, Result<ProductDetailDto>>
{
    public async Task<Result<ProductDetailDto>> Handle(GetProductBySlugQuery query, CancellationToken ct)
    {
        var product = await uow.Products.GetBySlugAsync(query.Slug, ct);
        if (product is null) return Result.Failure<ProductDetailDto>("Product not found.");

        var variants = product.Variants.Select(v => new VariantDto(
            v.Id, v.Sku, v.PriceOverride?.Amount, v.StockQuantity, v.Options));

        var approvedReviews = product.Reviews.Where(r => r.Status == Domain.Enums.ReviewStatus.Approved).ToList();
        double? avgRating = approvedReviews.Count > 0 ? approvedReviews.Average(r => r.Rating) : null;

        return Result.Success(new ProductDetailDto(
            product.Id, product.Name, product.Slug, product.Sku,
            product.Price.Amount, product.CompareAtPrice?.Amount, product.Price.Currency,
            product.StockQuantity, product.Status, product.Description,
            product.Category?.Name ?? string.Empty, product.Attributes,
            variants, avgRating, approvedReviews.Count));
    }
}

public record SearchProductsQuery(string Term, int Page = 1, int PageSize = 20)
    : IRequest<Result<PagedResult<ProductSummaryDto>>>;

public sealed class SearchProductsQueryHandler(IUnitOfWork uow)
    : IRequestHandler<SearchProductsQuery, Result<PagedResult<ProductSummaryDto>>>
{
    public async Task<Result<PagedResult<ProductSummaryDto>>> Handle(SearchProductsQuery query, CancellationToken ct)
    {
        var products = (await uow.Products.SearchAsync(query.Term, ct)).ToList();
        var total = products.Count;
        var paged = products
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(p => new ProductSummaryDto(
                p.Id, p.Name, p.Slug, p.Sku,
                p.Price.Amount, p.Price.Currency,
                p.StockQuantity, p.Status,
                p.Category?.Name ?? string.Empty,
                p.Attributes))
            .ToList();

        return Result.Success(new PagedResult<ProductSummaryDto>
        {
            Items = paged,
            TotalCount = total,
            Page = query.Page,
            PageSize = query.PageSize
        });
    }
}

public record GetProductsByCategoryQuery(Guid CategoryId, int Page = 1, int PageSize = 20)
    : IRequest<Result<PagedResult<ProductSummaryDto>>>;

public sealed class GetProductsByCategoryQueryHandler(IUnitOfWork uow)
    : IRequestHandler<GetProductsByCategoryQuery, Result<PagedResult<ProductSummaryDto>>>
{
    public async Task<Result<PagedResult<ProductSummaryDto>>> Handle(GetProductsByCategoryQuery query, CancellationToken ct)
    {
        var products = (await uow.Products.GetByCategoryAsync(query.CategoryId, ct)).ToList();
        var paged = products
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(p => new ProductSummaryDto(
                p.Id, p.Name, p.Slug, p.Sku,
                p.Price.Amount, p.Price.Currency,
                p.StockQuantity, p.Status,
                p.Category?.Name ?? string.Empty,
                p.Attributes))
            .ToList();

        return Result.Success(new PagedResult<ProductSummaryDto>
        {
            Items = paged,
            TotalCount = products.Count,
            Page = query.Page,
            PageSize = query.PageSize
        });
    }
}
