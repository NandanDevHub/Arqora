using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace Arqora.API.Extensions;

/// <summary>
/// JWT BEARER AUTHENTICATION CONFIGURATION
/// ────────────────────────────────────────
/// Registers the authentication scheme so ASP.NET Core can validate incoming
/// JWT tokens on every request to [Authorize]-protected endpoints.
///
/// LEARNING: The TokenValidationParameters tell the middleware exactly how to
/// validate a token — check the signing key, verify the issuer/audience strings
/// match what we expect, and reject expired tokens. If any check fails the
/// request gets a 401 Unauthorized response before it reaches the controller.
/// </summary>
public static class IdentityServiceExtensions
{
    public static IServiceCollection AddIdentityServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var jwtKey = configuration["Jwt:Key"]
            ?? throw new InvalidOperationException("Jwt:Key is not configured.");

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                // Validate the signing key — prevents token forgery.
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),

                // Validate that the token's "iss" claim matches our issuer.
                ValidateIssuer = true,
                ValidIssuer = configuration["Jwt:Issuer"],

                // Validate that the token's "aud" claim matches our audience.
                ValidateAudience = true,
                ValidAudience = configuration["Jwt:Audience"],

                // Reject expired tokens (default is true, explicit for clarity).
                ValidateLifetime = true,

                // Zero clock skew — tokens expire exactly when they should.
                ClockSkew = TimeSpan.Zero
            };
        });

        services.AddAuthorization();

        return services;
    }
}
