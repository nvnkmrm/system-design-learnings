using EcomPostgresDb.Domain.Entities;
using EcomPostgresDb.Domain.Events;

namespace EcomPostgresDb.Domain.Entities;

/// <summary>
/// Customer aggregate root.
/// Manages account credentials and shipping addresses.
/// </summary>
public sealed class Customer : BaseEntity
{
    public string FirstName { get; private set; } = default!;
    public string LastName { get; private set; } = default!;
    public string Email { get; private set; } = default!;
    public string PasswordHash { get; private set; } = default!;
    public string? PhoneNumber { get; private set; }
    public bool IsEmailVerified { get; private set; }
    public bool IsActive { get; private set; } = true;

    // Navigation properties
    private readonly List<Order> _orders = [];
    private readonly List<CustomerAddress> _addresses = [];
    private readonly List<CartItem> _cartItems = [];

    public IReadOnlyCollection<Order> Orders => _orders.AsReadOnly();
    public IReadOnlyCollection<CustomerAddress> Addresses => _addresses.AsReadOnly();
    public IReadOnlyCollection<CartItem> CartItems => _cartItems.AsReadOnly();

    // EF Core constructor
    private Customer() { }

    public static Customer Create(string firstName, string lastName, string email, string passwordHash)
    {
        var customer = new Customer
        {
            FirstName = firstName.Trim(),
            LastName = lastName.Trim(),
            Email = email.Trim().ToLowerInvariant(),
            PasswordHash = passwordHash
        };
        customer.AddDomainEvent(new CustomerRegisteredEvent(customer.Id, customer.Email));
        return customer;
    }

    public void UpdateProfile(string firstName, string lastName, string? phoneNumber)
    {
        FirstName = firstName.Trim();
        LastName = lastName.Trim();
        PhoneNumber = phoneNumber?.Trim();
        SetUpdatedAt();
    }

    public void VerifyEmail()
    {
        IsEmailVerified = true;
        SetUpdatedAt();
    }

    public void Deactivate()
    {
        IsActive = false;
        SetUpdatedAt();
    }

    public string FullName => $"{FirstName} {LastName}";
}
