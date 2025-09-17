namespace ExecutionRouter.Domain.ValueObjects;

/// <summary>
/// Represents the result from an executor
/// </summary>
public sealed record ExecutorResult
{
    public int? StatusCode { get; }
    public Dictionary<string, string> Headers { get; }
    public string? Body { get; }
    public Dictionary<string, object> Metadata { get; }

    private ExecutorResult(
        int? statusCode = null,
        Dictionary<string, string>? headers = null,
        string? body = null,
        Dictionary<string, object>? metadata = null)
    {
        StatusCode = statusCode;
        Headers = headers ?? new Dictionary<string, string>();
        Body = body;
        Metadata = metadata ?? new Dictionary<string, object>();
    }

    public static ExecutorResult Empty => new();
    
    public static ExecutorResult Http(int statusCode, Dictionary<string, string> headers, string body)
        => new(statusCode, headers, body);
    
    public static ExecutorResult PowerShell(string command, string stdout, string stderr, object? result = null)
    {
        var metadata = new Dictionary<string, object>
        {
            ["command"] = command,
            ["stdout"] = stdout,
            ["stderr"] = stderr
        };
        
        if (result != null)
        {
            metadata["result"] = result;
        }
        
        return new ExecutorResult(body: stdout, metadata: metadata);
    }
}