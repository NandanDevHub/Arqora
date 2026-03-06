using Arqora.Application.Common;
using Arqora.Application.DTOs;
using Arqora.Application.Features.Quotations.Commands;
using Arqora.Application.Features.Quotations.Queries;
using Arqora.Application.Interfaces;
using Arqora.Domain.Entities;
using Arqora.Domain.Enums;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Arqora.Application.Features.Quotations.Handlers;

// ═══════════════════════════════════════════════════════════════════════
// GENERATE QUOTATION — THE CORE PRICING ENGINE
// ═══════════════════════════════════════════════════════════════════════
//
// This handler is the beating heart of Arqora. It transforms a project's
// room-item configuration into a fully priced quotation. Here's the
// algorithm at a high level:
//
//   1. LOAD the project with all rooms → items → materials (eager loading).
//   2. DETERMINE the pricing tier (Basic / Standard / Premium).
//   3. FOR EACH room item:
//      a. Pick the unit rate: prefer custom override → fall back to material catalog.
//      b. Calculate line total = quantity × unit rate.
//      c. Create a denormalised QuotationLineItem (snapshot of current prices).
//   4. SUM all line totals → SubTotal.
//   5. APPLY discount (if any): DiscountAmount = SubTotal × DiscountPercentage / 100.
//   6. CALCULATE tax: TaxAmount = (SubTotal - DiscountAmount) × 18 / 100 (GST).
//   7. GRAND TOTAL = SubTotal - DiscountAmount + TaxAmount.
//   8. ESTIMATE timeline based on tier and number of rooms.
//   9. GENERATE a human-readable quotation number via IQuotationNumberGenerator.
//  10. PERSIST the Quotation + LineItems in a single SaveChangesAsync call.
//
// WHY DENORMALISE LINE ITEMS?
// Material prices change over time. By copying the rate into each line item
// at generation time, we create a point-in-time snapshot. The quotation PDF
// will always show the prices the customer saw — even if the catalog is
// updated later.
// ═══════════════════════════════════════════════════════════════════════

public class GenerateQuotationCommandHandler : IRequestHandler<GenerateQuotationCommand, Result<QuotationDetailDto>>
{
    private readonly IArqoraDbContext _context;
    private readonly IQuotationNumberGenerator _numberGenerator;
    private readonly IMapper _mapper;

    public GenerateQuotationCommandHandler(
        IArqoraDbContext context,
        IQuotationNumberGenerator numberGenerator,
        IMapper mapper)
    {
        _context = context;
        _numberGenerator = numberGenerator;
        _mapper = mapper;
    }

