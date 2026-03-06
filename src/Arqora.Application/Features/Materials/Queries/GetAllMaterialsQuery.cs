using Arqora.Application.Common;
using Arqora.Application.DTOs;
using MediatR;

namespace Arqora.Application.Features.Materials.Queries;

/// <summary>
/// CQRS — QUERY
/// Queries return data without modifying state. The optional Category filter
/// lets the caller request a subset of materials without needing a separate
/// endpoint or query class for each filter combination.
/// </summary>
public class GetAllMaterialsQuery : IRequest<Result<List<MaterialDto>>>
{
    /// <summary>Optional filter — when set, only materials in this category are returned.</summary>
    public string? Category { get; set; }
}
