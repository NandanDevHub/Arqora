using Arqora.Application.Interfaces;
using Arqora.Domain.Entities;
using Arqora.Infrastructure.Data;
using Arqora.Infrastructure.Identity;
using Arqora.Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Arqora.Infrastructure;

/// <summary>
/// COMPOSITION ROOT PATTERN
/// ────────────────────────
/// This static extension method is the single place where all Infrastructure
/// services are wired into the DI container. The API layer calls
/// builder.Services.AddInfrastructureServices(configuration) and gets
/// everything it needs — DbContext, Identity, JWT, PDF generation — without
/// knowing any implementation details.
///
/// This keeps the API layer's Program.cs clean and makes it trivial to
/// swap implementations (e.g. replace SQLite with PostgreSQL) by changing
/// only this file.
/// </summary>
public static class InfrastructureServiceRegistration
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // ── 1. Database ───────────────────────────────────────
        // SQLite is used for development simplicity — no server to install.
        // For production, swap UseSqlite for UseSqlServer / UseNpgsql.
        services.AddDbContext<ArqoraDbContext>(options =>
            options.UseSqlite(
                configuration.GetConnectionString("DefaultConnection"),
                // Grouping migrations in a specific assembly keeps the
                // Infrastructure project self-contained.
                b => b.MigrationsAssembly(typeof(ArqoraDbContext).Assembly.FullName)));

        // ── 2. ASP.NET Core Identity ──────────────────────────
        // AddIdentity registers UserManager, SignInManager, RoleManager,
        // and the default token providers for password reset, email
        // confirmation, etc.
        services.AddIdentity<AppUser, IdentityRole>(options =>
        {
            // Password rules — relaxed for development, tighten for production.
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireUppercase = true;
            options.Password.RequireNonAlphanumeric = true;
            options.Password.RequiredLength = 6;

            // User settings
            options.User.RequireUniqueEmail = true;
        })
        .AddEntityFrameworkStores<ArqoraDbContext>()
        .AddDefaultTokenProviders();

        // ── 3. Application services ───────────────────────────
        // Scoped lifetime = one instance per HTTP request. This matches
        // the DbContext lifetime and avoids threading issues.
        services.AddScoped<IArqoraDbContext>(provider => provider.GetRequiredService<ArqoraDbContext>());
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IQuotationPdfService, QuotationPdfService>();
        services.AddScoped<IQuotationNumberGenerator, QuotationNumberGenerator>();

        return services;
    }
}
