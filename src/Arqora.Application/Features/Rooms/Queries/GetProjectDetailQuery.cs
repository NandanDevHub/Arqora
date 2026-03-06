using Arqora.Application.Common;
using Arqora.Application.DTOs;
using MediatR;

namespace Arqora.Application.Features.Rooms.Queries;

public class GetProjectDetailQuery : IRequest<Result<ProjectDetailDto>>
{
    public Guid ProjectId { get; set; }
}
