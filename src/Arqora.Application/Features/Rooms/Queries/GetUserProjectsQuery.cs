using Arqora.Application.Common;
using Arqora.Application.DTOs;
using MediatR;

namespace Arqora.Application.Features.Rooms.Queries;

public class GetUserProjectsQuery : IRequest<Result<List<ProjectDto>>>
{
    public string UserId { get; set; } = string.Empty;
}
