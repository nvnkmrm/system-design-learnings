using EcomPostgresDb.Domain.Enums;
using EcomPostgresDb.Domain.ValueObjects;

namespace EcomPostgresDb.Domain.Entities;

/// <summary>
/// Customer saved address. Supports multiple billing/shipping addresses.
/// Uses owned Address value object for flat column storage.
/// </summary>
public sealed class CustomerAddress : BaseEntity
{
    public Guid CustomerId { get; private set; }
    public AddressType AddressType { get; private set; }
    public Address Address { get; private set; } = default!;
    public bool IsDefault { get; private set; }

    // Navigation
    public Customer Customer { get; private set; } = default!;

    private CustomerAddress() { }

    public static CustomerAddress Create(Guid customerId, AddressType type, Address address, bool isDefault = false) =>
        new()
        {
            CustomerId = customerId,
            AddressType = type,
            Address = address,
            IsDefault = isDefault
        };

    public void SetAsDefault() { IsDefault = true; SetUpdatedAt(); }
    public void UnsetDefault() { IsDefault = false; SetUpdatedAt(); }
}
