using Arqora.Application.Common;
using Arqora.Application.DTOs;
using FluentValidation;
using MediatR;

namespace Arqora.Application.Features.Quotations.Commands;

public class UpdateQuotationStatusCommand : IRequest<Result<QuotationDto>>
{
    public Guid Id { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class UpdateQuotationStatusCommandValidator : AbstractValidator<UpdateQuotationStatusCommand>
{
    public UpdateQuotationStatusCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Status).NotEmpty()
            .Must(s => s is "Draft" or "Sent" or "Accepted" or "Rejected" or "Expired")
            .WithMessage("Status must be Draft, Sent, Accepted, Rejected, or Expired.");
    }
}
