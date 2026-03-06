using FluentValidation;
using MediatR;

namespace Arqora.Application.Common;

/// <summary>
/// MEDIATR PIPELINE BEHAVIOR — VALIDATION
/// ───────────────────────────────────────
/// MediatR supports a "pipeline" concept similar to ASP.NET middleware.
/// Before a request reaches its handler, it passes through a chain of
/// IPipelineBehavior implementations. This behavior intercepts every
/// request that has registered FluentValidation validators and runs
/// them automatically.
///
/// WHY A PIPELINE BEHAVIOR?
/// Without this, every handler would need to manually call validators —
/// repetitive, error-prone, and easy to forget. The pipeline centralises
/// validation so that handlers can assume their input is already valid.
///
/// HOW IT WORKS:
/// 1. MediatR dispatches a request (e.g. CreateMaterialCommand).
/// 2. DI resolves all IValidator&lt;CreateMaterialCommand&gt; instances.
/// 3. This behavior runs them. If any fail → return Result.Failure.
/// 4. If all pass → call next() to invoke the actual handler.
/// </summary>
public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : class
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!_validators.Any())
            return await next();

        // Run all validators in parallel and collect results.
        var context = new ValidationContext<TRequest>(request);
        var validationResults = await Task.WhenAll(
            _validators.Select(v => v.ValidateAsync(context, cancellationToken)));

        var failures = validationResults
            .SelectMany(r => r.Errors)
            .Where(f => f != null)
            .ToList();

        if (failures.Count != 0)
        {
            var errorMessage = string.Join("; ", failures.Select(f => f.ErrorMessage));

            // Use reflection to call Result<T>.Failure if TResponse is a Result<T>.
            // This keeps the pipeline generic — it works with any Result<T> return type.
            var responseType = typeof(TResponse);
            if (responseType.IsGenericType && responseType.GetGenericTypeDefinition() == typeof(Result<>))
            {
                var failureMethod = responseType.GetMethod(nameof(Result<object>.Failure))!;
                return (TResponse)failureMethod.Invoke(null, [errorMessage])!;
            }

            // Fallback for non-Result return types — throw so the caller gets a clear signal.
            throw new ValidationException(failures);
        }

        return await next();
    }
}
