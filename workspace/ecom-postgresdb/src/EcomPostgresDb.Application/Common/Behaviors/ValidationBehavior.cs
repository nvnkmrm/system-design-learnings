using FluentValidation;
using MediatR;

namespace EcomPostgresDb.Application.Common.Behaviors;

/// <summary>
/// MediatR Pipeline Behaviour — Validation.
/// Automatically validates all commands/queries that have registered validators.
/// This is the Pipeline pattern: each behaviour wraps the next handler.
/// </summary>
public sealed class ValidationBehavior<TRequest, TResponse>(
    IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!validators.Any()) return await next(cancellationToken);

        var context = new ValidationContext<TRequest>(request);
        var failures = validators
            .Select(v => v.Validate(context))
            .SelectMany(r => r.Errors)
            .Where(f => f is not null)
            .ToList();

        if (failures.Count != 0)
            throw new FluentValidation.ValidationException(failures);

        return await next(cancellationToken);
    }
}
