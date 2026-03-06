using Arqora.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Arqora.Application.Interfaces;

/// <summary>
/// DEPENDENCY INVERSION — DBCONTEXT ABSTRACTION
/// ─────────────────────────────────────────────
/// In Clean Architecture the Application layer must NOT reference the
/// Infrastructure layer. But our MediatR handlers need to read/write
/// data. The solution: define an interface here (in Application) that
/// exposes the DbSet properties and SaveChangesAsync, then have the
/// concrete ArqoraDbContext (in Infrastructure) implement it.
///
/// This way the Application layer depends on an abstraction it owns,
/// and the Infrastructure layer "plugs in" the real implementation at
/// runtime via DI. If we ever swap EF Core for Dapper or a different
/// ORM, only the Infrastructure implementation changes — all handlers
/// remain untouched.
/// </summary>
public interface IArqoraDbContext
{
    DbSet<Project> Projects { get; }
    DbSet<ProjectRoom> ProjectRooms { get; }
    DbSet<Material> Materials { get; }
    DbSet<RoomItem> RoomItems { get; }
    DbSet<Quotation> Quotations { get; }
    DbSet<QuotationLineItem> QuotationLineItems { get; }
    DbSet<RateTemplate> RateTemplates { get; }
    DbSet<RateTemplateItem> RateTemplateItems { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
