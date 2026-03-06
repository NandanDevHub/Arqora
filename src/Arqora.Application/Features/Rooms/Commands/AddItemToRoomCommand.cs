using Arqora.Application.Common;
using Arqora.Application.DTOs;
using FluentValidation;
using MediatR;

namespace Arqora.Application.Features.Rooms.Commands;

public class AddItemToRoomCommand : IRequest<Result<RoomItemDto>>
{
    public Guid ProjectRoomId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Category { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public string Unit { get; set; } = string.Empty;
    public Guid? MaterialId { get; set; }
    public decimal? CustomBasicRate { get; set; }
    public decimal? CustomStandardRate { get; set; }
    public decimal? CustomPremiumRate { get; set; }
}

public class AddItemToRoomCommandValidator : AbstractValidator<AddItemToRoomCommand>
{
    public AddItemToRoomCommandValidator()
    {
        RuleFor(x => x.ProjectRoomId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Category).NotEmpty();
        RuleFor(x => x.Quantity).GreaterThan(0);
        RuleFor(x => x.Unit).NotEmpty();
    }
}
