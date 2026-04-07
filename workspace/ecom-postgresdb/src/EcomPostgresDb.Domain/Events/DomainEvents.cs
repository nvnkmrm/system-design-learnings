using EcomPostgresDb.Domain.Entities;
using EcomPostgresDb.Domain.ValueObjects;

namespace EcomPostgresDb.Domain.Events;

/// <summary>Domain events implement the Observer pattern via MediatR.</summary>
/// 
public record CustomerRegisteredEvent(Guid CustomerId, string Email) : IDomainEvent;

public record ProductCreatedEvent(Guid ProductId, string Name) : IDomainEvent;

public record OrderCreatedEvent(Guid OrderId, Guid CustomerId) : IDomainEvent;

public record OrderConfirmedEvent(Guid OrderId, Guid CustomerId, Money Total) : IDomainEvent;

public record OrderShippedEvent(Guid OrderId, Guid CustomerId, string TrackingNumber) : IDomainEvent;

public record OrderCancelledEvent(Guid OrderId, Guid CustomerId, string Reason) : IDomainEvent;

public record PaymentCapturedEvent(Guid PaymentId, Guid OrderId, Money Amount) : IDomainEvent;

public record LowStockAlertEvent(Guid ProductId, string ProductName, int CurrentStock) : IDomainEvent;
