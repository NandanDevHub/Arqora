using Arqora.Domain.Common;

namespace Arqora.Domain.Entities;

/// <summary>
/// Aggregate root for a homeowner's interior design project. An aggregate
/// root is the entry point through which all child entities (rooms,
/// quotations) are accessed — this enforces consistency boundaries and
/// makes it clear that a Project "owns" its rooms and quotations.
/// </summary>
public class Project : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Address { get; set; }
    public decimal TotalAreaSqFt { get; set; }

    // Foreign key uses string (not Guid) because IdentityUser's primary
    // key is a string by default. Matching the FK type avoids unnecessary
    // type conversions at the database level.
    public string AppUserId { get; set; } = string.Empty;
    public AppUser Owner { get; set; } = null!;

    public ICollection<ProjectRoom> Rooms { get; set; } = new List<ProjectRoom>();
    public ICollection<Quotation> Quotations { get; set; } = new List<Quotation>();
}
