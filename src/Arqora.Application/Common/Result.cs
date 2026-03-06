namespace Arqora.Application.Common;

/// <summary>
/// A generic result wrapper that every MediatR handler returns instead of
/// throwing exceptions for expected failures (e.g. "email already taken",
/// "project not found"). This pattern, sometimes called the Result/Either
/// pattern, gives callers a predictable shape to inspect — no try/catch
/// required for business-rule violations.
///
/// WHY NOT EXCEPTIONS?
/// Exceptions are expensive (stack trace capture) and should be reserved for
/// truly unexpected situations. Validation failures, not-found results, and
/// permission denials are *expected* outcomes that belong in the return type.
/// </summary>
public class Result<T>
{
    public bool IsSuccess { get; set; }
    public T? Value { get; set; }
    public string? Error { get; set; }

    /// <summary>Creates a successful result containing the given value.</summary>
    public static Result<T> Success(T value) => new() { IsSuccess = true, Value = value };

    /// <summary>Creates a failure result with an error message and no value.</summary>
    public static Result<T> Failure(string error) => new() { IsSuccess = false, Error = error };
}
