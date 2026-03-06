using System.Reflection;
using Arqora.Application.Common;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Arqora.Application;

/// <summary>
/// COMPOSITION ROOT — APPLICATION LAYER
/// ─────────────────────────────────────
/// Registers all Application-layer services into the DI container.
/// The API layer calls builder.Services.AddApplicationServices() and
/// gets MediatR, AutoMapper, and FluentValidation wired up automatically.
///
/// WHY AN EXTENSION METHOD?
/// Encapsulating DI registration in the layer that owns the services
/// follows the "Screaming Architecture" principle — the Application
/// project knows exactly what it needs, and the API project doesn't
/// need to know implementation details.
/// </summary>
public static class ApplicationServiceRegistration
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();

        // ── MediatR ───────────────────────────────────────────────
        // Scans this assembly for IRequestHandler implementations and
        // registers them. Also registers pipeline behaviors (like our
        // ValidationBehavior) which run before every handler.
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(assembly);
            // Pipeline behaviors execute in registration order.
            // ValidationBehavior runs first, ensuring invalid requests
            // never reach the handler.
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        });

        // ── AutoMapper ────────────────────────────────────────────
        // Scans this assembly for Profile subclasses (MappingProfiles)
        // and compiles all mapping configurations at startup.
        services.AddAutoMapper(assembly);

        // ── FluentValidation ──────────────────────────────────────
        // Scans this assembly for AbstractValidator<T> subclasses and
        // registers them so the ValidationBehavior can resolve them.
        services.AddValidatorsFromAssembly(assembly);

        return services;
    }
}
