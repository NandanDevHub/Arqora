namespace Arqora.Application.DTOs;

/// <summary>
/// DTOs for quotations. The separation between QuotationDto (list view)
/// and QuotationDetailDto (full view with line items) follows the CQRS
/// read-model principle: return only the data each screen actually needs.
/// </summary>

public class QuotationDto
{
    public Guid Id { get; set; }
    public string QuotationNumber { get; set; } = string.Empty;
    public string Tier { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string ProjectName { get; set; } = string.Empty;
    public decimal SubTotal { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal GrandTotal { get; set; }
    public decimal? DiscountPercentage { get; set; }
    public int EstimatedDaysToComplete { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ValidUntil { get; set; }
}

public class QuotationDetailDto
{
    public Guid Id { get; set; }
    public string QuotationNumber { get; set; } = string.Empty;
    public string Tier { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public Guid ProjectId { get; set; }
    public string ProjectName { get; set; } = string.Empty;
    public string? DesignerName { get; set; }

    public decimal SubTotal { get; set; }
    public decimal TaxPercentage { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal GrandTotal { get; set; }
    public decimal? DiscountPercentage { get; set; }
    public decimal? DiscountAmount { get; set; }

    public int EstimatedDaysToComplete { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ValidUntil { get; set; }

    public List<QuotationLineItemDto> LineItems { get; set; } = [];
}

public class GenerateQuotationDto
{
    public Guid ProjectId { get; set; }
    public string Tier { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public decimal? DiscountPercentage { get; set; }
}

public class QuotationLineItemDto
{
    public Guid Id { get; set; }
    public string RoomType { get; set; } = string.Empty;
    public string RoomName { get; set; } = string.Empty;
    public string ItemName { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public string Unit { get; set; } = string.Empty;
    public decimal UnitRate { get; set; }
    public decimal TotalAmount { get; set; }
    public string? MaterialName { get; set; }
    public string? MaterialBrand { get; set; }
}

public class QuotationSummaryDto
{
    public Dictionary<string, decimal> CategoryBreakdown { get; set; } = new();
    public Dictionary<string, decimal> RoomBreakdown { get; set; } = new();
    public decimal SubTotal { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal GrandTotal { get; set; }
    public int EstimatedDays { get; set; }
}

public class DashboardStatsDto
{
    public int TotalProjects { get; set; }
    public int TotalQuotations { get; set; }
    public decimal TotalRevenue { get; set; }
    public List<QuotationDto> RecentQuotations { get; set; } = [];
}
