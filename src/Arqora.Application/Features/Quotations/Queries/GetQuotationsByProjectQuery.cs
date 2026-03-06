using Arqora.Application.Common;
using Arqora.Application.DTOs;
using MediatR;

namespace Arqora.Application.Features.Quotations.Queries;

public class GetQuotationsByProjectQuery : IRequest<Result<List<QuotationDto>>>
{
    public Guid ProjectId { get; set; }
}
