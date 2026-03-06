namespace Arqora.Application.DTOs;

/// <summary>
/// DTOs (Data Transfer Objects) for authentication operations.
/// These are simple data carriers with no behaviour — they cross layer
/// boundaries (API → Application → back to API) without leaking domain entities.
/// </summary>

public class RegisterDto
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string? CompanyName { get; set; }
    public string? PhoneNumber { get; set; }
    public string? City { get; set; }
}

public class LoginDto
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class AuthResponseDto
{
    public string Token { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
}
