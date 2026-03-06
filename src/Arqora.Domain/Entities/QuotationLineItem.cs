using Arqora.Domain.Common;
using Arqora.Domain.Enums;

namespace Arqora.Domain.Entities;

/// <summary>
/// A denormalised snapshot of a single line in a quotation. The material
/// name and brand are copied here (rather than referenced via FK) so that
/// the quotation PDF remains accurate even if the material catalog is
/// updated or a material is deleted later. This is the "Event Sourcing
/// lite" principle — capture facts as they were at a point in time.
/// </summary>
public class QuotationLineItem : BaseEntity
{
    public Guid QuotationId { get; set; }
    public Quotation Quotation { get; set; } = null!;

    public RoomType RoomType { get; set; }
    public string RoomName { get; set; } = string.Empty;
    public string ItemName { get; set; } = string.Empty;
    public WorkCategory Category { get; set; }

    public decimal Quantity { get; set; }
    public UnitOfMeasurement Unit { get; set; }
    public decimal UnitRate { get; set; }
    public decimal TotalAmount { get; set; }

    // Denormalised from Material — see class-level comment for rationale.
    public string? MaterialName { get; set; }
    public string? MaterialBrand { get; set; }
}