    public async Task<Result<QuotationDetailDto>> Handle(
        GenerateQuotationCommand request,
        CancellationToken cancellationToken)
    {
        // ── Step 1: Parse the tier enum ──────────────────────────────
        if (!Enum.TryParse<QuotationTier>(request.Tier, ignoreCase: true, out var tier))
            return Result<QuotationDetailDto>.Failure($"Invalid tier: {request.Tier}");

        // ── Step 2: Load the project with ALL nested data ────────────
        // Eager-loading via Include/ThenInclude fetches the entire object
        // graph in one SQL query, avoiding the N+1 problem.
        var project = await _context.Projects
            .Include(p => p.Owner)
            .Include(p => p.Rooms)
                .ThenInclude(r => r.Items)
                    .ThenInclude(i => i.Material)
            .FirstOrDefaultAsync(p => p.Id == request.ProjectId, cancellationToken);

        if (project == null)
            return Result<QuotationDetailDto>.Failure("Project not found.");

        if (!project.Rooms.Any())
            return Result<QuotationDetailDto>.Failure("Project has no rooms. Add rooms before generating a quotation.");

        // ── Step 3: Build line items and calculate totals ─────────────
        var lineItems = new List<QuotationLineItem>();

        foreach (var room in project.Rooms)
        {
            foreach (var item in room.Items)
            {
                // RATE SELECTION LOGIC:
                // Custom rates take priority over catalog rates. This lets
                // designers negotiate special rates with vendors on a per-item
                // basis while still defaulting to the material catalog.
                var unitRate = GetRateForTier(item, tier);

                // If we can't determine a rate (no material, no custom rate),
                // skip this item rather than failing the entire quotation.
                if (unitRate == 0) continue;

                var totalAmount = item.Quantity * unitRate;

                lineItems.Add(new QuotationLineItem
                {
                    QuotationId = Guid.Empty, // Will be set after quotation is created.
                    RoomType = room.RoomType,
                    RoomName = room.CustomName ?? room.RoomType.ToString(),
                    ItemName = item.Name,
                    Category = item.Category,
                    Quantity = item.Quantity,
                    Unit = item.Unit,
                    UnitRate = unitRate,
                    TotalAmount = totalAmount,
                    // Denormalised snapshot from the material catalog.
                    MaterialName = item.Material?.Name,
                    MaterialBrand = item.Material?.Brand
                });
            }
        }

        if (!lineItems.Any())
            return Result<QuotationDetailDto>.Failure("No priceable items found. Ensure items have materials or custom rates.");

        // ── Step 4: Financial calculations ────────────────────────────
        var subTotal = lineItems.Sum(li => li.TotalAmount);

        // Discount: applied before tax (standard accounting practice).
        var discountPercentage = request.DiscountPercentage ?? 0;
        var discountAmount = subTotal * discountPercentage / 100;
        var afterDiscount = subTotal - discountAmount;

        // Tax: 18% GST — the standard rate for interior design services in India.
        const decimal taxPercentage = 18;
        var taxAmount = afterDiscount * taxPercentage / 100;
        var grandTotal = afterDiscount + taxAmount;

        // ── Step 5: Estimate timeline ─────────────────────────────────
        // Timeline estimation uses a simple formula based on industry norms:
        //   Basic   = 30 base days + 5 days per room  (minimal finishes)
        //   Standard = 45 base days + 7 days per room  (mid-range finishes)
        //   Premium  = 60 base days + 10 days per room (luxury finishes, more detail work)
        var roomCount = project.Rooms.Count;
        var estimatedDays = tier switch
        {
            QuotationTier.Basic    => 30 + (5 * roomCount),
            QuotationTier.Standard => 45 + (7 * roomCount),
            QuotationTier.Premium  => 60 + (10 * roomCount),
            _ => 45 + (7 * roomCount)
        };

        // ── Step 6: Generate quotation number ─────────────────────────
        var quotationNumber = await _numberGenerator.GenerateAsync();

        // ── Step 7: Assemble and persist the quotation ────────────────
        var quotation = new Quotation
        {
            QuotationNumber = quotationNumber,
            Tier = tier,
            Status = QuotationStatus.Draft,
            ProjectId = project.Id,
            DesignerId = request.DesignerId,
            SubTotal = subTotal,
            TaxPercentage = taxPercentage,
            TaxAmount = taxAmount,
            GrandTotal = grandTotal,
            DiscountPercentage = discountPercentage > 0 ? discountPercentage : null,
            DiscountAmount = discountAmount > 0 ? discountAmount : null,
            EstimatedDaysToComplete = estimatedDays,
            Notes = request.Notes,
            ValidUntil = DateTime.UtcNow.AddDays(30) // Quotations valid for 30 days.
        };

        _context.Quotations.Add(quotation);
        // SaveChanges here so that quotation.Id is generated (needed for line items).
        await _context.SaveChangesAsync(cancellationToken);

        // Link line items to the persisted quotation.
        foreach (var li in lineItems)
            li.QuotationId = quotation.Id;

        _context.QuotationLineItems.AddRange(lineItems);
        await _context.SaveChangesAsync(cancellationToken);

        // ── Step 8: Map to DTO and return ─────────────────────────────
        // Re-load to populate navigation properties for mapping.
        quotation.LineItems = lineItems;
        quotation.Project = project;

        return Result<QuotationDetailDto>.Success(_mapper.Map<QuotationDetailDto>(quotation));
    }

    /// <summary>
    /// Selects the unit rate for a given tier from either the item's custom
    /// rates or its linked material's catalog rates.
    ///
    /// PRIORITY: Custom rate → Material catalog rate → 0 (skip item).
    /// </summary>
    private static decimal GetRateForTier(RoomItem item, QuotationTier tier)
    {
        // First, check custom overrides.
        var customRate = tier switch
        {
            QuotationTier.Basic    => item.CustomBasicRate,
            QuotationTier.Standard => item.CustomStandardRate,
            QuotationTier.Premium  => item.CustomPremiumRate,
            _ => null
        };

        if (customRate.HasValue && customRate.Value > 0)
            return customRate.Value;

        // Fall back to material catalog rates.
        if (item.Material == null)
            return 0;

        return tier switch
        {
            QuotationTier.Basic    => item.Material.BasicRate,
            QuotationTier.Standard => item.Material.StandardRate,
            QuotationTier.Premium  => item.Material.PremiumRate,
            _ => item.Material.StandardRate
        };
    }
}

