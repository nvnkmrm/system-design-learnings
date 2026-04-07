using EcomPostgresDb.Domain.Enums;
using EcomPostgresDb.Domain.ValueObjects;
using EcomPostgresDb.Domain.Events;

namespace EcomPostgresDb.Domain.Entities;

/// <summary>
/// Payment entity linked to an order.
/// 
/// Strategy Pattern applied at the application layer for payment processing.
/// This entity just records the outcome.
/// </summary>
public sealed class Payment : BaseEntity
{
    public Guid OrderId { get; private set; }
    public Money Amount { get; private set; } = default!;
    public PaymentMethod Method { get; private set; }
    public PaymentStatus Status { get; private set; } = PaymentStatus.Pending;
    public string? TransactionId { get; private set; }
    public string? GatewayResponse { get; private set; }
    public DateTime? ProcessedAt { get; private set; }

    // Navigation
    public Order Order { get; private set; } = default!;

    private Payment() { }

    public static Payment Create(Guid orderId, Money amount, PaymentMethod method) =>
        new() { OrderId = orderId, Amount = amount, Method = method };

    public void Authorize(string transactionId)
    {
        Status = PaymentStatus.Authorized;
        TransactionId = transactionId;
        ProcessedAt = DateTime.UtcNow;
        SetUpdatedAt();
    }

    public void Capture(string gatewayResponse)
    {
        if (Status != PaymentStatus.Authorized)
            throw new InvalidOperationException("Payment must be authorized before capture.");
        Status = PaymentStatus.Captured;
        GatewayResponse = gatewayResponse;
        SetUpdatedAt();
        AddDomainEvent(new PaymentCapturedEvent(Id, OrderId, Amount));
    }

    public void Fail(string reason)
    {
        Status = PaymentStatus.Failed;
        GatewayResponse = reason;
        SetUpdatedAt();
    }

    public void Refund()
    {
        if (Status != PaymentStatus.Captured)
            throw new InvalidOperationException("Only captured payments can be refunded.");
        Status = PaymentStatus.Refunded;
        SetUpdatedAt();
    }
}
