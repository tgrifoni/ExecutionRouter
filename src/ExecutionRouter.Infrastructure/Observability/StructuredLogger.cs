namespace ExecutionRouter.Infrastructure.Observability;

/// <summary>
/// Structured logging implementation with sensitive data masking
/// </summary>
public sealed class StructuredLogger(bool maskSensitiveData = true)
{
    public void LogExecutionStart(string requestId, string? correlationId, string executorType, string method, string path)
    {
        var logEntry = new
        {
            Timestamp = DateTime.UtcNow,
            Level = "Information",
            Event = "ExecutionStarted",
            RequestId = requestId,
            CorrelationId = correlationId,
            ExecutorType = executorType,
            Method = method,
            Path = MaskSensitivePath(path)
        };

        Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(logEntry));
    }

    public static void LogExecutionEnd(string requestId, string? correlationId, string status, double durationMs, int attemptCount)
    {
        var logEntry = new
        {
            Timestamp = DateTime.UtcNow,
            Level = "Information",
            Event = "ExecutionCompleted",
            RequestId = requestId,
            CorrelationId = correlationId,
            Status = status,
            DurationMs = durationMs,
            AttemptCount = attemptCount
        };

        Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(logEntry));
    }

    public static void LogError(string requestId, string? correlationId, string errorMessage, Exception? exception = null)
    {
        var logEntry = new
        {
            Timestamp = DateTime.UtcNow,
            Level = "Error",
            Event = "ExecutionError",
            RequestId = requestId,
            CorrelationId = correlationId,
            ErrorMessage = errorMessage,
            ExceptionType = exception?.GetType().Name,
            exception?.StackTrace
        };

        Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(logEntry));
    }

    private string MaskSensitivePath(string path)
    {
        if (!maskSensitiveData)
        {
            return path;
        }

        var sensitivePatterns = new[] { "token=", "key=", "password=", "secret=" };
        
        foreach (var pattern in sensitivePatterns)
        {
            var index = path.IndexOf(pattern, StringComparison.OrdinalIgnoreCase);
            if (index < 0)
            {
                continue;
            }
            
            var start = index + pattern.Length;
            var end = path.IndexOfAny(['&', '?', '#'], start);
            if (end == -1)
            {
                end = path.Length;
            }
                
            path = path[..start] + "***MASKED***" + path[end..];
        }

        return path;
    }
}