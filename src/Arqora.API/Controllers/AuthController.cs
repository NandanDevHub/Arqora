using System.Security.Claims;
using Arqora.Application.Features.Auth.Commands;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Arqora.API.Controllers;

/// <summary>
/// AUTHENTICATION CONTROLLER
/// ─────────────────────────
/// Handles user registration, login, and retrieving the current user profile.
///
/// LEARNING: [AllowAnonymous] vs [Authorize]
/// ──────────────────────────────────────────
/// • [AllowAnonymous] — permits unauthenticated access. Used on register/login
///   because the user doesn't have a token yet.
/// • [Authorize] — requires a valid JWT Bearer token in the Authorization header.
///   The JWT middleware validates the token's signature, expiry, issuer, and audience
///   before the request reaches the controller action.
///
/// LEARNING: How JWT Claims Work
/// ─────────────────────────────
/// When a user logs in, the server issues a JWT containing claims (key-value pairs)
/// like sub (user ID), email, role, etc. On subsequent requests the client sends
/// this token in the "Authorization: Bearer {token}" header. ASP.NET Core's JWT
/// middleware decodes the token and populates HttpContext.User.Claims, making them
/// available via User.FindFirstValue(ClaimTypes.XXX).
/// </summary>
public class AuthController : BaseApiController
{
    /// <summary>
    /// POST /api/auth/register
    /// Creates a new user account and returns a JWT token.
    /// [AllowAnonymous] because the caller has no token yet.
    /// </summary>
    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterCommand command)
    {
        var result = await Mediator.Send(command);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }

    /// <summary>
    /// POST /api/auth/login
    /// Validates credentials and returns a JWT token.
    /// [AllowAnonymous] because the caller has no token yet.
    /// </summary>
    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginQuery query)
    {
        var result = await Mediator.Send(query);
        return result.IsSuccess ? Ok(result.Value) : Unauthorized(result.Error);
    }

    /// <summary>
    /// GET /api/auth/me
    /// Returns the current user's profile information extracted from JWT claims.
    /// [Authorize] ensures only authenticated users can call this endpoint.
    ///
    /// LEARNING: ClaimTypes mapping
    /// • ClaimTypes.NameIdentifier → the "sub" claim (user ID)
    /// • ClaimTypes.Email → the "email" claim
    /// • ClaimTypes.Name → the "unique_name" claim (full name)
    /// • ClaimTypes.Role → the "role" claim
    /// </summary>
    [Authorize]
    [HttpGet("me")]
    public IActionResult GetCurrentUser()
    {
        return Ok(new
        {
            Id = User.FindFirstValue(ClaimTypes.NameIdentifier),
            Email = User.FindFirstValue(ClaimTypes.Email),
            FullName = User.FindFirstValue(ClaimTypes.Name),
            Role = User.FindFirstValue(ClaimTypes.Role)
        });
    }
}
