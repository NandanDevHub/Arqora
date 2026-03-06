namespace Arqora.Domain.Common;

/// <summary>
/// Every entity in the domain inherits from this base class so that
/// cross-cutting concerns (identity, auditing) are defined once.
/// Using an abstract class instead of an interface lets us provide
/// default values (e.g. auto-generated Id) that concrete entities
/// inherit without any extra code.
/// </summary>
public abstract class BaseEntity
{
    /// <summary>
    /// A GUID primary key is preferred over an auto-increment int because
    /// it can be generated on the client side, which is essential for
    /// offline-capable or distributed systems and avoids round-trips to
    /// the database just to obtain an Id.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    // Audit fields let us track WHO changed WHAT and WHEN without relying
    // on database triggers. Keeping them in the base entity means every
    // table gets these columns automatically via EF Core inheritance.
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }
}
