using Arqora.Application.Common;
using Arqora.Application.DTOs;
using FluentValidation;
using MediatR;

namespace Arqora.Application.Features.Rooms.Commands;

public class AddRoomToProjectCommand : IRequest<Result<ProjectRoomDto>>
{
    public Guid ProjectId { get; set; }
    public string RoomType { get; set; } = string.Empty;
    public string? CustomName { get; set; }
    public decimal AreaSqFt { get; set; }
    public decimal? CeilingHeightFt { get; set; }
}

public class AddRoomToProjectCommandValidator : AbstractValidator<AddRoomToProjectCommand>
{
    public AddRoomToProjectCommandValidator()
    {
        RuleFor(x => x.ProjectId).NotEmpty();
        RuleFor(x => x.RoomType).NotEmpty();
        RuleFor(x => x.AreaSqFt).GreaterThan(0);
        RuleFor(x => x.CeilingHeightFt).GreaterThan(0).When(x => x.CeilingHeightFt.HasValue);
    }
}
