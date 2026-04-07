using EcomPostgresDb.Domain.Enums;

namespace EcomPostgresDb.Domain.Entities;

/// <summary>
/// Customer product review with rating and optional body.
///
/// PostgreSQL: partial index on (ProductId) WHERE Status = 'Approved'
/// for fast average-rating queries.
/// </summary>
public sealed class Review : BaseEntity
{
    public Guid ProductId { get; private set; }
    public Guid CustomerId { get; private set; }
    public int Rating { get; private set; }       // 1–5
    public string? Title { get; private set; }
    public string? Body { get; private set; }
    public ReviewStatus Status { get; private set; } = ReviewStatus.Pending;

    // Navigation
    public Product Product { get; private set; } = default!;
    public Customer Customer { get; private set; } = default!;

    private Review() { }

    public static Review Create(Guid productId, Guid customerId, int rating, string? title, string? body)
    {
        if (rating < 1 || rating > 5)
            throw new ArgumentOutOfRangeException(nameof(rating), "Rating must be between 1 and 5.");
        return new Review
        {
            ProductId = productId,
            CustomerId = customerId,
            Rating = rating,
            Title = title?.Trim(),
            Body = body?.Trim()
        };
    }

    public void Approve() { Status = ReviewStatus.Approved; SetUpdatedAt(); }
    public void Reject() { Status = ReviewStatus.Rejected; SetUpdatedAt(); }
}
