using System.Text;
using ExecutionRouter.Domain.Entities;
using ExecutionRouter.Domain.Interfaces;
using ExecutionRouter.Domain.ValueObjects;
using ExecutionRouter.Domain.Exceptions;

namespace ExecutionRouter.Infrastructure.Executors;

/// <summary>
/// HTTP executor that forwards requests to external HTTP endpoints
/// </summary>
public sealed class HttpExecutor(HttpClient httpClient) : IExecutor
{
    public ExecutorType ExecutorType => ExecutorType.Http;

    private readonly HashSet<string> _allowedHeaders = new(StringComparer.OrdinalIgnoreCase)
    {
        "accept",
        "accept-encoding",
        "accept-language",
        "authorization",
        "cache-control",
        "content-type",
        "if-match",
        "if-none-match",
        "if-modified-since",
        "if-unmodified-since",
        "user-agent",
        "x-requested-with",
        "x-forwarded-for",
        "x-real-ip",
        "x-api-key",
        "x-auth-token",
        "x-correlation-id",
        "x-request-id"
    };

    public async Task<ExecutorResult> ExecuteAsync(ExecutionRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var targetUrl = BuildTargetUrl(request);
            
            using var httpRequestMessage = CreateHttpRequestMessage(request, targetUrl);
            
            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeoutCts.CancelAfter(request.TimeoutSeconds);
            
            using var response = await httpClient.SendAsync(httpRequestMessage, timeoutCts.Token);
            
            return await BuildExecutorResult(response);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw new ExecutionFailedException("Request was cancelled", isTransient: false);
        }
        catch (OperationCanceledException)
        {
            throw new ExecutionTimeoutException(request.TimeoutSeconds);
        }
        catch (HttpRequestException ex)
        {
            var isTransient = IsTransientHttpError(ex);
            throw new ExecutionFailedException($"HTTP request failed: {ex.Message}", ex, isTransient);
        }
        catch (Exception ex)
        {
            throw new ExecutionFailedException($"Unexpected error during HTTP execution: {ex.Message}", ex, isTransient: false);
        }
    }

    private static string BuildTargetUrl(ExecutionRequest request)
    {
        var baseUrl = ExtractBaseUrl(request.Path);
        var path = ExtractPath(request.Path);
        
        var uriBuilder = new UriBuilder(baseUrl) { Path = path };

        if (!request.QueryParameters.Any())
        {
            return uriBuilder.ToString();
        }
        
        var queryParams = new List<string>();
        if (!string.IsNullOrEmpty(uriBuilder.Query))
        {
            queryParams.Add(uriBuilder.Query.TrimStart('?'));
        }

        queryParams.AddRange(request.QueryParameters.Select(kvp => $"{Uri.EscapeDataString(kvp.Key)}={Uri.EscapeDataString(kvp.Value)}"));

        uriBuilder.Query = string.Join("&", queryParams);

        return uriBuilder.ToString();
    }

    private static string ExtractBaseUrl(string path)
    {
        if (!path.StartsWith("https://"))
        {
            return path;
        }
        
        var uri = new Uri(path);
        return $"{uri.Scheme}://{uri.Authority}";
    }

    private static string ExtractPath(string path)
    {
        if (!path.StartsWith("https://"))
        {
            return path;
        }
        
        var uri = new Uri(path);
        return uri.PathAndQuery;
    }

    private HttpRequestMessage CreateHttpRequestMessage(ExecutionRequest request, string targetUrl)
    {
        var httpRequest = new HttpRequestMessage(new HttpMethod(request.Method), targetUrl);
        var validHeaders = request.Headers.Where(header =>
            _allowedHeaders.Contains(header.Key) &&
            !header.Key.Equals("content-type", StringComparison.OrdinalIgnoreCase));
        
        foreach (var header in validHeaders)
        {
            httpRequest.Headers.Add(header.Key, header.Value);
        }
        
        httpRequest.Headers.Add("X-ExecutionRouter-RequestId", request.RequestId.Value);
        if (request.CorrelationId is not null)
        {
            httpRequest.Headers.Add("X-ExecutionRouter-CorrelationId", request.CorrelationId.Value);
        }

        if (string.IsNullOrEmpty(request.Body))
        {
            return httpRequest;
        }
        
        var contentType = request.Headers.GetValueOrDefault("content-type", "application/json");
        httpRequest.Content = new StringContent(request.Body, Encoding.UTF8, contentType);

        return httpRequest;
    }

    private static async Task<ExecutorResult> BuildExecutorResult(HttpResponseMessage response)
    {
        var headers = new Dictionary<string, string>();
        
        foreach (var header in response.Headers)
        {
            headers[header.Key] = string.Join(", ", header.Value);
        }
        
        foreach (var header in response.Content.Headers)
        {
            headers[header.Key] = string.Join(", ", header.Value);
        }
        
        var body = await response.Content.ReadAsStringAsync();
        if (body.Length > 10_000)
        {
            body = body[..10000] + "... (truncated)";
        }

        return ExecutorResult.Http((int)response.StatusCode, headers, body);
    }

    private static bool IsTransientHttpError(HttpRequestException ex)
    {
        var message = ex.Message.ToLowerInvariant();
        
        return message.Contains("timeout") ||
           message.Contains("connection reset") ||
           message.Contains("network is unreachable") ||
           message.Contains("temporary failure") ||
           message.Contains("service unavailable") ||
           message.Contains("too many requests");
    }
}