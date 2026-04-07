namespace EcomPostgresDb.Application.Common.Interfaces;

/// <summary>Password hashing abstraction — implementation uses BCrypt in Infrastructure.</summary>
public interface IPasswordHasher
{
    string Hash(string password);
    bool Verify(string password, string hash);
}

/// <summary>JWT token generation abstraction.</summary>
public interface ITokenService
{
    string GenerateToken(Guid userId, string email, string role);
}

/// <summary>Current user context — populated from JWT claims in middleware.</summary>
public interface ICurrentUserService
{
    Guid? UserId { get; }
    string? Email { get; }
    bool IsAuthenticated { get; }
}
