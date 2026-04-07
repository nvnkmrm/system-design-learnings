using MediatR;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace EcomPostgresDb.Application.Common.Behaviors;

/// <summary>
/// MediatR Pipeline Behaviour — Logging.
/// Logs entry/exit and execution time for all commands and queries.
/// </summary>
public sealed class LoggingBehavior<TRequest, TResponse>(
    ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        logger.LogInformation("Handling {RequestName}", requestName);

        var sw = Stopwatch.StartNew();
        try
        {
            var response = await next(cancellationToken);
            sw.Stop();
            logger.LogInformation("Handled {RequestName} in {Elapsed}ms", requestName, sw.ElapsedMilliseconds);
            return response;
        }
        catch (Exception ex)
        {
            sw.Stop();
            logger.LogError(ex, "Error handling {RequestName} after {Elapsed}ms", requestName, sw.ElapsedMilliseconds);
            throw;
        }
    }
}
