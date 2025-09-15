using System.Text;
using ExecutionRouter.Domain.Entities;
using ExecutionRouter.Domain.Interfaces;

namespace ExecutionRouter.Application.Validators;

/// <summary>
/// Service for validating execution requests
/// </summary>
public sealed class ValidationService(int maxBodySizeBytes = ValidationService.MaxBodySizeBytes) : IValidationService
{
    private const int MaxTimeoutSeconds = 600;
    private const int MaxBodySizeBytes = 10 * 1024 * 1024;
    private const int MaxHeaderValueLength = 1024;
    private const int MaxPathLength = 2048;

    private readonly HashSet<string> _allowedMethods = new(StringComparer.OrdinalIgnoreCase)
    {
        "GET", "POST", "PUT", "PATCH", "DELETE", "HEAD", "OPTIONS"
    };
    private readonly HashSet<string> _blockedHeaders = new(StringComparer.OrdinalIgnoreCase)
    {
        "host", "connection", "content-length", "transfer-encoding", "upgrade"
    };

    public ValidationResult ValidateRequest(ExecutionRequest request)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(request.Method))
        {
            errors.Add("Method is required");
        }
        else if (!_allowedMethods.Contains(request.Method))
        {
            errors.Add($"Unsupported HTTP method: {request.Method}");
        }

        if (string.IsNullOrWhiteSpace(request.Path))
        {
            errors.Add("Path is required");
        }
        else if (request.Path.Length > MaxPathLength)
        {
            errors.Add($"Path too long. Maximum length is {MaxPathLength} characters");
        }

        if (!string.IsNullOrEmpty(request.Body))
        {
            var bodyBytes = Encoding.UTF8.GetByteCount(request.Body);
            if (bodyBytes > maxBodySizeBytes)
            {
                errors.Add($"Request body too large. Maximum size is {maxBodySizeBytes} bytes");
            }
        }

        foreach (var header in request.Headers)
        {
            if (string.IsNullOrWhiteSpace(header.Key))
            {
                errors.Add("Header name cannot be empty");
                continue;
            }

            if (_blockedHeaders.Contains(header.Key))
            {
                errors.Add($"Header '{header.Key}' is not allowed");
                continue;
            }

            if (header.Value.Length > MaxHeaderValueLength)
            {
                errors.Add($"Header '{header.Key}' value too long. Maximum length is {MaxHeaderValueLength} characters");
            }
        }

        if (request.TimeoutSeconds <= TimeSpan.Zero)
        {
            errors.Add("Timeout must be greater than zero");
        }
        else if (request.TimeoutSeconds > TimeSpan.FromSeconds(MaxTimeoutSeconds))
        {
            errors.Add("Timeout cannot exceed 10 minutes");
        }

        ValidateExecutorSpecific(request, errors);

        return errors.Count != 0
            ? ValidationResult.Failure(errors.ToArray())
            : ValidationResult.Success();
    }

    private static void ValidateExecutorSpecific(ExecutionRequest request, List<string> errors)
    {
        switch (request.ExecutorType.Value)
        {
            case "http":
                ValidateHttpExecutor(request, errors);
                break;
            case "powershell":
                ValidatePowerShellExecutor(request, errors);
                break;
        }
    }

    private static void ValidateHttpExecutor(ExecutionRequest request, List<string> errors)
    {
        if (request.Method == "GET" && !string.IsNullOrEmpty(request.Body))
        {
            errors.Add("GET requests should not have a body");
        }
    }

    private static void ValidatePowerShellExecutor(ExecutionRequest request, List<string> errors)
    {
        if (request.Method != "POST")
        {
            errors.Add("PowerShell executor only supports POST method");
        }

        if (string.IsNullOrEmpty(request.Body))
        {
            errors.Add("PowerShell executor requires a body with command information");
        }
    }
}