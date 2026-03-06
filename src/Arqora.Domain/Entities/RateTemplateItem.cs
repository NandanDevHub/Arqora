using Arqora.Domain.Common;
using Arqora.Domain.Enums;

namespace Arqora.Domain.Entities;

/// <summary>
/// A single rate entry inside a template. The optional ApplicableRoomType
/// lets admins define room-specific rates (e.g. kitchen cabinets cost
/// differently than bedroom wardrobes) while still falling back to a
/// general rate when the room type is null.
/// </summary>
public class RateTemplateItem : BaseEntity
{
    public Guid RateTemplateId { get; set; }
    public RateTemplate RateTemplate { get; set; } = null!;

    public string ItemName { get; set; } = string.Empty;
    public WorkCategory Category { get; set; }

    /// <summary>
    /// When null, the rate applies to ALL room types. When set, the rate
    /// applies only to that specific room type — this enables fine-grained
    /// pricing without an explosion of separate templates.
    /// </summary>
    public RoomType? ApplicableRoomType { get; set; }

    public UnitOfMeasurement Unit { get; set; }
    public decimal BasicRate { get; set; }
    public decimal StandardRate { get; set; }
    public decimal PremiumRate { get; set; }
}
