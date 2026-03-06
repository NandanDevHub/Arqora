using Arqora.Application.Common;
using Arqora.Application.DTOs;
using FluentValidation;
using MediatR;

namespace Arqora.Application.Features.Materials.Commands;

public class UpdateMaterialCommand : IRequest<Result<MaterialDto>>
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Brand { get; set; }
    public string Category { get; set; } = string.Empty;
    public string Unit { get; set; } = string.Empty;
    public decimal BasicRate { get; set; }
    public decimal StandardRate { get; set; }
    public decimal PremiumRate { get; set; }
    public string? ImageUrl { get; set; }
}

public class UpdateMaterialCommandValidator : AbstractValidator<UpdateMaterialCommand>
{
    public UpdateMaterialCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Category).NotEmpty();
        RuleFor(x => x.Unit).NotEmpty();
        RuleFor(x => x.BasicRate).GreaterThan(0);
        RuleFor(x => x.StandardRate).GreaterThan(0);
        RuleFor(x => x.PremiumRate).GreaterThan(0);
    }
}
