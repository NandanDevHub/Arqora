using System.Net;
using System.Text.Json;

namespace Arqora.API.Middleware;

/// <summary>
/// GLOBAL EXCEPTION HANDLER MIDDLEWARE
/// ────────────────────────────────────
/// Catches any unhandled exception that escapes from controllers or MediatR
/// handlers and returns a standardised JSON error response instead of letting
/// ASP.NET Core return an HTML error page or stack trace.
///
/// This ensures the API *always* returns JSON, even for 500-level errors,
/// which simplifies error handling on the frontend.
///
/// MIDDLEWARE PIPELINE ORDER MATTERS:
/// This middleware should be registered early (before routing/auth) so it
/// wraps the entire pipeline and catches exceptions from any stage.
/// </summary>
public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var (statusCode, message) = exception switch
        {
            UnauthorizedAccessException => (HttpStatusCode.Unauthorized, "You are not authorized."),
            KeyNotFoundException => (HttpStatusCode.NotFound, "The requested resource was not found."),
            ArgumentException ex => (HttpStatusCode.BadRequest, ex.Message),
            FluentValidation.ValidationException ex => (HttpStatusCode.BadRequest,
                string.Join("; ", ex.Errors.Select(e => e.ErrorMessage))),
            _ => (HttpStatusCode.InternalServerError, "An unexpected error occurred.")
        };

        context.Response.StatusCode = (int)statusCode;

        var response = new
        {
            statusCode = (int)statusCode,
            message,
            // Only include the detailed error in Development; hide in Production.
            detail = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development"
                ? exception.Message
                : null
        };

        var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(json);
    }
}

/// <summary>Extension method for clean registration in Program.cs.</summary>
public static class ExceptionMiddlewareExtensions
{
    public static IApplicationBuilder UseExceptionMiddleware(this IApplicationBuilder app)
    {
        return app.UseMiddleware<ExceptionMiddleware>();
    }
}
