using Arqora.Application.Common;
using Arqora.Application.DTOs;
using Arqora.Application.Features.Materials.Commands;
using Arqora.Application.Features.Materials.Queries;
using Arqora.Application.Interfaces;
using Arqora.Domain.Entities;
using Arqora.Domain.Enums;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Arqora.Application.Features.Materials.Handlers;

// ─── CREATE ──────────────────────────────────────────────────────────
public class CreateMaterialCommandHandler : IRequestHandler<CreateMaterialCommand, Result<MaterialDto>>
{
    private readonly IArqoraDbContext _context;
    private readonly IMapper _mapper;

    public CreateMaterialCommandHandler(IArqoraDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Result<MaterialDto>> Handle(CreateMaterialCommand request, CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<WorkCategory>(request.Category, ignoreCase: true, out var category))
            return Result<MaterialDto>.Failure($"Invalid category: {request.Category}");

        if (!Enum.TryParse<UnitOfMeasurement>(request.Unit, ignoreCase: true, out var unit))
            return Result<MaterialDto>.Failure($"Invalid unit: {request.Unit}");

        var material = new Material
        {
            Name = request.Name,
            Description = request.Description,
            Brand = request.Brand,
            Category = category,
            Unit = unit,
            BasicRate = request.BasicRate,
            StandardRate = request.StandardRate,
            PremiumRate = request.PremiumRate,
            ImageUrl = request.ImageUrl
        };

        _context.Materials.Add(material);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<MaterialDto>.Success(_mapper.Map<MaterialDto>(material));
    }
}

// ─── UPDATE ──────────────────────────────────────────────────────────
public class UpdateMaterialCommandHandler : IRequestHandler<UpdateMaterialCommand, Result<MaterialDto>>
{
    private readonly IArqoraDbContext _context;
    private readonly IMapper _mapper;

    public UpdateMaterialCommandHandler(IArqoraDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Result<MaterialDto>> Handle(UpdateMaterialCommand request, CancellationToken cancellationToken)
    {
        var material = await _context.Materials.FindAsync([request.Id], cancellationToken);
        if (material == null)
            return Result<MaterialDto>.Failure("Material not found.");

        if (!Enum.TryParse<WorkCategory>(request.Category, ignoreCase: true, out var category))
            return Result<MaterialDto>.Failure($"Invalid category: {request.Category}");

        if (!Enum.TryParse<UnitOfMeasurement>(request.Unit, ignoreCase: true, out var unit))
            return Result<MaterialDto>.Failure($"Invalid unit: {request.Unit}");

        material.Name = request.Name;
        material.Description = request.Description;
        material.Brand = request.Brand;
        material.Category = category;
        material.Unit = unit;
        material.BasicRate = request.BasicRate;
        material.StandardRate = request.StandardRate;
        material.PremiumRate = request.PremiumRate;
        material.ImageUrl = request.ImageUrl;

        await _context.SaveChangesAsync(cancellationToken);

        return Result<MaterialDto>.Success(_mapper.Map<MaterialDto>(material));
    }
}

// ─── DELETE (soft) ───────────────────────────────────────────────────
public class DeleteMaterialCommandHandler : IRequestHandler<DeleteMaterialCommand, Result<bool>>
{
    private readonly IArqoraDbContext _context;

    public DeleteMaterialCommandHandler(IArqoraDbContext context)
    {
        _context = context;
    }

    public async Task<Result<bool>> Handle(DeleteMaterialCommand request, CancellationToken cancellationToken)
    {
        var material = await _context.Materials.FindAsync([request.Id], cancellationToken);
        if (material == null)
            return Result<bool>.Failure("Material not found.");

        // Soft delete — set IsActive to false instead of removing the row.
        material.IsActive = false;
        await _context.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(true);
    }
}

// ─── GET ALL ─────────────────────────────────────────────────────────
public class GetAllMaterialsQueryHandler : IRequestHandler<GetAllMaterialsQuery, Result<List<MaterialDto>>>
{
    private readonly IArqoraDbContext _context;
    private readonly IMapper _mapper;

    public GetAllMaterialsQueryHandler(IArqoraDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Result<List<MaterialDto>>> Handle(GetAllMaterialsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Materials
            .Where(m => m.IsActive)
            .AsNoTracking();

        // Apply optional category filter.
        if (!string.IsNullOrWhiteSpace(request.Category) &&
            Enum.TryParse<WorkCategory>(request.Category, ignoreCase: true, out var category))
        {
            query = query.Where(m => m.Category == category);
        }

        var materials = await query
            .OrderBy(m => m.Name)
            .ProjectTo<MaterialDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

        return Result<List<MaterialDto>>.Success(materials);
    }
}

// ─── GET BY ID ───────────────────────────────────────────────────────
public class GetMaterialByIdQueryHandler : IRequestHandler<GetMaterialByIdQuery, Result<MaterialDto>>
{
    private readonly IArqoraDbContext _context;
    private readonly IMapper _mapper;

    public GetMaterialByIdQueryHandler(IArqoraDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Result<MaterialDto>> Handle(GetMaterialByIdQuery request, CancellationToken cancellationToken)
    {
        var material = await _context.Materials
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == request.Id, cancellationToken);

        if (material == null)
            return Result<MaterialDto>.Failure("Material not found.");

        return Result<MaterialDto>.Success(_mapper.Map<MaterialDto>(material));
    }
}
