using ExecutionRouter.Application.Configuration;
using ExecutionRouter.Application.Models;
using ExecutionRouter.Application.Services;
using ExecutionRouter.Domain.Constants;
using ExecutionRouter.Domain.Entities;
using ExecutionRouter.Domain.Exceptions;
using ExecutionRouter.Domain.ValueObjects;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace ExecutionRouter.Api.Controllers;

/// <summary>
/// Main execution controller with catch-all routing
/// </summary>
[ApiController]
[Route("api/{**path}")]
public class ExecutionController(
    ExecutionOrchestrationService orchestrationService,
    IOptions<SecuritySettings> securityOptions,
    ILogger<ExecutionController> logger)
    : ControllerBase
{
    /// <summary>
    /// Catch-all route for the standard HTTP verbs
    /// </summary>
    [HttpGet]
    [HttpPost]
    [HttpPut]
    [HttpPatch]
    [HttpDelete]
    public async Task<IActionResult> ExecuteRequest(string path, CancellationToken cancellationToken = default)
    {
        var requestId = GetOrGenerateRequestId();
        var correlationId = GetCorrelationId();

        try
        {
            var executionRequest = await BuildExecutionRequestAsync(path, requestId, correlationId);
            var response = await orchestrationService.ExecuteAsync(executionRequest, cancellationToken);
            var responseDto = ExecutionResponseDto.FromDomain(response);
            AddResponseHeaders(responseDto);
            
            var statusCode = DetermineHttpStatusCode(response);
            return StatusCode(statusCode, responseDto);
        }
        catch (ValidationException ex)
        {
            logger.LogWarning("Validation failed for request {RequestId}: {Errors}", requestId, string.Join(", ", ex.Errors));
            
            var errorResponse = CreateErrorResponse(requestId, correlationId, "ValidationFailed", ex.Message);
            AddResponseHeaders(errorResponse);
            return BadRequest(errorResponse);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error processing request {RequestId}", requestId);
            
            var errorResponse = CreateErrorResponse(requestId, correlationId, "InternalError", "An unexpected error occurred");
            AddResponseHeaders(errorResponse);
            return StatusCode(StatusCodes.Status500InternalServerError, errorResponse);
        }
    }

    private async Task<ExecutionRequest> BuildExecutionRequestAsync(string path, string requestId, string? correlationId)
    {
        var executorType = Request.Query["executor"].FirstOrDefault() ??
           Request.Headers[Headers.ExecutionRouter.ExecutorType].FirstOrDefault() ??
           ExecutorTypes.Http;

        var queryParameters = Request.Query
            .Where(q => !string.Equals(q.Key, "executor", StringComparison.OrdinalIgnoreCase))
            .ToDictionary(q => q.Key, q => q.Value.ToString());

        var headers = Request.Headers
            .Where(h => ShouldIncludeHeader(h.Key))
            .ToDictionary(h => h.Key, h => h.Value.ToString());

        string? body = null;
        if (Request.ContentLength > 0)
        {
            using var reader = new StreamReader(Request.Body);
            body = await reader.ReadToEndAsync();
        }

        var securitySettings = securityOptions.Value;
        var timeoutSeconds = securitySettings.MaxTimeoutSeconds;
        if (Request.Headers.TryGetValue(Headers.Extended.XRequestTimeout, out var timeoutHeader) &&
            int.TryParse(timeoutHeader.FirstOrDefault(), out var parsedTimeout))
        {
            timeoutSeconds = Math.Min(parsedTimeout, securitySettings.MaxTimeoutSeconds);
        }

        return ExecutionRequest.Create(
            requestId,
            correlationId,
            executorType,
            Request.Method,
            path,
            timeoutSeconds: TimeSpan.FromSeconds(timeoutSeconds),
            queryParameters,
            headers,
            body);
    }

    private static bool ShouldIncludeHeader(string headerName)
    {
        var excludedHeaders = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            Headers.Standard.Host,
            Headers.Standard.Connection,
            Headers.Standard.ContentLength,
            Headers.Standard.TransferEncoding,
            Headers.Standard.Upgrade,
            Headers.Extended.XRequestId,
            Headers.Extended.XCorrelationId,
            Headers.ExecutionRouter.ExecutorType,
            Headers.Extended.XRequestTimeout
        };

        return !excludedHeaders.Contains(headerName);
    }

    private string GetOrGenerateRequestId() => Request.Headers[Headers.Extended.XRequestId].FirstOrDefault() ??
       Request.Headers[Headers.ExecutionRouter.RequestId].FirstOrDefault() ??
       Guid.NewGuid().ToString("N")[..12];

    private string? GetCorrelationId() => Request.Headers[Headers.Extended.XCorrelationId].FirstOrDefault() ??
        Request.Headers[Headers.ExecutionRouter.CorrelationId].FirstOrDefault();

    private void AddResponseHeaders(ExecutionResponseDto response)
    {
        Response.Headers[Headers.ExecutionRouter.RequestId] = response.RequestId;
        Response.Headers[Headers.ExecutionRouter.Instance] = Environment.MachineName;
        Response.Headers[Headers.ExecutionRouter.AttemptCount] = response.AttemptSummaries.Count().ToString();
        Response.Headers[Headers.ExecutionRouter.Duration] = $"{response.DurationMilliseconds:F2}ms";
        
        if (!string.IsNullOrEmpty(response.CorrelationId))
        {
            Response.Headers[Headers.ExecutionRouter.CorrelationId] = response.CorrelationId;
        }
    }

    private void AddResponseHeaders(object errorResponse)
    {
        if (errorResponse.GetType().GetProperty("RequestId")?.GetValue(errorResponse) is not string requestId)
        {
            return;
        }
        
        Response.Headers[Headers.ExecutionRouter.RequestId] = requestId;
        Response.Headers[Headers.ExecutionRouter.Instance] = Environment.MachineName;
    }

    private static int DetermineHttpStatusCode(ExecutionResponse response)
    {
        if (response.Status is ExecutionStatus.Success)
        {
            return response.Result.StatusCode ?? StatusCodes.Status200OK;
        }
        
        if (response.Result.StatusCode.HasValue)
        {
            return response.Result.StatusCode.Value;
        }

        return response.Status switch
        {
            ExecutionStatus.Timeout => StatusCodes.Status408RequestTimeout,
            ExecutionStatus.Cancelled => StatusCodes.Status499ClientClosedRequest,
            _ => StatusCodes.Status502BadGateway
        };
    }

    private static object CreateErrorResponse(string requestId, string? correlationId, string errorType, string message)
    {
        return new
        {
            RequestId = requestId,
            CorrelationId = correlationId,
            Status = "failed",
            ErrorType = errorType,
            ErrorMessage = message,
            Timestamp = DateTime.UtcNow
        };
    }
}