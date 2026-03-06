using Arqora.Domain.Entities;

namespace Arqora.Application.Interfaces;

/// <summary>
/// Abstracts JWT token creation so the Application layer can request tokens
/// without knowing anything about signing keys, token formats, or expiration
/// policies. The Infrastructure layer provides the concrete implementation.
/// This follows the Dependency Inversion Principle — high-level modules
/// (Application) depend on abstractions, not on low-level details (JWT libraries).
/// </summary>
public interface ITokenService
{
    /// <summary>
    /// Creates a signed JWT for the given user.
    /// Roles are passed in (rather than fetched internally) so that the
    /// implementation stays a pure function with no hidden I/O — the caller
    /// (typically a MediatR handler) is responsible for fetching roles from
    /// UserManager.
    /// </summary>
    string CreateToken(AppUser user, IList<string> roles);
}
