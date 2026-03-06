namespace Arqora.Application.DTOs;

/// <summary>
/// A flat DTO that carries everything the PDF renderer needs.
/// Using a dedicated DTO (instead of passing the domain entity directly)
/// decouples the PDF layout from the entity model — if entity shapes
/// change, only the mapping code needs updating, not the PDF template.
/// </summary>
public class QuotationPdfDto
{
    // Header
    public string QuotationNumber { get; set; } = string.Empty;
    public DateTime QuotationDate { get; set; }
    public DateTime? ValidUntil { get; set; }

    // Client details
    public string ClientName { get; set; } = string.Empty;
    public string? ClientEmail { get; set; }
    public string? ClientPhone { get; set; }
    public string? ProjectName { get; set; }
    public string? ProjectAddress { get; set; }
    public decimal TotalAreaSqFt { get; set; }

    // Designer details
    public string? DesignerName { get; set; }
    public string? DesignerCompany { get; set; }

    // Room-by-room line items
    public List<QuotationPdfRoomDto> Rooms { get; set; } = [];

    // Financials
    public decimal SubTotal { get; set; }
    public decimal? DiscountPercentage { get; set; }
    public decimal? DiscountAmount { get; set; }
    public decimal TaxPercentage { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal GrandTotal { get; set; }

    // Timeline
    public int EstimatedDaysToComplete { get; set; }
    public string? Notes { get; set; }

    // Category-wise summary for the pie chart section
    public List<CategorySummaryDto> CategorySummaries { get; set; } = [];
}

public class QuotationPdfRoomDto
{
    public string RoomName { get; set; } = string.Empty;
    public List<QuotationPdfLineItemDto> Items { get; set; } = [];
}

public class QuotationPdfLineItemDto
{
    public string ItemName { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public string Unit { get; set; } = string.Empty;
    public decimal UnitRate { get; set; }
    public decimal TotalAmount { get; set; }
    public string? MaterialName { get; set; }
    public string? MaterialBrand { get; set; }
}

public class CategorySummaryDto
{
    public string Category { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public decimal Percentage { get; set; }
}
