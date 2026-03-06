using Arqora.Application.Common;
using Arqora.Application.DTOs;
using MediatR;

namespace Arqora.Application.Features.Materials.Queries;

public class GetMaterialByIdQuery : IRequest<Result<MaterialDto>>
{
    public Guid Id { get; set; }
}
