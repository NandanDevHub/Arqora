using Arqora.Domain.Common;
using Arqora.Domain.Enums;

namespace Arqora.Domain.Entities;

/// <summary>
/// An item the homeowner/designer has selected for a specific room.
///
/// This entity bridges the gap between the Material catalog and the
/// room configuration. The optional Material FK lets users pick from the
/// catalog, while the Custom*Rate properties let designers override
/// catalog rates on a per-item basis — a real-world requirement where
/// designers negotiate special rates with vendors.
/// </summary>
public class RoomItem : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public WorkCategory Category { get; set; }
    public decimal Quantity { get; set; }
    public UnitOfMeasurement Unit { get; set; }

    public Guid ProjectRoomId { get; set; }
    public ProjectRoom ProjectRoom { get; set; } = null!;

    // Nullable FK: a room item may or may not be linked to a catalog material.
    // This keeps the entity flexible — custom/one-off items can exist
    // without polluting the material catalog.
    public Guid? MaterialId { get; set; }
    public Material? Material { get; set; }

    // Custom rates override the material's catalog rates when present.
    // The quotation generator should prefer these over Material.*Rate
    // when they are non-null.
    public decimal? CustomBasicRate { get; set; }
    public decimal? CustomStandardRate { get; set; }
    public decimal? CustomPremiumRate { get; set; }
}
