using Arqora.Application.Features.Materials.Commands;
using Arqora.Application.Features.Materials.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Arqora.API.Controllers;

public class MaterialsController : BaseApiController
{
    /// <summary>GET /api/materials?category=Furniture — list materials, optionally filtered.</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? category)
    {
        var result = await Mediator.Send(new GetAllMaterialsQuery { Category = category });
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }

    /// <summary>GET /api/materials/{id} — single material by ID.</summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await Mediator.Send(new GetMaterialByIdQuery { Id = id });
        return result.IsSuccess ? Ok(result.Value) : NotFound(result.Error);
    }

    /// <summary>POST /api/materials — create a new material (Admin only).</summary>
    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> Create(CreateMaterialCommand command)
    {
        var result = await Mediator.Send(command);
        return result.IsSuccess
            ? CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, result.Value)
            : BadRequest(result.Error);
    }

    /// <summary>PUT /api/materials/{id} — update an existing material (Admin only).</summary>
    [Authorize(Roles = "Admin")]
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, UpdateMaterialCommand command)
    {
        command.Id = id;
        var result = await Mediator.Send(command);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }

    /// <summary>DELETE /api/materials/{id} — soft-delete a material (Admin only).</summary>
    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await Mediator.Send(new DeleteMaterialCommand { Id = id });
        return result.IsSuccess ? NoContent() : BadRequest(result.Error);
    }
}
