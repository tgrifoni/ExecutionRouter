using ExecutionRouter.Domain.Constants;
using ExecutionRouter.Domain.Interfaces;
using ExecutionRouter.Infrastructure.Observability;
using System.Diagnostics;

namespace ExecutionRouter.Api.Middleware;

/// <summary>
/// Middleware for request/response logging and metrics collection
/// </summary>
public class RequestLoggingMiddleware(
    RequestDelegate next,
    StructuredLogger logger,
    IMetricsCollector metricsCollector)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        var requestId = GetRequestId(context);
        var correlationId = GetCorrelationId(context);

        logger.LogExecutionStart(
            requestId,
            correlationId,
            "http",
            context.Request.Method,
            context.Request.Path);

        try
        {
            await next(context);
            
            stopwatch.Stop();
            
            StructuredLogger.LogExecutionEnd(
                requestId,
                correlationId,
                DetermineStatus(context.Response.StatusCode),
                stopwatch.Elapsed.TotalMilliseconds,
                1);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            
            StructuredLogger.LogError(requestId, correlationId, ex.Message, ex);
            
            throw;
        }
        finally
        {
            metricsCollector.RecordLatency(stopwatch.Elapsed);
        }
    }

    private static string GetRequestId(HttpContext context) =>
        context.Request.Headers["X-Request-Id"].FirstOrDefault() ??
        context.Request.Headers[Headers.ExecutionRouter.RequestId].FirstOrDefault() ??
        Guid.NewGuid().ToString("N")[..12];

    private static string? GetCorrelationId(HttpContext context) =>
        context.Request.Headers["X-Correlation-Id"].FirstOrDefault() ??
        context.Request.Headers[Headers.ExecutionRouter.CorrelationId].FirstOrDefault();

    private static string DetermineStatus(int statusCode) =>
        statusCode switch
        {
            >= 200 and < 300 => "success",
            >= 400 and < 500 => "client_error",
            >= 500 => "server_error",
            _ => "unknown"
        };
}