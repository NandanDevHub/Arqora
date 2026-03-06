using Arqora.Application.DTOs;
using Arqora.Domain.Entities;
using AutoMapper;

namespace Arqora.Application.Mappings;

/// <summary>
/// AutoMapper profile that defines how domain entities map to DTOs.
/// All mappings are centralised here so they're discoverable and auditable.
/// AutoMapper uses these profiles at startup to compile fast, expression-based
/// mappers — there's no reflection at runtime after initialisation.
/// </summary>
public class MappingProfiles : Profile
{
    public MappingProfiles()
    {
        // ─── Material ─────────────────────────────────────────────
        CreateMap<Material, MaterialDto>()
            .ForMember(d => d.Category, opt => opt.MapFrom(s => s.Category.ToString()))
            .ForMember(d => d.Unit, opt => opt.MapFrom(s => s.Unit.ToString()));

        // ─── Project ──────────────────────────────────────────────
        CreateMap<Project, ProjectDto>()
            .ForMember(d => d.OwnerName, opt => opt.MapFrom(s => s.Owner != null ? s.Owner.FullName : string.Empty))
            .ForMember(d => d.RoomCount, opt => opt.MapFrom(s => s.Rooms != null ? s.Rooms.Count : 0))
            .ForMember(d => d.QuotationCount, opt => opt.MapFrom(s => s.Quotations != null ? s.Quotations.Count : 0));

        CreateMap<Project, ProjectDetailDto>()
            .ForMember(d => d.OwnerName, opt => opt.MapFrom(s => s.Owner != null ? s.Owner.FullName : string.Empty));

        // ─── ProjectRoom ──────────────────────────────────────────
        CreateMap<ProjectRoom, ProjectRoomDto>()
            .ForMember(d => d.RoomType, opt => opt.MapFrom(s => s.RoomType.ToString()));

        // ─── RoomItem ─────────────────────────────────────────────
        CreateMap<RoomItem, RoomItemDto>()
            .ForMember(d => d.Category, opt => opt.MapFrom(s => s.Category.ToString()))
            .ForMember(d => d.Unit, opt => opt.MapFrom(s => s.Unit.ToString()))
            .ForMember(d => d.MaterialName, opt => opt.MapFrom(s => s.Material != null ? s.Material.Name : null));

        // ─── Quotation ────────────────────────────────────────────
        CreateMap<Quotation, QuotationDto>()
            .ForMember(d => d.Tier, opt => opt.MapFrom(s => s.Tier.ToString()))
            .ForMember(d => d.Status, opt => opt.MapFrom(s => s.Status.ToString()))
            .ForMember(d => d.ProjectName, opt => opt.MapFrom(s => s.Project != null ? s.Project.Name : string.Empty));

        CreateMap<Quotation, QuotationDetailDto>()
            .ForMember(d => d.Tier, opt => opt.MapFrom(s => s.Tier.ToString()))
            .ForMember(d => d.Status, opt => opt.MapFrom(s => s.Status.ToString()))
            .ForMember(d => d.ProjectName, opt => opt.MapFrom(s => s.Project != null ? s.Project.Name : string.Empty))
            .ForMember(d => d.DesignerName, opt => opt.MapFrom(s => s.Designer != null ? s.Designer.FullName : null));

        // ─── QuotationLineItem ────────────────────────────────────
        CreateMap<QuotationLineItem, QuotationLineItemDto>()
            .ForMember(d => d.RoomType, opt => opt.MapFrom(s => s.RoomType.ToString()))
            .ForMember(d => d.Category, opt => opt.MapFrom(s => s.Category.ToString()))
            .ForMember(d => d.Unit, opt => opt.MapFrom(s => s.Unit.ToString()));
    }
}
