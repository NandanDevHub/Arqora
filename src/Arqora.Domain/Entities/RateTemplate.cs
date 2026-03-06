using Arqora.Domain.Common;

namespace Arqora.Domain.Entities;

/// <summary>
/// Admin-managed rate templates allow different pricing sets for different
/// cities or market segments (e.g. "Mumbai Rates 2026", "Bangalore Standard").
///
/// The Template pattern here decouples rate definitions from individual
/// materials — an admin can create a template and bulk-apply rates, while
/// designers can override rates at the RoomItem level. This two-tier
/// approach (template → per-item override) mirrors how real interior firms
/// manage their pricing.
/// </summary>
public class RateTemplate : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    /// <summary>
    /// Only one template should be marked as default at a time. The
    /// application layer should enforce this uniqueness constraint rather
    /// than the database, so that the rule can be changed without a
    /// migration.
    /// </summary>
    public bool IsDefault { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<RateTemplateItem> Items { get; set; } = new List<RateTemplateItem>();
}
