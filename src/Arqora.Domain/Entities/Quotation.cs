using Arqora.Domain.Common;
using Arqora.Domain.Enums;

namespace Arqora.Domain.Entities;

/// <summary>
/// The generated quotation document. This entity captures a point-in-time
/// snapshot of pricing — even if material rates change later, the quotation
/// retains the prices that were valid when it was created.
///
/// Storing calculated totals (SubTotal, TaxAmount, GrandTotal) alongside
/// the line items is intentional: it avoids recalculating values every time
/// the quotation is displayed and preserves the exact figures the customer
/// saw when they accepted the quote.
/// </summary>
public class Quotation : BaseEntity
{
    /// <summary>
    /// Human-readable identifier (e.g. "ARQ-2026-0001"). Auto-generated
    /// by the application layer, not the database, so the domain stays
    /// database-agnostic.
    /// </summary>
    public string QuotationNumber { get; set; } = string.Empty;

    public QuotationTier Tier { get; set; }
    public QuotationStatus Status { get; set; } = QuotationStatus.Draft;

    public Guid ProjectId { get; set; }
    public Project Project { get; set; } = null!;

    // Nullable because a homeowner can self-generate a quotation without
    // a designer. When a designer creates the quote, this FK links to
    // their AppUser record.
    public string? DesignerId { get; set; }
    public AppUser? Designer { get; set; }

    public decimal SubTotal { get; set; }

    /// <summary>
    /// Defaults to 18 % — the standard GST rate in India for interior
    /// design services. Stored per-quotation so that rate changes don't
    /// retroactively alter historical quotes.
    /// </summary>
    public decimal TaxPercentage { get; set; } = 18;
    public decimal TaxAmount { get; set; }
    public decimal GrandTotal { get; set; }

    public decimal? DiscountPercentage { get; set; }
    public decimal? DiscountAmount { get; set; }

    public int EstimatedDaysToComplete { get; set; }
    public string? Notes { get; set; }
    public DateTime? ValidUntil { get; set; }

    public ICollection<QuotationLineItem> LineItems { get; set; } = new List<QuotationLineItem>();
}
