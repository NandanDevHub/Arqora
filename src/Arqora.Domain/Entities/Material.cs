using Arqora.Domain.Common;
using Arqora.Domain.Enums;

namespace Arqora.Domain.Entities;

/// <summary>
/// A catalog entry for a material or product. Materials are intentionally
/// kept separate from room items so that the same material can be reused
/// across many rooms and projects — this is the classic "catalog pattern."
///
/// Storing three rate columns (Basic / Standard / Premium) directly on
/// the entity avoids a separate pricing table and keeps lookups simple.
/// If pricing complexity grows (e.g. regional rates, date-based pricing),
/// this could be refactored into a Strategy or Specification pattern.
/// </summary>
public class Material : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Brand { get; set; }
    public WorkCategory Category { get; set; }
    public UnitOfMeasurement Unit { get; set; }

    public decimal BasicRate { get; set; }
    public decimal StandardRate { get; set; }
    public decimal PremiumRate { get; set; }

    public bool IsActive { get; set; } = true;
    public string? ImageUrl { get; set; }
}
