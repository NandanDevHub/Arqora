using Arqora.Domain.Entities;
using Arqora.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Arqora.Infrastructure.Data.Seed;

/// <summary>
/// WHY SEED DATA MATTERS FOR DEVELOPMENT
/// ──────────────────────────────────────
/// Seed data provides a consistent starting point so every developer (and
/// every CI pipeline) works against the same baseline. Without it:
///   • New developers spend time manually creating test users and materials.
///   • UI work stalls because there's nothing to display.
///   • Integration tests are fragile because they depend on database state.
///
/// REALISTIC PRICING APPROACH
/// ──────────────────────────
/// The rates below reflect actual 2025-2026 Indian interior design market
/// prices (metro cities like Bangalore, Mumbai, Hyderabad). Using realistic
/// numbers means:
///   • Designers can demo the app with believable data.
///   • Totals on generated quotations look plausible (₹5-25 lakh range).
///   • Students learning from this codebase understand real-world pricing.
///
/// Sources: HomeLane, Livspace, DesignCafe published rate cards.
/// </summary>
public static class DataSeeder
{
    public static async Task SeedAsync(
        ArqoraDbContext context,
        UserManager<AppUser> userManager,
        RoleManager<IdentityRole> roleManager)
    {
        // Ensure the database schema is up to date.
        await context.Database.MigrateAsync();

        await SeedRolesAsync(roleManager);
        await SeedUsersAsync(userManager);
        await SeedMaterialsAsync(context);
        await SeedRateTemplateAsync(context);
    }

