using Arqora.Application.Common;
using Arqora.Application.DTOs;
using FluentValidation;
using MediatR;

namespace Arqora.Application.Features.Rooms.Commands;

public class CreateProjectCommand : IRequest<Result<ProjectDto>>
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Address { get; set; }
    public decimal TotalAreaSqFt { get; set; }

    /// <summary>The ID of the authenticated user creating the project.</summary>
    public string UserId { get; set; } = string.Empty;
}

public class CreateProjectCommandValidator : AbstractValidator<CreateProjectCommand>
{
    public CreateProjectCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.TotalAreaSqFt).GreaterThan(0);
        RuleFor(x => x.UserId).NotEmpty();
    }
}
