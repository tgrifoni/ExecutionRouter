using ExecutionRouter.Application.Models;
using Microsoft.AspNetCore.Mvc;
using ExecutionRouter.Domain.Interfaces;

namespace ExecutionRouter.Api.Controllers;

/// <summary>
/// Health and metrics endpoints
/// </summary>
[ApiController]
public class HealthController(IMetricsCollector metricsCollector, ILogger<HealthController> logger)
    : ControllerBase
{
    /// <summary>
    /// Simple health check endpoint
    /// </summary>
    [HttpGet("/ping")]
    [HttpGet("/health")]
    public IActionResult Ping()
    {
        var response = new
        {
            Status = "healthy",
            Service = "ExecutionRouter",
            Version = GetType().Assembly.GetName().Version?.ToString() ?? "1.0.0",
            Timestamp = DateTime.UtcNow,
            Instance = Environment.MachineName,
            Uptime = GetUptime()
        };

        return Ok(response);
    }

    /// <summary>
    /// Detailed health check with dependencies
    /// </summary>
    [HttpGet("/health/detailed")]
    public async Task<IActionResult> HealthDetailed()
    {
        var checks = new List<HealthCheck>();

        try
        {
            var metrics = metricsCollector.GetSnapshot();
            checks.Add(new HealthCheck("MetricsCollector", "healthy", $"Total requests: {metrics.TotalRequests}"));
        }
        catch (Exception ex)
        {
            checks.Add(new HealthCheck("MetricsCollector", "unhealthy", ex.Message));
        }

        checks.Add(await CheckSystemResourcesAsync());

        var overallStatus = checks.All(c => c.Status == "healthy") ? "healthy" : "degraded";

        var response = new
        {
            Status = overallStatus,
            Service = "ExecutionRouter",
            Version = GetType().Assembly.GetName().Version?.ToString() ?? "1.0.0",
            Timestamp = DateTime.UtcNow,
            Instance = Environment.MachineName,
            Uptime = GetUptime(),
            Checks = checks
        };

        var statusCode = overallStatus == "healthy" ? 200 : 503;
        return StatusCode(statusCode, response);
    }

    /// <summary>
    /// Metrics endpoint
    /// </summary>
    [HttpGet("/metrics")]
    public IActionResult Metrics()
    {
        try
        {
            var snapshot = metricsCollector.GetSnapshot();
            var metricsDto = MetricsSnapshotDto.FromDomain(snapshot);

            var enhancedMetrics = new
            {
                metricsDto.TotalRequests,
                metricsDto.SuccessfulRequests,
                metricsDto.FailedRequests,
                metricsDto.TransientFailures,
                metricsDto.RetriedRequests,
                AverageLatencyMs = metricsDto.AverageLatencyMilliseconds,
                P95LatencyMs = metricsDto.P95LatencyMilliseconds,
                metricsDto.SuccessRate,
                metricsDto.SnapshotTime,
                
                SystemMetrics = new
                {
                    WorkingSetMB = Math.Round(GC.GetTotalMemory(false) / 1024.0 / 1024.0, 2),
                    Gen0Collections = GC.CollectionCount(0),
                    Gen1Collections = GC.CollectionCount(1),
                    Gen2Collections = GC.CollectionCount(2),
                    ThreadCount = System.Diagnostics.Process.GetCurrentProcess().Threads.Count,
                    Uptime = GetUptime()
                }
            };

            return Ok(enhancedMetrics);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to retrieve metrics");
            return StatusCode(500, new { Error = "Failed to retrieve metrics", ex.Message });
        }
    }

    /// <summary>
    /// Reset metrics (for testing purposes)
    /// </summary>
    [HttpPost("/metrics/reset")]
    public IActionResult ResetMetrics()
    {
        return Ok(new { Message = "Metrics reset is not implemented in this demo version" });
    }

    private static string GetUptime()
    {
        var uptime = DateTime.UtcNow - System.Diagnostics.Process.GetCurrentProcess().StartTime.ToUniversalTime();
        return $"{uptime.Days}d {uptime.Hours}h {uptime.Minutes}m {uptime.Seconds}s";
    }

    private static async Task<HealthCheck> CheckSystemResourcesAsync()
    {
        try
        {
            await Task.Delay(1);
            
            var process = System.Diagnostics.Process.GetCurrentProcess();
            var workingSetInMb = Math.Round(process.WorkingSet64 / 1024.0 / 1024.0, 2);
            var cpuTime = process.TotalProcessorTime.TotalMilliseconds;
            
            var status = workingSetInMb > 1000 ? "degraded" : "healthy";
            var message = $"Working set: {workingSetInMb}MB, CPU time: {cpuTime}ms";
            
            return new HealthCheck("SystemResources", status, message);
        }
        catch (Exception ex)
        {
            return new HealthCheck("SystemResources", "unhealthy", ex.Message);
        }
    }

    private record HealthCheck(string Name, string Status, string Message);
}