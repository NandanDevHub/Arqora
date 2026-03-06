namespace Arqora.Application.DTOs;

/// <summary>
/// DTOs for Projects, Rooms, and Room Items.
/// ProjectDetailDto includes nested room and item data — this is the
/// "read model" pattern where the shape of the DTO matches what the
/// UI needs, not what the database tables look like.
/// </summary>

public class ProjectDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Address { get; set; }
    public decimal TotalAreaSqFt { get; set; }
    public string OwnerName { get; set; } = string.Empty;
    public int RoomCount { get; set; }
    public int QuotationCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class ProjectDetailDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Address { get; set; }
    public decimal TotalAreaSqFt { get; set; }
    public string OwnerName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<ProjectRoomDto> Rooms { get; set; } = [];
}

public class CreateProjectDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Address { get; set; }
    public decimal TotalAreaSqFt { get; set; }
}

public class UpdateProjectDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Address { get; set; }
    public decimal TotalAreaSqFt { get; set; }
}

public class ProjectRoomDto
{
    public Guid Id { get; set; }
    public string RoomType { get; set; } = string.Empty;
    public string? CustomName { get; set; }
    public decimal AreaSqFt { get; set; }
    public decimal? CeilingHeightFt { get; set; }

    /// <summary>Display name resolves CustomName ?? RoomType for UI convenience.</summary>
    public string DisplayName => CustomName ?? RoomType;

    public List<RoomItemDto> Items { get; set; } = [];
}

public class AddRoomDto
{
    public string RoomType { get; set; } = string.Empty;
    public string? CustomName { get; set; }
    public decimal AreaSqFt { get; set; }
    public decimal? CeilingHeightFt { get; set; }
}

public class RoomItemDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Category { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public string Unit { get; set; } = string.Empty;
    public Guid? MaterialId { get; set; }
    public string? MaterialName { get; set; }
    public decimal? CustomBasicRate { get; set; }
    public decimal? CustomStandardRate { get; set; }
    public decimal? CustomPremiumRate { get; set; }
}

public class AddRoomItemDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Category { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public string Unit { get; set; } = string.Empty;
    public Guid? MaterialId { get; set; }
    public decimal? CustomBasicRate { get; set; }
    public decimal? CustomStandardRate { get; set; }
    public decimal? CustomPremiumRate { get; set; }
}
