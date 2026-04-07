using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace EcomPostgresDb.API.Middleware;

/// <summary>
/// Global exception handler — maps domain/application exceptions to HTTP responses.
/// Keeps controllers clean by centralising error translation.
/// </summary>
public sealed class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext ctx, Exception ex, CancellationToken ct)
    {
        logger.LogError(ex, "Unhandled exception for {Method} {Path}", ctx.Request.Method, ctx.Request.Path);

        var (statusCode, title) = ex switch
        {
            ValidationException ve => (StatusCodes.Status400BadRequest, FormatValidationErrors(ve)),
            KeyNotFoundException => (StatusCodes.Status404NotFound, ex.Message),
            UnauthorizedAccessException => (StatusCodes.Status403Forbidden, ex.Message),
            InvalidOperationException => (StatusCodes.Status409Conflict, ex.Message),
            _ => (StatusCodes.Status500InternalServerError, "An unexpected error occurred.")
        };

        ctx.Response.StatusCode = statusCode;
        await ctx.Response.WriteAsJsonAsync(new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Instance = ctx.Request.Path
        }, ct);

        return true;
    }

    private static string FormatValidationErrors(ValidationException ex) =>
        string.Join("; ", ex.Errors.Select(e => e.ErrorMessage));
}
