using EcomPostgresDb.Domain.Enums;
using EcomPostgresDb.Domain.ValueObjects;

namespace EcomPostgresDb.Domain.Entities;

/// <summary>
/// Coupon/promotion code entity.
/// Strategy Pattern: the discount calculation strategy is determined by DiscountType.
/// </summary>
public sealed class Coupon : BaseEntity
{
    public string Code { get; private set; } = default!;
    public string Description { get; private set; } = default!;
    public DiscountType DiscountType { get; private set; }
    public decimal DiscountValue { get; private set; }
    public Money? MinimumOrderAmount { get; private set; }
    public int? UsageLimit { get; private set; }
    public int UsedCount { get; private set; }
    public DateTime? ExpiresAt { get; private set; }
    public bool IsActive { get; private set; } = true;

    private Coupon() { }

    public static Coupon Create(string code, string description, DiscountType type,
        decimal value, int? usageLimit = null, DateTime? expiresAt = null,
        Money? minimumOrderAmount = null) =>
        new()
        {
            Code = code.Trim().ToUpperInvariant(),
            Description = description,
            DiscountType = type,
            DiscountValue = value,
            UsageLimit = usageLimit,
            ExpiresAt = expiresAt,
            MinimumOrderAmount = minimumOrderAmount
        };

    public bool IsValid(Money orderAmount)
    {
        if (!IsActive) return false;
        if (ExpiresAt.HasValue && ExpiresAt.Value < DateTime.UtcNow) return false;
        if (UsageLimit.HasValue && UsedCount >= UsageLimit.Value) return false;
        if (MinimumOrderAmount is not null && orderAmount.Amount < MinimumOrderAmount.Amount) return false;
        return true;
    }

    /// <summary>Strategy: calculates discount based on type.</summary>
    public decimal CalculateDiscount(decimal orderTotal) =>
        DiscountType switch
        {
            DiscountType.Percentage => Math.Round(orderTotal * DiscountValue / 100, 2),
            DiscountType.FixedAmount => Math.Min(DiscountValue, orderTotal),
            _ => 0
        };

    public void IncrementUsage() { UsedCount++; SetUpdatedAt(); }
    public void Deactivate() { IsActive = false; SetUpdatedAt(); }
}
