using ExecutionRouter.Domain.Constants;

namespace ExecutionRouter.Api.Middleware;

/// <summary>
/// Middleware for handling global exceptions
/// </summary>
public class GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception occurred");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = System.Net.Mime.MediaTypeNames.Application.Json;
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;

        var requestId = context.Request.Headers[Headers.Extended.XRequestId].FirstOrDefault() ??
            context.Request.Headers[Headers.ExecutionRouter.RequestId].FirstOrDefault() ??
            "unknown";

        var response = new
        {
            RequestId = requestId,
            Status = "error",
            ErrorType = "InternalServerError",
            ErrorMessage = $"An unexpected error occurred: {exception}",
            Timestamp = DateTime.UtcNow
        };

        context.Response.Headers[Headers.ExecutionRouter.RequestId] = requestId;
        context.Response.Headers[Headers.ExecutionRouter.Instance] = Environment.MachineName;

        var jsonResponse = System.Text.Json.JsonSerializer.Serialize(response);
        await context.Response.WriteAsync(jsonResponse);
    }
}