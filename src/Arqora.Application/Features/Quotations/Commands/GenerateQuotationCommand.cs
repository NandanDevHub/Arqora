using Arqora.Application.Common;
using Arqora.Application.DTOs;
using FluentValidation;
using MediatR;

namespace Arqora.Application.Features.Quotations.Commands;

/// <summary>
/// CORE ENGINE COMMAND — GENERATE QUOTATION
/// ─────────────────────────────────────────
/// This is the heart of Arqora. It takes a project and a pricing tier,
/// walks through every room and every item in that project, looks up
/// the appropriate rate (custom override → material catalog), calculates
/// line-item totals, applies tax and discounts, estimates a timeline,
/// and produces a complete Quotation entity.
///
/// The command carries the input; the handler (GenerateQuotationCommandHandler)
/// contains the calculation engine. Separating them keeps the command a
/// simple, serialisable data bag — important if you ever need to queue
/// quotation generation as a background job.
/// </summary>
public class GenerateQuotationCommand : IRequest<Result<QuotationDetailDto>>
{
    public Guid ProjectId { get; set; }
    public string Tier { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public decimal? DiscountPercentage { get; set; }

    /// <summary>
    /// Optional — the designer generating the quotation. Null when a
    /// homeowner generates their own quote.
    /// </summary>
    public string? DesignerId { get; set; }
}

public class GenerateQuotationCommandValidator : AbstractValidator<GenerateQuotationCommand>
{
    public GenerateQuotationCommandValidator()
    {
        RuleFor(x => x.ProjectId).NotEmpty();
        RuleFor(x => x.Tier).NotEmpty()
            .Must(t => t is "Basic" or "Standard" or "Premium")
            .WithMessage("Tier must be Basic, Standard, or Premium.");
        RuleFor(x => x.DiscountPercentage)
            .InclusiveBetween(0, 100)
            .When(x => x.DiscountPercentage.HasValue);
    }
}
