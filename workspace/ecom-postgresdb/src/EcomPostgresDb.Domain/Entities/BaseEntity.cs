namespace EcomPostgresDb.Domain.Entities;

/// <summary>
/// Base entity with audit fields and domain event support.
/// All domain entities inherit from this class.
/// </summary>
public abstract class BaseEntity
{
    public Guid Id { get; protected set; } = Guid.NewGuid();
    public DateTime CreatedAt { get; protected set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; protected set; } = DateTime.UtcNow;

    private readonly List<IDomainEvent> _domainEvents = [];

    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    protected void AddDomainEvent(IDomainEvent domainEvent) => _domainEvents.Add(domainEvent);

    public void ClearDomainEvents() => _domainEvents.Clear();

    protected void SetUpdatedAt() => UpdatedAt = DateTime.UtcNow;
}

/// <summary>
/// Marker interface for domain events (Observer pattern).
/// </summary>
public interface IDomainEvent { }
