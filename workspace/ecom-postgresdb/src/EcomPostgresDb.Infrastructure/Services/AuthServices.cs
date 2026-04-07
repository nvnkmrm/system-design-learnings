using BCrypt.Net;
using EcomPostgresDb.Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace EcomPostgresDb.Infrastructure.Services;

/// <summary>
/// BCrypt password hasher — uses work factor 12 for a good security/perf balance.
/// </summary>
public sealed class BcryptPasswordHasher : IPasswordHasher
{
    private const int WorkFactor = 12;

    public string Hash(string password) =>
        BCrypt.Net.BCrypt.HashPassword(password, WorkFactor);

    public bool Verify(string password, string hash) =>
        BCrypt.Net.BCrypt.Verify(password, hash);
}

/// <summary>
/// JWT token service — generates HS256 signed tokens with standard claims.
/// </summary>
public sealed class JwtTokenService(IConfiguration config) : ITokenService
{
    public string GenerateToken(Guid userId, string email, string role)
    {
        var secretKey = config["Jwt:SecretKey"]
            ?? throw new InvalidOperationException("JWT SecretKey is not configured.");
        var issuer = config["Jwt:Issuer"] ?? "EcomPostgresDb";
        var audience = config["Jwt:Audience"] ?? "EcomPostgresDb";
        var expiryMinutes = int.Parse(config["Jwt:ExpiryMinutes"] ?? "60");

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, email),
            new Claim(ClaimTypes.Role, role),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
