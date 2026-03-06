using Arqora.Application.Common;
using Arqora.Application.DTOs;
using MediatR;

namespace Arqora.Application.Features.Quotations.Queries;

public class GetQuotationSummaryQuery : IRequest<Result<QuotationSummaryDto>>
{
    public Guid QuotationId { get; set; }
}
