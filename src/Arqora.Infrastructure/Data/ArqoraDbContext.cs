using Arqora.Application.Interfaces;
using Arqora.Domain.Common;
using Arqora.Domain.Entities;
using Arqora.Domain.Enums;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Arqora.Infrastructure.Data;

/// <summary>
/// The EF Core DbContext is the "Unit of Work" + "Repository" that bridges
/// the domain model and the database. Inheriting from IdentityDbContext
/// gives us all the ASP.NET Identity tables (Users, Roles, Claims, etc.)
/// for free, while we add our own domain-specific DbSets on top.
///
/// Implements IArqoraDbContext so the Application layer can depend on an
/// abstraction rather than this concrete class (Dependency Inversion).
///
/// KEY PATTERN: We override OnModelCreating for Fluent API configuration
/// instead of relying on data annotations on entities. This keeps the
/// domain entities clean — they know nothing about persistence details
/// like column types, indices, or cascade rules.
/// </summary>
public class ArqoraDbContext : IdentityDbContext<AppUser>, IArqoraDbContext
{
    public ArqoraDbContext(DbContextOptions<ArqoraDbContext> options) : base(options) { }

    // Each DbSet<T> maps a domain entity to a database table.
    // EF Core uses these properties to discover the entity types and
    // generate the corresponding schema.
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<ProjectRoom> ProjectRooms => Set<ProjectRoom>();
    public DbSet<Material> Materials => Set<Material>();
    public DbSet<RoomItem> RoomItems => Set<RoomItem>();
    public DbSet<Quotation> Quotations => Set<Quotation>();
    public DbSet<QuotationLineItem> QuotationLineItems => Set<QuotationLineItem>();
    public DbSet<RateTemplate> RateTemplates => Set<RateTemplate>();
    public DbSet<RateTemplateItem> RateTemplateItems => Set<RateTemplateItem>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        // IMPORTANT: Always call base.OnModelCreating first when inheriting
        // from IdentityDbContext — it configures all the Identity tables.
        // Forgetting this call is a common source of runtime errors.
        base.OnModelCreating(builder);

        // ──────────────────────────────────────────────────────────
        // WHY STRING CONVERSION FOR ENUMS IN SQLITE?
        // ──────────────────────────────────────────────────────────
        // By default EF Core stores enums as integers (0, 1, 2…). While
        // this is compact, it makes the database unreadable when inspected
        // directly — you'd see "Category = 3" instead of "Category = Painting".
        //
        // SQLite is especially affected because it lacks a native enum type
        // and its tooling (DB Browser, sqlite3 CLI) is widely used during
        // development. Storing readable strings makes debugging, manual
        // queries, and seed-data verification dramatically easier.
        //
        // The minor storage overhead is negligible for a project of this
        // scale. In a high-throughput production system you might revisit
        // this in favour of integer storage with an enum lookup table.
        // ──────────────────────────────────────────────────────────

