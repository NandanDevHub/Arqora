using Arqora.Application.DTOs;
using Arqora.Application.Interfaces;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Arqora.Infrastructure.Services;

/// <summary>
/// Generates a professional quotation PDF using QuestPDF.
///
/// QUESTPDF COMMUNITY LICENSE
/// ──────────────────────────
/// QuestPDF is free under the Community License for companies and individuals
/// with annual gross revenue of less than $1M USD. For revenue above that
/// threshold you must purchase a Professional or Enterprise license.
/// See: https://www.questpdf.com/license/
/// We set LicenseType.Community explicitly to acknowledge this.
/// </summary>
public class QuotationPdfService : IQuotationPdfService
{
    public byte[] GenerateQuotationPdf(QuotationPdfDto dto)
    {
        QuestPDF.Settings.License = LicenseType.Community;

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(30);
                page.DefaultTextStyle(x => x.FontSize(10));

                page.Header().Element(c => ComposeHeader(c, dto));
                page.Content().Element(c => ComposeContent(c, dto));
                page.Footer().Element(c => ComposeFooter(c));
            });
        });

        return document.GeneratePdf();
    }

    // ─── Header ───────────────────────────────────────────────
    private static void ComposeHeader(IContainer container, QuotationPdfDto dto)
    {
        container.Column(column =>
        {
            // Brand bar
            column.Item().Row(row =>
            {
                row.RelativeItem().Text("ARQORA")
                    .FontSize(24).Bold().FontColor(Colors.Blue.Darken3);

                row.RelativeItem().AlignRight().Column(right =>
                {
                    right.Item().Text($"Quotation #{dto.QuotationNumber}")
                        .FontSize(14).Bold();
                    right.Item().Text($"Date: {dto.QuotationDate:dd MMM yyyy}");
                    if (dto.ValidUntil.HasValue)
                        right.Item().Text($"Valid Until: {dto.ValidUntil:dd MMM yyyy}");
                });
            });

            column.Item().PaddingVertical(5).LineHorizontal(1).LineColor(Colors.Grey.Medium);

            // Client & designer details
            column.Item().Row(row =>
            {
                row.RelativeItem().Column(left =>
                {
                    left.Item().Text("Client Details").Bold().FontSize(11);
                    left.Item().Text(dto.ClientName);
                    if (!string.IsNullOrEmpty(dto.ClientEmail))
                        left.Item().Text(dto.ClientEmail);
                    if (!string.IsNullOrEmpty(dto.ClientPhone))
                        left.Item().Text($"Phone: {dto.ClientPhone}");
                });

                row.RelativeItem().Column(right =>
                {
                    right.Item().Text("Project Details").Bold().FontSize(11);
                    if (!string.IsNullOrEmpty(dto.ProjectName))
                        right.Item().Text(dto.ProjectName);
                    if (!string.IsNullOrEmpty(dto.ProjectAddress))
                        right.Item().Text(dto.ProjectAddress);
                    right.Item().Text($"Total Area: {dto.TotalAreaSqFt:N0} sqft");
                });
            });

            if (!string.IsNullOrEmpty(dto.DesignerName))
            {
                column.Item().PaddingTop(5).Row(row =>
                {
                    row.RelativeItem().Column(left =>
                    {
                        left.Item().Text("Designer").Bold().FontSize(11);
                        left.Item().Text(dto.DesignerName);
                        if (!string.IsNullOrEmpty(dto.DesignerCompany))
                            left.Item().Text(dto.DesignerCompany);
                    });
                });
            }

            column.Item().PaddingVertical(5).LineHorizontal(1).LineColor(Colors.Grey.Lighten1);
        });
    }

    // ─── Content ──────────────────────────────────────────────
    private static void ComposeContent(IContainer container, QuotationPdfDto dto)
    {
        container.PaddingVertical(10).Column(column =>
        {
            // Room-by-room breakdown tables
            foreach (var room in dto.Rooms)
            {
                column.Item().PaddingBottom(10).Column(roomCol =>
                {
                    roomCol.Item().Text(room.RoomName).Bold().FontSize(12)
                        .FontColor(Colors.Blue.Darken2);

                    roomCol.Item().Table(table =>
                    {
                        table.ColumnsDefinition(cols =>
                        {
                            cols.RelativeColumn(3);  // Item
                            cols.RelativeColumn(1.5f); // Category
                            cols.RelativeColumn(1);  // Qty
                            cols.RelativeColumn(1);  // Unit
                            cols.RelativeColumn(1.2f); // Rate
                            cols.RelativeColumn(1.3f); // Amount
                        });

                        // Table header
                        table.Header(header =>
                        {
                            header.Cell().Background(Colors.Blue.Darken3).Padding(4)
                                .Text("Item").FontColor(Colors.White).FontSize(9).Bold();
                            header.Cell().Background(Colors.Blue.Darken3).Padding(4)
                                .Text("Category").FontColor(Colors.White).FontSize(9).Bold();
                            header.Cell().Background(Colors.Blue.Darken3).Padding(4)
                                .Text("Qty").FontColor(Colors.White).FontSize(9).Bold();
                            header.Cell().Background(Colors.Blue.Darken3).Padding(4)
                                .Text("Unit").FontColor(Colors.White).FontSize(9).Bold();
                            header.Cell().Background(Colors.Blue.Darken3).Padding(4)
                                .Text("Rate (₹)").FontColor(Colors.White).FontSize(9).Bold();
                            header.Cell().Background(Colors.Blue.Darken3).Padding(4)
                                .Text("Amount (₹)").FontColor(Colors.White).FontSize(9).Bold();
                        });

                        // Table rows
                        foreach (var item in room.Items)
                        {
                            var bgColor = room.Items.ToList().IndexOf(item) % 2 == 0
                                ? Colors.White
                                : Colors.Grey.Lighten4;

                            table.Cell().Background(bgColor).Padding(3).Text(item.ItemName).FontSize(9);
                            table.Cell().Background(bgColor).Padding(3).Text(item.Category).FontSize(9);
                            table.Cell().Background(bgColor).Padding(3).Text($"{item.Quantity:N1}").FontSize(9);
                            table.Cell().Background(bgColor).Padding(3).Text(item.Unit).FontSize(9);
                            table.Cell().Background(bgColor).Padding(3).AlignRight()
                                .Text($"{item.UnitRate:N2}").FontSize(9);
                            table.Cell().Background(bgColor).Padding(3).AlignRight()
                                .Text($"{item.TotalAmount:N2}").FontSize(9);
                        }
                    });
                });
            }

            // ── Category-wise Summary ─────────────────────────
            column.Item().PaddingVertical(10).Column(summaryCol =>
            {
                summaryCol.Item().Text("Category-wise Cost Breakdown")
                    .Bold().FontSize(12).FontColor(Colors.Blue.Darken2);

                summaryCol.Item().PaddingTop(5).Table(table =>
                {
                    table.ColumnsDefinition(cols =>
                    {
                        cols.RelativeColumn(3);   // Category
                        cols.RelativeColumn(1.5f); // Amount
                        cols.RelativeColumn(1);   // Percentage
                    });

                    table.Header(header =>
                    {
                        header.Cell().Background(Colors.Grey.Darken1).Padding(4)
                            .Text("Category").FontColor(Colors.White).FontSize(9).Bold();
                        header.Cell().Background(Colors.Grey.Darken1).Padding(4)
                            .Text("Amount (₹)").FontColor(Colors.White).FontSize(9).Bold();
                        header.Cell().Background(Colors.Grey.Darken1).Padding(4)
                            .Text("% of Total").FontColor(Colors.White).FontSize(9).Bold();
                    });

                    foreach (var cat in dto.CategorySummaries)
                    {
                        // Text-based percentage bar since QuestPDF is a layout engine,
                        // not a charting library. A visual bar can be added with
                        // Container().Width(cat.Percentage * 2) if desired.
                        table.Cell().Padding(3).Text(cat.Category).FontSize(9);
                        table.Cell().Padding(3).AlignRight().Text($"{cat.Amount:N2}").FontSize(9);
                        table.Cell().Padding(3).AlignRight().Text($"{cat.Percentage:N1}%").FontSize(9);
                    }
                });
            });

            // ── Timeline ──────────────────────────────────────
            column.Item().PaddingVertical(5).Column(timeCol =>
            {
                timeCol.Item().Text("Project Timeline")
                    .Bold().FontSize(12).FontColor(Colors.Blue.Darken2);
                timeCol.Item().Text($"Estimated completion: {dto.EstimatedDaysToComplete} working days");
                if (!string.IsNullOrEmpty(dto.Notes))
                    timeCol.Item().PaddingTop(3).Text($"Notes: {dto.Notes}").Italic();
            });

            // ── Grand Total ───────────────────────────────────
            column.Item().PaddingVertical(10).Background(Colors.Grey.Lighten3)
                .Padding(10).Column(totalCol =>
            {
                totalCol.Item().Row(row =>
                {
                    row.RelativeItem().Text("Sub Total").Bold();
                    row.RelativeItem().AlignRight().Text($"₹ {dto.SubTotal:N2}").Bold();
                });

                if (dto.DiscountPercentage.HasValue && dto.DiscountAmount.HasValue)
                {
                    totalCol.Item().Row(row =>
                    {
                        row.RelativeItem().Text($"Discount ({dto.DiscountPercentage:N1}%)");
                        row.RelativeItem().AlignRight()
                            .Text($"- ₹ {dto.DiscountAmount:N2}").FontColor(Colors.Green.Darken2);
                    });
                }

                totalCol.Item().Row(row =>
                {
                    row.RelativeItem().Text($"GST ({dto.TaxPercentage:N1}%)");
                    row.RelativeItem().AlignRight().Text($"₹ {dto.TaxAmount:N2}");
                });

                totalCol.Item().PaddingTop(5).LineHorizontal(1);

                totalCol.Item().PaddingTop(5).Row(row =>
                {
                    row.RelativeItem().Text("Grand Total").Bold().FontSize(14);
                    row.RelativeItem().AlignRight()
                        .Text($"₹ {dto.GrandTotal:N2}").Bold().FontSize(14)
                        .FontColor(Colors.Blue.Darken3);
                });
            });
        });
    }

    // ─── Footer ───────────────────────────────────────────────
    private static void ComposeFooter(IContainer container)
    {
        container.Column(column =>
        {
            column.Item().PaddingVertical(5).LineHorizontal(1).LineColor(Colors.Grey.Lighten1);

            column.Item().Text("Terms & Conditions").Bold().FontSize(9);
            column.Item().Text(text =>
            {
                text.Span("1. This quotation is valid for 30 days from the date of issue. ").FontSize(8);
                text.Span("2. 50% advance payment is required to commence work. ").FontSize(8);
                text.Span("3. Rates are subject to change based on material availability. ").FontSize(8);
                text.Span("4. Any additional work not covered in this quotation will be charged separately. ").FontSize(8);
                text.Span("5. GST is applicable as per government norms.").FontSize(8);
            });

            column.Item().PaddingTop(10).AlignCenter()
                .Text("Generated by Arqora — Interior Design Quotation Platform")
                .FontSize(8).FontColor(Colors.Grey.Medium);

            column.Item().AlignCenter().Text(text =>
            {
                text.Span("Page ").FontSize(8);
                text.CurrentPageNumber().FontSize(8);
                text.Span(" of ").FontSize(8);
                text.TotalPages().FontSize(8);
            });
        });
    }
}
