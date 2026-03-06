using Arqora.Application.Common;
using Arqora.Application.DTOs;
using Arqora.Application.Features.Auth.Commands;
using Arqora.Application.Interfaces;
using Arqora.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Arqora.Application.Features.Auth.Handlers;

/// <summary>
/// Handles login by validating credentials via ASP.NET Identity's
/// UserManager and returning a JWT token on success.
/// </summary>
public class LoginQueryHandler : IRequestHandler<LoginQuery, Result<AuthResponseDto>>
{
    private readonly UserManager<AppUser> _userManager;
    private readonly ITokenService _tokenService;

    public LoginQueryHandler(UserManager<AppUser> userManager, ITokenService tokenService)
    {
        _userManager = userManager;
        _tokenService = tokenService;
    }

    public async Task<Result<AuthResponseDto>> Handle(LoginQuery request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
            return Result<AuthResponseDto>.Failure("Invalid email or password.");

        if (!user.IsActive)
            return Result<AuthResponseDto>.Failure("This account has been deactivated.");

        var isValidPassword = await _userManager.CheckPasswordAsync(user, request.Password);
        if (!isValidPassword)
            return Result<AuthResponseDto>.Failure("Invalid email or password.");

        var roles = await _userManager.GetRolesAsync(user);
        var token = _tokenService.CreateToken(user, roles);

        return Result<AuthResponseDto>.Success(new AuthResponseDto
        {
            Token = token,
            Email = user.Email!,
            FullName = user.FullName,
            Role = user.Role.ToString(),
            ExpiresAt = DateTime.UtcNow.AddHours(24)
        });
    }
}