        ConfigureProject(builder);
        ConfigureProjectRoom(builder);
        ConfigureMaterial(builder);
        ConfigureRoomItem(builder);
        ConfigureQuotation(builder);
        ConfigureQuotationLineItem(builder);
        ConfigureRateTemplate(builder);
        ConfigureRateTemplateItem(builder);
    }

    // ─── Project ──────────────────────────────────────────────
    private static void ConfigureProject(ModelBuilder builder)
    {
        builder.Entity<Project>(e =>
        {
            // Decimal precision (18,2) is the standard for money columns —
            // 18 total digits with 2 decimal places. This avoids rounding
            // errors that float/double would introduce.
            e.Property(p => p.TotalAreaSqFt).HasPrecision(18, 2);

            // FK to AppUser. Restrict delete means we can't accidentally
            // delete a user who still owns projects.
            e.HasOne(p => p.Owner)
             .WithMany(u => u.Projects)
             .HasForeignKey(p => p.AppUserId)
             .OnDelete(DeleteBehavior.Restrict);
        });
    }

    // ─── ProjectRoom ──────────────────────────────────────────
    private static void ConfigureProjectRoom(ModelBuilder builder)
    {
        builder.Entity<ProjectRoom>(e =>
        {
            e.Property(r => r.AreaSqFt).HasPrecision(18, 2);
            e.Property(r => r.CeilingHeightFt).HasPrecision(18, 2);

            // Store the RoomType enum as a human-readable string.
            e.Property(r => r.RoomType)
             .HasConversion<string>();

            // Cascade delete: when a project is deleted, its rooms go too.
            // This matches the domain rule that rooms can't exist without
            // their parent project.
            e.HasOne(r => r.Project)
             .WithMany(p => p.Rooms)
             .HasForeignKey(r => r.ProjectId)
             .OnDelete(DeleteBehavior.Cascade);
        });
    }

    // ─── Material ─────────────────────────────────────────────
    private static void ConfigureMaterial(ModelBuilder builder)
    {
        builder.Entity<Material>(e =>
        {
            e.Property(m => m.BasicRate).HasPrecision(18, 2);
            e.Property(m => m.StandardRate).HasPrecision(18, 2);
            e.Property(m => m.PremiumRate).HasPrecision(18, 2);

            e.Property(m => m.Category).HasConversion<string>();
            e.Property(m => m.Unit).HasConversion<string>();

            // Index on Name speeds up material search/autocomplete queries.
            e.HasIndex(m => m.Name);
        });
    }

    // ─── RoomItem ─────────────────────────────────────────────
    private static void ConfigureRoomItem(ModelBuilder builder)
    {
        builder.Entity<RoomItem>(e =>
        {
            e.Property(ri => ri.Quantity).HasPrecision(18, 2);
            e.Property(ri => ri.CustomBasicRate).HasPrecision(18, 2);
            e.Property(ri => ri.CustomStandardRate).HasPrecision(18, 2);
            e.Property(ri => ri.CustomPremiumRate).HasPrecision(18, 2);

            e.Property(ri => ri.Category).HasConversion<string>();
            e.Property(ri => ri.Unit).HasConversion<string>();

            // Cascade: deleting a room removes its items.
            e.HasOne(ri => ri.ProjectRoom)
             .WithMany(r => r.Items)
             .HasForeignKey(ri => ri.ProjectRoomId)
             .OnDelete(DeleteBehavior.Cascade);

            // SetNull: if a material is deleted, the room item stays but
            // loses its catalog link. This preserves historical data.
            e.HasOne(ri => ri.Material)
             .WithMany()
             .HasForeignKey(ri => ri.MaterialId)
             .OnDelete(DeleteBehavior.SetNull);
        });
    }

    // ─── Quotation ────────────────────────────────────────────
    private static void ConfigureQuotation(ModelBuilder builder)
    {
        builder.Entity<Quotation>(e =>
        {
            e.Property(q => q.SubTotal).HasPrecision(18, 2);
            e.Property(q => q.TaxPercentage).HasPrecision(5, 2);
            e.Property(q => q.TaxAmount).HasPrecision(18, 2);
            e.Property(q => q.GrandTotal).HasPrecision(18, 2);
            e.Property(q => q.DiscountPercentage).HasPrecision(5, 2);
            e.Property(q => q.DiscountAmount).HasPrecision(18, 2);

            e.Property(q => q.Tier).HasConversion<string>();
            e.Property(q => q.Status).HasConversion<string>();

            // Unique index ensures no two quotations share the same number.
            // This is the database-level safety net; the application layer
            // also prevents duplicates via QuotationNumberGenerator.
            e.HasIndex(q => q.QuotationNumber).IsUnique();

            // Restrict: can't delete a project that has quotations.
            e.HasOne(q => q.Project)
             .WithMany(p => p.Quotations)
             .HasForeignKey(q => q.ProjectId)
             .OnDelete(DeleteBehavior.Restrict);

            // A quotation's designer is optional (homeowner-generated quotes).
            e.HasOne(q => q.Designer)
             .WithMany(u => u.CreatedQuotations)
             .HasForeignKey(q => q.DesignerId)
             .OnDelete(DeleteBehavior.SetNull);
        });
    }

    // ─── QuotationLineItem ────────────────────────────────────
    private static void ConfigureQuotationLineItem(ModelBuilder builder)
    {
        builder.Entity<QuotationLineItem>(e =>
        {
            e.Property(li => li.Quantity).HasPrecision(18, 2);
            e.Property(li => li.UnitRate).HasPrecision(18, 2);
            e.Property(li => li.TotalAmount).HasPrecision(18, 2);

            e.Property(li => li.RoomType).HasConversion<string>();
            e.Property(li => li.Category).HasConversion<string>();
            e.Property(li => li.Unit).HasConversion<string>();

            // Cascade: deleting a quotation removes all its line items.
            e.HasOne(li => li.Quotation)
             .WithMany(q => q.LineItems)
             .HasForeignKey(li => li.QuotationId)
             .OnDelete(DeleteBehavior.Cascade);
        });
    }

    // ─── RateTemplate ─────────────────────────────────────────
    private static void ConfigureRateTemplate(ModelBuilder builder)
    {
        builder.Entity<RateTemplate>(e =>
        {
            e.HasIndex(rt => rt.Name);
        });
    }

    // ─── RateTemplateItem ─────────────────────────────────────
    private static void ConfigureRateTemplateItem(ModelBuilder builder)
    {
        builder.Entity<RateTemplateItem>(e =>
        {
            e.Property(rti => rti.BasicRate).HasPrecision(18, 2);
            e.Property(rti => rti.StandardRate).HasPrecision(18, 2);
            e.Property(rti => rti.PremiumRate).HasPrecision(18, 2);

            e.Property(rti => rti.Category).HasConversion<string>();
            e.Property(rti => rti.Unit).HasConversion<string>();
            e.Property(rti => rti.ApplicableRoomType).HasConversion<string>();

            // Cascade: deleting a template removes all its items.
            e.HasOne(rti => rti.RateTemplate)
             .WithMany(rt => rt.Items)
             .HasForeignKey(rti => rti.RateTemplateId)
             .OnDelete(DeleteBehavior.Cascade);
        });
    }

    // ──────────────────────────────────────────────────────────
    // AUTOMATIC AUDIT FIELDS
    // ──────────────────────────────────────────────────────────
    // Overriding SaveChangesAsync lets us intercept every save operation
    // and automatically stamp CreatedAt/UpdatedAt on entities that inherit
    // from BaseEntity. This is the "auditing interceptor" pattern — it
    // centralises audit logic so individual services never forget to set
    // these timestamps.
    // ──────────────────────────────────────────────────────────
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = DateTime.UtcNow;
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                    break;

                case EntityState.Modified:
                    // Only touch UpdatedAt — CreatedAt is immutable after creation.
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                    break;
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}