// ═══════════════════════════════════════════════════════════════════════
// UPDATE QUOTATION STATUS
// ═══════════════════════════════════════════════════════════════════════
public class UpdateQuotationStatusCommandHandler : IRequestHandler<UpdateQuotationStatusCommand, Result<QuotationDto>>
{
    private readonly IArqoraDbContext _context;
    private readonly IMapper _mapper;

    public UpdateQuotationStatusCommandHandler(IArqoraDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Result<QuotationDto>> Handle(UpdateQuotationStatusCommand request, CancellationToken cancellationToken)
    {
        var quotation = await _context.Quotations
            .Include(q => q.Project)
            .FirstOrDefaultAsync(q => q.Id == request.Id, cancellationToken);

        if (quotation == null)
            return Result<QuotationDto>.Failure("Quotation not found.");

        if (!Enum.TryParse<QuotationStatus>(request.Status, ignoreCase: true, out var status))
            return Result<QuotationDto>.Failure($"Invalid status: {request.Status}");

        quotation.Status = status;
        await _context.SaveChangesAsync(cancellationToken);

        return Result<QuotationDto>.Success(_mapper.Map<QuotationDto>(quotation));
    }
}

// ═══════════════════════════════════════════════════════════════════════
// QUERY HANDLERS
// ═══════════════════════════════════════════════════════════════════════

public class GetQuotationByIdQueryHandler : IRequestHandler<GetQuotationByIdQuery, Result<QuotationDetailDto>>
{
    private readonly IArqoraDbContext _context;
    private readonly IMapper _mapper;

    public GetQuotationByIdQueryHandler(IArqoraDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Result<QuotationDetailDto>> Handle(GetQuotationByIdQuery request, CancellationToken cancellationToken)
    {
        var quotation = await _context.Quotations
            .AsNoTracking()
            .Include(q => q.Project)
            .Include(q => q.Designer)
            .Include(q => q.LineItems)
            .FirstOrDefaultAsync(q => q.Id == request.Id, cancellationToken);

        if (quotation == null)
            return Result<QuotationDetailDto>.Failure("Quotation not found.");

        return Result<QuotationDetailDto>.Success(_mapper.Map<QuotationDetailDto>(quotation));
    }
}

public class GetQuotationsByProjectQueryHandler : IRequestHandler<GetQuotationsByProjectQuery, Result<List<QuotationDto>>>
{
    private readonly IArqoraDbContext _context;
    private readonly IMapper _mapper;

    public GetQuotationsByProjectQueryHandler(IArqoraDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Result<List<QuotationDto>>> Handle(GetQuotationsByProjectQuery request, CancellationToken cancellationToken)
    {
        var quotations = await _context.Quotations
            .AsNoTracking()
            .Include(q => q.Project)
            .Where(q => q.ProjectId == request.ProjectId)
            .OrderByDescending(q => q.CreatedAt)
            .ToListAsync(cancellationToken);

        return Result<List<QuotationDto>>.Success(_mapper.Map<List<QuotationDto>>(quotations));
    }
}

// ═══════════════════════════════════════════════════════════════════════
// QUOTATION SUMMARY — CATEGORY & ROOM BREAKDOWNS
// ═══════════════════════════════════════════════════════════════════════
public class GetQuotationSummaryQueryHandler : IRequestHandler<GetQuotationSummaryQuery, Result<QuotationSummaryDto>>
{
    private readonly IArqoraDbContext _context;

    public GetQuotationSummaryQueryHandler(IArqoraDbContext context)
    {
        _context = context;
    }

