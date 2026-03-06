using Arqora.API.Extensions;
using Arqora.API.Middleware;
using Arqora.Application;
using Arqora.Domain.Entities;
using Arqora.Infrastructure;
using Arqora.Infrastructure.Data;
using Arqora.Infrastructure.Data.Seed;
using Microsoft.AspNetCore.Identity;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// ══════════════════════════════════════════════════════════════════
//  SERVICE REGISTRATION
// ══════════════════════════════════════════════════════════════════

// Application layer — MediatR, AutoMapper, FluentValidation
builder.Services.AddApplicationServices();

// Infrastructure layer — DbContext, Identity, TokenService, PDF
builder.Services.AddInfrastructureServices(builder.Configuration);

// JWT Bearer authentication — validates tokens on [Authorize] endpoints
builder.Services.AddIdentityServices(builder.Configuration);

// Controllers (MVC)
builder.Services.AddControllers();

// CORS — allow the Vite dev server during development
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowVite", policy =>
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials());
});

// Swagger / OpenAPI with JWT support so you can test authenticated
// endpoints directly from the Swagger UI.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Arqora API",
        Version = "v1",
        Description = "Interior design quotation platform API"
    });

    // Add a JWT Bearer security definition so Swagger UI shows the
    // "Authorize 🔒" button and sends the token in request headers.
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter your JWT token (without the 'Bearer ' prefix)."
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// ══════════════════════════════════════════════════════════════════
//  MIDDLEWARE PIPELINE
//  ORDER MATTERS — each middleware sees the request in registration
//  order and the response in reverse order:
//    ExceptionMiddleware → CORS → Auth → Routing → Controllers
// ══════════════════════════════════════════════════════════════════

// 1. Global exception handler — catches unhandled exceptions from the
//    entire pipeline and returns a standardised JSON error response.
app.UseExceptionMiddleware();

// 2. Swagger UI — only exposed in Development.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Arqora API v1"));
}

app.UseHttpsRedirection();

// 3. CORS must come before auth so preflight OPTIONS requests succeed.
app.UseCors("AllowVite");

// 4. Authentication then Authorisation — order is critical.
//    UseAuthentication reads and validates the JWT token.
//    UseAuthorization enforces [Authorize] / [Authorize(Roles = "...")]
app.UseAuthentication();
app.UseAuthorization();

// 5. Map controller routes.
app.MapControllers();

// ══════════════════════════════════════════════════════════════════
//  STARTUP — auto-migrate database and seed initial data
// ══════════════════════════════════════════════════════════════════
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ArqoraDbContext>();
        var userManager = services.GetRequiredService<UserManager<AppUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

        await DataSeeder.SeedAsync(context, userManager, roleManager);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred during database migration/seeding.");
    }
}

app.Run();
