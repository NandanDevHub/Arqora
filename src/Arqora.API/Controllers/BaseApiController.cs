using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Arqora.API.Controllers;

/// <summary>
/// BASE CONTROLLER — shared configuration for all API controllers.
/// ────────────────────────────────────────────────────────────────
/// [ApiController] enables automatic model validation, binding source inference,
/// and ProblemDetails responses for 400 errors.
///
/// [Route("api/[controller]")] sets a conventional route prefix; [controller]
/// is replaced by the class name minus "Controller" (e.g. AuthController → api/auth).
///
/// The Mediator property lazily resolves IMediator from DI so derived controllers
/// don't need constructor injection for it.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public abstract class BaseApiController : ControllerBase
{
    private IMediator? _mediator;

    /// <summary>
    /// Lazy-resolved MediatR instance. HttpContext.RequestServices is the
    /// scoped DI container for the current request — safe to use here because
    /// controllers are always instantiated within a request scope.
    /// </summary>
    protected IMediator Mediator =>
        _mediator ??= HttpContext.RequestServices.GetRequiredService<IMediator>();
}