    public async Task<Result<QuotationSummaryDto>> Handle(GetQuotationSummaryQuery request, CancellationToken cancellationToken)
    {
        var quotation = await _context.Quotations
            .AsNoTracking()
            .Include(q => q.LineItems)
            .FirstOrDefaultAsync(q => q.Id == request.QuotationId, cancellationToken);

        if (quotation == null)
            return Result<QuotationSummaryDto>.Failure("Quotation not found.");

        // Group line items by category to produce a category-wise cost breakdown.
        var categoryBreakdown = quotation.LineItems
            .GroupBy(li => li.Category.ToString())
            .ToDictionary(g => g.Key, g => g.Sum(li => li.TotalAmount));

        // Group line items by room to produce a room-wise cost breakdown.
        var roomBreakdown = quotation.LineItems
            .GroupBy(li => li.RoomName)
            .ToDictionary(g => g.Key, g => g.Sum(li => li.TotalAmount));

        return Result<QuotationSummaryDto>.Success(new QuotationSummaryDto
        {
            CategoryBreakdown = categoryBreakdown,
            RoomBreakdown = roomBreakdown,
            SubTotal = quotation.SubTotal,
            TaxAmount = quotation.TaxAmount,
            GrandTotal = quotation.GrandTotal,
            EstimatedDays = quotation.EstimatedDaysToComplete
        });
    }
}

// ═══════════════════════════════════════════════════════════════════════
// QUOTATION PDF GENERATION
// ═══════════════════════════════════════════════════════════════════════
public class GetQuotationPdfQueryHandler : IRequestHandler<GetQuotationPdfQuery, Result<byte[]>>
{
    private readonly IArqoraDbContext _context;
    private readonly IQuotationPdfService _pdfService;

    public GetQuotationPdfQueryHandler(IArqoraDbContext context, IQuotationPdfService pdfService)
    {
        _context = context;
        _pdfService = pdfService;
    }

    public async Task<Result<byte[]>> Handle(GetQuotationPdfQuery request, CancellationToken cancellationToken)
    {
        var quotation = await _context.Quotations
            .AsNoTracking()
            .Include(q => q.Project).ThenInclude(p => p.Owner)
            .Include(q => q.Designer)
            .Include(q => q.LineItems)
            .FirstOrDefaultAsync(q => q.Id == request.QuotationId, cancellationToken);

        if (quotation == null)
            return Result<byte[]>.Failure("Quotation not found.");

        // Map domain data to the flat PDF DTO.
        var pdfDto = new QuotationPdfDto
        {
            QuotationNumber = quotation.QuotationNumber,
            QuotationDate = quotation.CreatedAt,
            ValidUntil = quotation.ValidUntil,
            ClientName = quotation.Project.Owner.FullName,
            ClientEmail = quotation.Project.Owner.Email,
            ClientPhone = quotation.Project.Owner.PhoneNumber,
            ProjectName = quotation.Project.Name,
            ProjectAddress = quotation.Project.Address,
            TotalAreaSqFt = quotation.Project.TotalAreaSqFt,
            DesignerName = quotation.Designer?.FullName,
            DesignerCompany = quotation.Designer?.CompanyName,
            SubTotal = quotation.SubTotal,
            DiscountPercentage = quotation.DiscountPercentage,
            DiscountAmount = quotation.DiscountAmount,
            TaxPercentage = quotation.TaxPercentage,
            TaxAmount = quotation.TaxAmount,
            GrandTotal = quotation.GrandTotal,
            EstimatedDaysToComplete = quotation.EstimatedDaysToComplete,
            Notes = quotation.Notes,
            Rooms = quotation.LineItems
                .GroupBy(li => li.RoomName)
                .Select(g => new QuotationPdfRoomDto
                {
                    RoomName = g.Key,
                    Items = g.Select(li => new QuotationPdfLineItemDto
                    {
                        ItemName = li.ItemName,
                        Category = li.Category.ToString(),
                        Quantity = li.Quantity,
                        Unit = li.Unit.ToString(),
                        UnitRate = li.UnitRate,
                        TotalAmount = li.TotalAmount,
                        MaterialName = li.MaterialName,
                        MaterialBrand = li.MaterialBrand
                    }).ToList()
                }).ToList(),
            CategorySummaries = quotation.LineItems
                .GroupBy(li => li.Category.ToString())
                .Select(g => new CategorySummaryDto
                {
                    Category = g.Key,
                    Amount = g.Sum(li => li.TotalAmount),
                    Percentage = quotation.SubTotal > 0
                        ? Math.Round(g.Sum(li => li.TotalAmount) / quotation.SubTotal * 100, 1)
                        : 0
                }).ToList()
        };

        var pdfBytes = _pdfService.GenerateQuotationPdf(pdfDto);
        return Result<byte[]>.Success(pdfBytes);
    }
}
