using System.Text;
using ExecutionRouter.Application.Configuration;
using ExecutionRouter.Domain.Constants;
using ExecutionRouter.Domain.Entities;
using ExecutionRouter.Domain.Interfaces;
using Microsoft.Extensions.Options;

namespace ExecutionRouter.Application.Validators;

/// <summary>
/// Service for validating execution requests
/// </summary>
public sealed class ValidationService(IOptions<SecuritySettings> securityOptions) : IValidationService
{
    private readonly SecuritySettings _securitySettings = securityOptions.Value;

    public ValidationResult ValidateRequest(ExecutionRequest request)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(request.Method))
        {
            errors.Add("Method is required");
        }
        else if (!_securitySettings.AllowedMethods.Contains(request.Method, StringComparer.OrdinalIgnoreCase))
        {
            errors.Add($"Unsupported HTTP method: {request.Method}");
        }

        if (string.IsNullOrWhiteSpace(request.Path))
        {
            errors.Add("Path is required");
        }
        else if (request.Path.Length > _securitySettings.MaxPathLength)
        {
            errors.Add($"Path too long. Maximum length is {_securitySettings.MaxPathLength} characters");
        }

        if (!string.IsNullOrEmpty(request.Body))
        {
            var bodyBytes = Encoding.UTF8.GetByteCount(request.Body);
            if (bodyBytes > _securitySettings.MaxRequestBodySizeBytes)
            {
                errors.Add($"Request body too large. Maximum size is {_securitySettings.MaxRequestBodySizeBytes} bytes");
            }
        }

        foreach (var header in request.Headers)
        {
            if (string.IsNullOrWhiteSpace(header.Key))
            {
                errors.Add("Header name cannot be empty");
                continue;
            }

            if (_securitySettings.BlockedHeaders.Contains(header.Key, StringComparer.OrdinalIgnoreCase))
            {
                errors.Add($"Header '{header.Key}' is not allowed");
                continue;
            }

            if (header.Value.Length > _securitySettings.MaxHeaderValueLength)
            {
                errors.Add($"Header '{header.Key}' value too long. Maximum length is {_securitySettings.MaxHeaderValueLength} characters");
            }
        }

        if (request.TimeoutSeconds <= TimeSpan.Zero)
        {
            errors.Add("Timeout must be greater than zero");
        }
        else if (request.TimeoutSeconds > TimeSpan.FromSeconds(_securitySettings.MaxTimeoutSeconds))
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
            case ExecutorTypes.Http:
                ValidateHttpExecutor(request, errors);
                break;
            case ExecutorTypes.PowerShell:
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