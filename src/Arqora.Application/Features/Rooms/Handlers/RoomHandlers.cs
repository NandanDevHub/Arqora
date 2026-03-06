using Arqora.Application.Common;
using Arqora.Application.DTOs;
using Arqora.Application.Features.Rooms.Commands;
using Arqora.Application.Features.Rooms.Queries;
using Arqora.Application.Interfaces;
using Arqora.Domain.Entities;
using Arqora.Domain.Enums;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Arqora.Application.Features.Rooms.Handlers;

// ─── CREATE PROJECT ──────────────────────────────────────────────────
public class CreateProjectCommandHandler : IRequestHandler<CreateProjectCommand, Result<ProjectDto>>
{
    private readonly IArqoraDbContext _context;
    private readonly IMapper _mapper;

    public CreateProjectCommandHandler(IArqoraDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Result<ProjectDto>> Handle(CreateProjectCommand request, CancellationToken cancellationToken)
    {
        var project = new Project
        {
            Name = request.Name,
            Description = request.Description,
            Address = request.Address,
            TotalAreaSqFt = request.TotalAreaSqFt,
            AppUserId = request.UserId
        };

        _context.Projects.Add(project);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<ProjectDto>.Success(_mapper.Map<ProjectDto>(project));
    }
}

// ─── ADD ROOM TO PROJECT ─────────────────────────────────────────────
public class AddRoomToProjectCommandHandler : IRequestHandler<AddRoomToProjectCommand, Result<ProjectRoomDto>>
{
    private readonly IArqoraDbContext _context;
    private readonly IMapper _mapper;

    public AddRoomToProjectCommandHandler(IArqoraDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Result<ProjectRoomDto>> Handle(AddRoomToProjectCommand request, CancellationToken cancellationToken)
    {
        var projectExists = await _context.Projects.AnyAsync(p => p.Id == request.ProjectId, cancellationToken);
        if (!projectExists)
            return Result<ProjectRoomDto>.Failure("Project not found.");

        if (!Enum.TryParse<RoomType>(request.RoomType, ignoreCase: true, out var roomType))
            return Result<ProjectRoomDto>.Failure($"Invalid room type: {request.RoomType}");

        var room = new ProjectRoom
        {
            ProjectId = request.ProjectId,
            RoomType = roomType,
            CustomName = request.CustomName,
            AreaSqFt = request.AreaSqFt,
            CeilingHeightFt = request.CeilingHeightFt ?? 10 // Default 10 ft ceiling height.
        };

        _context.ProjectRooms.Add(room);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<ProjectRoomDto>.Success(_mapper.Map<ProjectRoomDto>(room));
    }
}

// ─── ADD ITEM TO ROOM ────────────────────────────────────────────────
public class AddItemToRoomCommandHandler : IRequestHandler<AddItemToRoomCommand, Result<RoomItemDto>>
{
    private readonly IArqoraDbContext _context;
    private readonly IMapper _mapper;

    public AddItemToRoomCommandHandler(IArqoraDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Result<RoomItemDto>> Handle(AddItemToRoomCommand request, CancellationToken cancellationToken)
    {
        var roomExists = await _context.ProjectRooms.AnyAsync(r => r.Id == request.ProjectRoomId, cancellationToken);
        if (!roomExists)
            return Result<RoomItemDto>.Failure("Room not found.");

        if (!Enum.TryParse<WorkCategory>(request.Category, ignoreCase: true, out var category))
            return Result<RoomItemDto>.Failure($"Invalid category: {request.Category}");

        if (!Enum.TryParse<UnitOfMeasurement>(request.Unit, ignoreCase: true, out var unit))
            return Result<RoomItemDto>.Failure($"Invalid unit: {request.Unit}");

        // Validate material exists if provided.
        if (request.MaterialId.HasValue)
        {
            var materialExists = await _context.Materials.AnyAsync(
                m => m.Id == request.MaterialId.Value && m.IsActive, cancellationToken);
            if (!materialExists)
                return Result<RoomItemDto>.Failure("Material not found or inactive.");
        }

        var item = new RoomItem
        {
            ProjectRoomId = request.ProjectRoomId,
            Name = request.Name,
            Description = request.Description,
            Category = category,
            Quantity = request.Quantity,
            Unit = unit,
            MaterialId = request.MaterialId,
            CustomBasicRate = request.CustomBasicRate,
            CustomStandardRate = request.CustomStandardRate,
            CustomPremiumRate = request.CustomPremiumRate
        };

        _context.RoomItems.Add(item);
        await _context.SaveChangesAsync(cancellationToken);

        // Re-load with material for complete DTO mapping.
        var saved = await _context.RoomItems
            .AsNoTracking()
            .Include(ri => ri.Material)
            .FirstAsync(ri => ri.Id == item.Id, cancellationToken);

        return Result<RoomItemDto>.Success(_mapper.Map<RoomItemDto>(saved));
    }
}

// ─── GET PROJECT DETAIL ──────────────────────────────────────────────
public class GetProjectDetailQueryHandler : IRequestHandler<GetProjectDetailQuery, Result<ProjectDetailDto>>
{
    private readonly IArqoraDbContext _context;
    private readonly IMapper _mapper;

    public GetProjectDetailQueryHandler(IArqoraDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Result<ProjectDetailDto>> Handle(GetProjectDetailQuery request, CancellationToken cancellationToken)
    {
        var project = await _context.Projects
            .AsNoTracking()
            .Include(p => p.Owner)
            .Include(p => p.Rooms)
                .ThenInclude(r => r.Items)
                    .ThenInclude(i => i.Material)
            .FirstOrDefaultAsync(p => p.Id == request.ProjectId, cancellationToken);

        if (project == null)
            return Result<ProjectDetailDto>.Failure("Project not found.");

        return Result<ProjectDetailDto>.Success(_mapper.Map<ProjectDetailDto>(project));
    }
}

// ─── GET USER PROJECTS ───────────────────────────────────────────────
public class GetUserProjectsQueryHandler : IRequestHandler<GetUserProjectsQuery, Result<List<ProjectDto>>>
{
    private readonly IArqoraDbContext _context;
    private readonly IMapper _mapper;

    public GetUserProjectsQueryHandler(IArqoraDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Result<List<ProjectDto>>> Handle(GetUserProjectsQuery request, CancellationToken cancellationToken)
    {
        var projects = await _context.Projects
            .AsNoTracking()
            .Include(p => p.Owner)
            .Include(p => p.Rooms)
            .Include(p => p.Quotations)
            .Where(p => p.AppUserId == request.UserId)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(cancellationToken);

        return Result<List<ProjectDto>>.Success(_mapper.Map<List<ProjectDto>>(projects));
    }
}
