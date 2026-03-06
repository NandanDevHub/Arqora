using Arqora.Application.Common;
using Arqora.Application.DTOs;
using MediatR;

namespace Arqora.Application.Features.Quotations.Queries;

public class GetQuotationByIdQuery : IRequest<Result<QuotationDetailDto>>
{
    public Guid Id { get; set; }
}
