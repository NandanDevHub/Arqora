using Microsoft.AspNetCore.Identity;
using Arqora.Domain.Enums;

namespace Arqora.Domain.Entities;

/// <summary>
/// PRAGMATIC TRADE-OFF: In strict Domain-Driven Design the Domain layer should
/// have ZERO infrastructure dependencies. IdentityUser belongs to ASP.NET Core
/// Identity (an infrastructure concern), so a purist approach would define a
/// separate domain User entity and map it to IdentityUser in the Infrastructure
/// layer via an anti-corruption layer.
///
/// We inherit directly from IdentityUser here because:
///   1. It avoids duplicating dozens of properties (Email, PasswordHash, etc.).
///   2. For a learning project the added complexity of an anti-corruption layer
///      would obscure the core concepts without meaningful benefit.
///   3. The coupling is limited to this single class — the rest of the domain
///      depends on AppUser, not on IdentityUser directly.
///
/// If the project ever needs to swap out Identity (e.g. for a third-party auth
/// provider), only this class and the Infrastructure layer would change.
/// </summary>
public class AppUser : IdentityUser
{
    public string FullName { get; set; } = string.Empty;
    public UserRole Role { get; set; }

    /// <summary>
    /// Only meaningful for Designer users. Nullable because Homeowners
    /// and Admins don't have a company.
    /// </summary>
    public string? CompanyName { get; set; }

    // PhoneNumber is already declared in IdentityUser, so we intentionally
    // use the `new` keyword to shadow it with our own property that keeps
    // the same name for domain clarity.
    public new string? PhoneNumber { get; set; }

    public string? City { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties use ICollection<T> (not List<T>) because
    // EF Core only needs the collection interface — this keeps the entity
    // loosely coupled to any specific collection implementation.
    public ICollection<Project> Projects { get; set; } = new List<Project>();
    public ICollection<Quotation> CreatedQuotations { get; set; } = new List<Quotation>();
}
