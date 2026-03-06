using Arqora.Application.Common;
using MediatR;

namespace Arqora.Application.Features.Quotations.Queries;

/// <summary>
/// Returns the quotation as a PDF byte array. The handler delegates
/// to IQuotationPdfService — this query is essentially an orchestrator
/// that loads the data, maps it to QuotationPdfDto, and hands it to
/// the PDF renderer.
/// </summary>
public class GetQuotationPdfQuery : IRequest<Result<byte[]>>
{
    public Guid QuotationId { get; set; }
}
