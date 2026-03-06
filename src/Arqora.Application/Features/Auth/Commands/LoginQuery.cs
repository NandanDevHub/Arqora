using Arqora.Application.Common;
using Arqora.Application.DTOs;
using MediatR;

namespace Arqora.Application.Features.Auth.Commands;

/// <summary>
/// CQRS — QUERY (placed in Commands folder for Auth grouping convenience)
/// ──────────────────────────────────────────────────────────────────────
/// Queries represent intentions to READ state without side effects.
/// Login is modelled as a query because it does not create or modify any
/// domain entity — it merely validates credentials and returns a token.
///
/// WHY SEPARATE COMMANDS FROM QUERIES?
/// The Command Query Responsibility Segregation (CQRS) pattern separates
/// read and write models. Benefits:
///   1. Each handler has a single responsibility (easier to test/maintain).
///   2. Reads can be optimised independently (e.g. no-tracking queries).
///   3. Writes can enforce invariants without read-model concerns.
///   4. Scales naturally — reads can hit a replica, writes hit the primary.
/// </summary>
public class LoginQuery : IRequest<Result<AuthResponseDto>>
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