    // ─── Roles ────────────────────────────────────────────────
    private static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
    {
        string[] roles = ["Admin", "Designer", "Homeowner"];

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }
    }

    // ─── Users ────────────────────────────────────────────────
    private static async Task SeedUsersAsync(UserManager<AppUser> userManager)
    {
        await CreateUserIfNotExists(userManager, new AppUser
        {
            UserName = "admin@arqora.com",
            Email = "admin@arqora.com",
            FullName = "Arqora Admin",
            Role = UserRole.Admin,
            EmailConfirmed = true,
            IsActive = true
        }, "Admin@123", "Admin");

        await CreateUserIfNotExists(userManager, new AppUser
        {
            UserName = "designer@arqora.com",
            Email = "designer@arqora.com",
            FullName = "Priya Sharma",
            Role = UserRole.Designer,
            CompanyName = "Sharma Interiors",
            PhoneNumber = "9876543210",
            City = "Bangalore",
            EmailConfirmed = true,
            IsActive = true
        }, "Designer@123", "Designer");

        await CreateUserIfNotExists(userManager, new AppUser
        {
            UserName = "homeowner@arqora.com",
            Email = "homeowner@arqora.com",
            FullName = "Rahul Mehta",
            Role = UserRole.Homeowner,
            PhoneNumber = "9123456789",
            City = "Mumbai",
            EmailConfirmed = true,
            IsActive = true
        }, "Home@123", "Homeowner");
    }

    private static async Task CreateUserIfNotExists(
        UserManager<AppUser> userManager,
        AppUser user,
        string password,
        string role)
    {
        if (await userManager.FindByEmailAsync(user.Email!) is null)
        {
            var result = await userManager.CreateAsync(user, password);
            if (result.Succeeded)
                await userManager.AddToRoleAsync(user, role);
        }
    }

    // ─── Materials ────────────────────────────────────────────
    // Rates are per unit (₹/sqft, ₹/piece, ₹/set, etc.)
    // Basic = economy-grade, Standard = mid-range, Premium = luxury
    private static async Task SeedMaterialsAsync(ArqoraDbContext context)
    {
        if (await context.Materials.AnyAsync()) return;

        var materials = new List<Material>
        {
            // ── Furniture ─────────────────────────────────────
            Mat("Wardrobe (Sliding)", WorkCategory.Furniture, UnitOfMeasurement.SquareFeet,
                950, 1400, 2200, "?"," Sliding mechanism with soft-close"),
            Mat("Wardrobe (Openable)", WorkCategory.Furniture, UnitOfMeasurement.SquareFeet,
                850, 1250, 1900),
            Mat("TV Unit", WorkCategory.Furniture, UnitOfMeasurement.SquareFeet,
                800, 1200, 1800),
            Mat("Shoe Rack", WorkCategory.Furniture, UnitOfMeasurement.Piece,
                4500, 8500, 15000),
            Mat("Study Table", WorkCategory.Furniture, UnitOfMeasurement.Piece,
                6000, 12000, 22000),
            Mat("Bookshelf", WorkCategory.Furniture, UnitOfMeasurement.SquareFeet,
                750, 1100, 1700),
            Mat("Crockery Unit", WorkCategory.Furniture, UnitOfMeasurement.SquareFeet,
                900, 1350, 2100),
            Mat("Mandir / Pooja Unit", WorkCategory.Furniture, UnitOfMeasurement.Piece,
                12000, 25000, 50000),
            Mat("Kitchen Cabinets (Per sqft)", WorkCategory.Furniture, UnitOfMeasurement.SquareFeet,
                800, 1300, 2000),
            Mat("Loft Storage", WorkCategory.Furniture, UnitOfMeasurement.SquareFeet,
                550, 850, 1300),

            // ── Modular ───────────────────────────────────────
            Mat("Modular Kitchen – L-Shaped", WorkCategory.Modular, UnitOfMeasurement.Set,
                125000, 210000, 350000, description: "8-ft × 6-ft L-shaped layout with accessories"),
            Mat("Modular Kitchen – U-Shaped", WorkCategory.Modular, UnitOfMeasurement.Set,
                175000, 280000, 450000, description: "10-ft × 8-ft × 6-ft U-shaped layout"),
            Mat("Modular Kitchen – Straight", WorkCategory.Modular, UnitOfMeasurement.Set,
                85000, 150000, 250000, description: "8-ft straight/single-wall layout"),

            // ── Flooring ──────────────────────────────────────
            Mat("Vitrified Tiles", WorkCategory.Flooring, UnitOfMeasurement.SquareFeet,
                45, 75, 120),
            Mat("Wooden Flooring (Laminate)", WorkCategory.Flooring, UnitOfMeasurement.SquareFeet,
                75, 130, 220, brand: "Pergo"),
            Mat("Marble Flooring", WorkCategory.Flooring, UnitOfMeasurement.SquareFeet,
                90, 180, 350, description: "Italian / Makrana marble with polishing"),

            // ── Painting ──────────────────────────────────────
            Mat("Interior Wall Paint", WorkCategory.Painting, UnitOfMeasurement.SquareFeet,
                22, 35, 55, brand: "Asian Paints"),
            Mat("Texture Paint", WorkCategory.Painting, UnitOfMeasurement.SquareFeet,
                40, 65, 100, brand: "Asian Paints Royale"),
            Mat("POP Punning (Wall/Ceiling)", WorkCategory.Painting, UnitOfMeasurement.SquareFeet,
                18, 28, 42),

            // ── False Ceiling ─────────────────────────────────
            Mat("Gypsum False Ceiling", WorkCategory.FalseCeiling, UnitOfMeasurement.SquareFeet,
                65, 95, 145, brand: "Gyproc / Saint-Gobain"),
            Mat("POP False Ceiling", WorkCategory.FalseCeiling, UnitOfMeasurement.SquareFeet,
                55, 80, 120),

            // ── Electrical ────────────────────────────────────
            Mat("Electrical Point", WorkCategory.Electrical, UnitOfMeasurement.Piece,
                350, 550, 850, description: "Per point including wiring, conduit, and plate"),
            Mat("Switchboard (Modular)", WorkCategory.Electrical, UnitOfMeasurement.Piece,
                800, 1500, 2500, brand: "Legrand / Schneider"),

            // ── Plumbing ──────────────────────────────────────
            Mat("CP Fittings (Bathroom Set)", WorkCategory.Plumbing, UnitOfMeasurement.Set,
                8000, 15000, 30000, brand: "Jaquar"),
            Mat("Sanitary Ware (WC + Basin)", WorkCategory.Plumbing, UnitOfMeasurement.Set,
                12000, 22000, 45000, brand: "Kohler / Duravit"),

            // ── Lighting ──────────────────────────────────────
            Mat("LED Downlights (6W)", WorkCategory.Lighting, UnitOfMeasurement.Piece,
                250, 500, 900, brand: "Philips / Wipro"),
            Mat("Chandelier", WorkCategory.Lighting, UnitOfMeasurement.Piece,
                5000, 15000, 40000),
            Mat("Cove Lighting (LED Strip)", WorkCategory.Lighting, UnitOfMeasurement.RunningFeet,
                80, 150, 280, description: "Per running foot including profile and driver"),
        };

        context.Materials.AddRange(materials);
        await context.SaveChangesAsync();
    }

    /// <summary>
    /// Helper factory to keep the material list concise.
    /// </summary>
    private static Material Mat(
        string name,
        WorkCategory category,
        UnitOfMeasurement unit,
        decimal basic,
        decimal standard,
        decimal premium,
        string? brand = null,
        string? description = null) => new()
    {
        Name = name,
        Category = category,
        Unit = unit,
        BasicRate = basic,
        StandardRate = standard,
        PremiumRate = premium,
        Brand = brand,
        Description = description,
        IsActive = true
    };

    // ─── Rate Template ────────────────────────────────────────
    private static async Task SeedRateTemplateAsync(ArqoraDbContext context)
    {
        if (await context.RateTemplates.AnyAsync()) return;

        var template = new RateTemplate
        {
            Name = "Standard India Rates 2026",
            Description = "Default rate card based on metro-city averages (Bangalore, Mumbai, Hyderabad) for 2025-2026.",
            IsDefault = true,
            IsActive = true
        };

        context.RateTemplates.Add(template);
        await context.SaveChangesAsync();

        // Build template items that mirror the seeded materials.
        var materials = await context.Materials.ToListAsync();
        var templateItems = materials.Select(m => new RateTemplateItem
        {
            RateTemplateId = template.Id,
            ItemName = m.Name,
            Category = m.Category,
            Unit = m.Unit,
            BasicRate = m.BasicRate,
            StandardRate = m.StandardRate,
            PremiumRate = m.PremiumRate,
            ApplicableRoomType = null // General rates — applicable to all rooms.
        });

        context.RateTemplateItems.AddRange(templateItems);
        await context.SaveChangesAsync();
    }
}
