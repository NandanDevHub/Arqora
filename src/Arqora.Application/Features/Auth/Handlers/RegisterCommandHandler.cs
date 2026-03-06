using Arqora.Application.Common;
using Arqora.Application.DTOs;
using Arqora.Application.Features.Auth.Commands;
using Arqora.Application.Interfaces;
using Arqora.Domain.Entities;
using Arqora.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Arqora.Application.Features.Auth.Handlers;

/// <summary>
/// MEDIATR HANDLER — REGISTER
/// ──────────────────────────
/// Every MediatR request has exactly one handler. The handler contains
/// the business logic for that specific operation. This separation means
/// controllers stay thin (just dispatch the request) and handlers are
/// independently testable (inject mocks for UserManager, ITokenService).
/// </summary>
public class RegisterCommandHandler : IRequestHandler<RegisterCommand, Result<AuthResponseDto>>
{
    private readonly UserManager<AppUser> _userManager;
    private readonly ITokenService _tokenService;

    public RegisterCommandHandler(UserManager<AppUser> userManager, ITokenService tokenService)
    {
        _userManager = userManager;
        _tokenService = tokenService;
    }

    public async Task<Result<AuthResponseDto>> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        // Check if a user with this email already exists.
        var existingUser = await _userManager.FindByEmailAsync(request.Email);
        if (existingUser != null)
            return Result<AuthResponseDto>.Failure("A user with this email already exists.");

        // Parse the role string into the domain enum for type safety.
        if (!Enum.TryParse<UserRole>(request.Role, ignoreCase: true, out var role))
            return Result<AuthResponseDto>.Failure("Invalid role specified.");

        var user = new AppUser
        {
            Email = request.Email,
            UserName = request.Email,
            FullName = request.FullName,
            Role = role,
            CompanyName = request.CompanyName,
            PhoneNumber = request.PhoneNumber,
            City = request.City
        };

        // UserManager handles password hashing, validation, and persistence.
        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            var errors = string.Join("; ", result.Errors.Select(e => e.Description));
            return Result<AuthResponseDto>.Failure(errors);
        }

        // Assign the ASP.NET Identity role (used for [Authorize(Roles = "...")] checks).
        await _userManager.AddToRoleAsync(user, request.Role);

        var roles = await _userManager.GetRolesAsync(user);
        var token = _tokenService.CreateToken(user, roles);

        return Result<AuthResponseDto>.Success(new AuthResponseDto
        {
            Token = token,
            Email = user.Email!,
            FullName = user.FullName,
            Role = request.Role,
            ExpiresAt = DateTime.UtcNow.AddHours(24)
        });
    }
}
