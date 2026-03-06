using Arqora.Application.Common;
using MediatR;

namespace Arqora.Application.Features.Materials.Commands;

/// <summary>
/// Soft-deletes a material by setting IsActive = false instead of physically
/// removing the row. This preserves referential integrity — quotation line
/// items that referenced this material still resolve correctly.
/// </summary>
public class DeleteMaterialCommand : IRequest<Result<bool>>
{
    public Guid Id { get; set; }
}
