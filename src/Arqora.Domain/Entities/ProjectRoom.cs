using Arqora.Domain.Common;
using Arqora.Domain.Enums;

namespace Arqora.Domain.Entities;

/// <summary>
/// Represents a single room within a project. Separating rooms into their
/// own entity (instead of embedding them as a JSON blob inside Project)
/// lets us query, filter, and report on rooms independently — e.g. "show
/// me the average kitchen cost across all projects."
/// </summary>
public class ProjectRoom : BaseEntity
{
    public RoomType RoomType { get; set; }

    /// <summary>
    /// Allows users to override the default enum name. For example a
    /// homeowner might call their second bedroom "Ananya's Room" instead
    /// of "KidsBedroom." The enum still drives business rules; the custom
    /// name is purely for display.
    /// </summary>
    public string? CustomName { get; set; }

    public decimal AreaSqFt { get; set; }

    /// <summary>
    /// Defaults to 10 ft — a common residential ceiling height in India.
    /// Nullable so that EF Core can distinguish "not yet set" from "set
    /// to 10." The application layer should coalesce null to 10 when
    /// generating quotations.
    /// </summary>
    public decimal? CeilingHeightFt { get; set; } = 10;

    public Guid ProjectId { get; set; }
    public Project Project { get; set; } = null!;

    public ICollection<RoomItem> Items { get; set; } = new List<RoomItem>();
}
