using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Arqora.Application.Interfaces;
using Arqora.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Arqora.Infrastructure.Identity;

/// <summary>
/// JWT (JSON Web Token) STRUCTURE — A QUICK PRIMER
/// ────────────────────────────────────────────────
/// A JWT has three parts separated by dots: Header.Payload.Signature
///
///   Header   – algorithm (HS256) and token type (JWT).
///   Payload  – claims: key-value pairs that describe the user.
///   Signature – HMAC-SHA256 hash of Header + Payload using a secret key.
///
/// WHY CLAIMS MATTER
/// ─────────────────
/// Claims are the data the API trusts without hitting the database on every
/// request. By embedding userId, email, fullName, and role directly in the
/// token, the API can authorise requests instantly — no DB round-trip needed.
///
/// Trade-off: if a user's role changes, the old token still carries the
/// stale role until it expires. The 7-day expiry below is a pragmatic
/// balance between UX (users don't have to log in constantly) and security
/// (stale claims don't live forever).
/// </summary>
public class TokenService : ITokenService
{
    private readonly IConfiguration _configuration;

    public TokenService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string CreateToken(AppUser user, IList<string> roles)
    {
        // ── 1. Build the claims list ──────────────────────────
        // Each claim becomes a key-value pair inside the JWT payload.
        var claims = new List<Claim>
        {
            // "sub" (subject) is the standard JWT claim for user identity.
            new(JwtRegisteredClaimNames.Sub, user.Id),

            // "email" lets the frontend display the user's email without
            // an extra API call.
            new(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),

            // "jti" (JWT ID) is a unique identifier for THIS specific token.
            // Useful for token revocation lists if we add that feature later.
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),

            // Custom claims — not part of the JWT standard but useful for
            // our application.
            new("fullName", user.FullName),
            new("userId", user.Id),
        };

        // Add one claim per role so [Authorize(Roles = "Admin")] works.
        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        // ── 2. Create the signing credentials ────────────────
        // The secret key MUST be at least 256 bits (32 bytes) for HS256.
        // It's stored in appsettings.json (or better, in environment
        // variables / Azure Key Vault in production).
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]
                ?? throw new InvalidOperationException("Jwt:Key is not configured.")));

        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        // ── 3. Assemble and sign the token ────────────────────
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddDays(7),
            SigningCredentials = credentials,
            Issuer = _configuration["Jwt:Issuer"],
            Audience = _configuration["Jwt:Audience"],
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);

        return tokenHandler.WriteToken(token);
    }
}
