namespace EcomPostgresDb.Domain.ValueObjects;

/// <summary>
/// Value Object: Address — embedded in orders and customer profiles.
/// Stored as an owned entity (flattened columns) in PostgreSQL.
/// </summary>
public sealed class Address : IEquatable<Address>
{
    public string Line1 { get; }
    public string? Line2 { get; }
    public string City { get; }
    public string State { get; }
    public string PostalCode { get; }
    public string Country { get; }

    public Address(string line1, string? line2, string city, string state, string postalCode, string country)
    {
        Line1 = line1 ?? throw new ArgumentNullException(nameof(line1));
        Line2 = line2;
        City = city ?? throw new ArgumentNullException(nameof(city));
        State = state ?? throw new ArgumentNullException(nameof(state));
        PostalCode = postalCode ?? throw new ArgumentNullException(nameof(postalCode));
        Country = country ?? throw new ArgumentNullException(nameof(country));
    }

    public bool Equals(Address? other) =>
        other is not null &&
        Line1 == other.Line1 &&
        Line2 == other.Line2 &&
        City == other.City &&
        State == other.State &&
        PostalCode == other.PostalCode &&
        Country == other.Country;

    public override bool Equals(object? obj) => obj is Address a && Equals(a);
    public override int GetHashCode() => HashCode.Combine(Line1, City, State, PostalCode, Country);
    public override string ToString() => $"{Line1}, {City}, {State} {PostalCode}, {Country}";
}
