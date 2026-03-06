using Arqora.Application.DTOs;

namespace Arqora.Application.Interfaces;

/// <summary>
/// Abstracts PDF generation so that the Application layer can produce
/// quotation documents without coupling to a specific PDF library.
/// If we ever swap QuestPDF for iTextSharp or a cloud-based service,
/// only the Infrastructure implementation changes.
/// </summary>
public interface IQuotationPdfService
{
    byte[] GenerateQuotationPdf(QuotationPdfDto quotation);
}
