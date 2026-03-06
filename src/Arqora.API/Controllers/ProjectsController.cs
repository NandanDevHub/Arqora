using System.Security.Claims;
using Arqora.Application.DTOs;
using Arqora.Application.Features.Rooms.Commands;
using Arqora.Application.Features.Rooms.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Arqora.API.Controllers;

[Authorize]
public class ProjectsController : BaseApiController
{
    private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    /// <summary>GET /api/projects — list the current user's projects.</summary>
    [HttpGet]
    public async Task<IActionResult> GetUserProjects()
    {
        var result = await Mediator.Send(new GetUserProjectsQuery { UserId = UserId });
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }

    /// <summary>GET /api/projects/{id} — full project detail with rooms and items.</summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetProjectDetail(Guid id)
    {
        var result = await Mediator.Send(new GetProjectDetailQuery { ProjectId = id });
        return result.IsSuccess ? Ok(result.Value) : NotFound(result.Error);
    }

    /// <summary>POST /api/projects — create a new project for the current user.</summary>
    [HttpPost]
    public async Task<IActionResult> Create(CreateProjectCommand command)
    {
        command.UserId = UserId;
        var result = await Mediator.Send(command);
        return result.IsSuccess
            ? CreatedAtAction(nameof(GetProjectDetail), new { id = result.Value!.Id }, result.Value)
            : BadRequest(result.Error);
    }

    /// <summary>POST /api/projects/{id}/rooms — add a room to a project.</summary>
    [HttpPost("{id:guid}/rooms")]
    public async Task<IActionResult> AddRoom(Guid id, AddRoomToProjectCommand command)
    {
        command.ProjectId = id;
        var result = await Mediator.Send(command);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }

    /// <summary>POST /api/projects/{projectId}/rooms/{roomId}/items — add an item to a room.</summary>
    [HttpPost("{projectId:guid}/rooms/{roomId:guid}/items")]
    public async Task<IActionResult> AddItem(Guid projectId, Guid roomId, AddItemToRoomCommand command)
    {
        command.ProjectRoomId = roomId;
        var result = await Mediator.Send(command);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }
}
