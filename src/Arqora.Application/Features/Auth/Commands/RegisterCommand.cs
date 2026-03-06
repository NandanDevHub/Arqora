using Arqora.Application.Common;
using Arqora.Application.DTOs;
using FluentValidation;
using MediatR;

namespace Arqora.Application.Features.Auth.Commands;

/// <summary>
/// CQRS — COMMAND
/// ──────────────
/// Commands represent intentions to CHANGE state (create, update, delete).
/// This command asks the system to register a new user. It implements
/// IRequest&lt;Result&lt;AuthResponseDto&gt;&gt; which tells MediatR:
///   "Send this to a handler and expect a Result&lt;AuthResponseDto&gt; back."
///
/// Commands are named as imperative verbs (Register, Create, Update) because
/// they describe an action, not a question.
/// </summary>
public class RegisterCommand : IRequest<Result<AuthResponseDto>>
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string? CompanyName { get; set; }
    public string? PhoneNumber { get; set; }
    public string? City { get; set; }
}

/// <summary>
/// FluentValidation validator — automatically invoked by ValidationBehavior
/// before the handler executes. If any rule fails, the handler is never called.
/// </summary>
public class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    public RegisterCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty().MinimumLength(6);
        RuleFor(x => x.FullName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Role).NotEmpty()
            .Must(r => r is "Admin" or "Designer" or "Homeowner")
            .WithMessage("Role must be Admin, Designer, or Homeowner.");
    }
}
