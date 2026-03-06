namespace Arqora.Application.DTOs;

/// <summary>
/// DTOs for the Material catalog. Separate Create/Update DTOs let us enforce
/// different validation rules (e.g. Update requires an Id, Create does not).
/// </summary>

public class MaterialDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Brand { get; set; }
    public string Category { get; set; } = string.Empty;
    public string Unit { get; set; } = string.Empty;
    public decimal BasicRate { get; set; }
    public decimal StandardRate { get; set; }
    public decimal PremiumRate { get; set; }
    public bool IsActive { get; set; }
    public string? ImageUrl { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class CreateMaterialDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Brand { get; set; }
    public string Category { get; set; } = string.Empty;
    public string Unit { get; set; } = string.Empty;
    public decimal BasicRate { get; set; }
    public decimal StandardRate { get; set; }
    public decimal PremiumRate { get; set; }
    public string? ImageUrl { get; set; }
}

public class UpdateMaterialDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Brand { get; set; }
    public string Category { get; set; } = string.Empty;
    public string Unit { get; set; } = string.Empty;
    public decimal BasicRate { get; set; }
    public decimal StandardRate { get; set; }
    public decimal PremiumRate { get; set; }
    public string? ImageUrl { get; set; }
}
