using System.Text.Json;
using ExecutionRouter.Application.Models;
using ExecutionRouter.Domain.Entities;
using ExecutionRouter.Domain.Interfaces;
using ExecutionRouter.Domain.ValueObjects;
using ExecutionRouter.Domain.Exceptions;

namespace ExecutionRouter.Infrastructure.Executors;

/// <summary>
/// PowerShell executor for running Exchange Online and directory commands
/// Simplified implementation for demonstration purposes
/// </summary>
public sealed class PowerShellExecutor(ISystemClock systemClock) : IExecutor, IDisposable
{
    public ExecutorType ExecutorType => ExecutorType.PowerShell;

    private readonly HashSet<string> _allowedCommands = new(StringComparer.OrdinalIgnoreCase)
    {
        "Get-Mailbox",
        "Get-User", 
        "Get-Group",
        "Get-DistributionGroup",
        "Get-UnifiedGroup",
        "Get-Recipient",
        "Get-OrganizationConfig",
        "Get-AcceptedDomain",
        "Get-Transport*",
        "Get-Compliance*"
    };

    private readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };
    
    private readonly SemaphoreSlim _sessionSemaphore = new(1, 1);
    private bool _disposed;

    public async Task<ExecutorResult> ExecuteAsync(ExecutionRequest request, CancellationToken cancellationToken = default)
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(nameof(PowerShellExecutor));
        }

        try
        {
            await _sessionSemaphore.WaitAsync(cancellationToken);
            
            var commandRequest = ParseCommandRequest(request.Body);
            ValidateCommand(commandRequest.Command);
            
            await EnsureSessionAsync(cancellationToken);
            
            var result = await ExecuteCommandAsync(commandRequest, request.TimeoutSeconds, cancellationToken);
            
            return result;
        }
        finally
        {
            _sessionSemaphore.Release();
        }
    }

    private PowerShellCommandRequest ParseCommandRequest(string? body)
    {
        if (string.IsNullOrEmpty(body))
        {
            throw new ValidationException(["PowerShell executor requires a command in the request body"]);
        }

        try
        {
            var request = JsonSerializer.Deserialize<PowerShellCommandRequest>(body, _jsonSerializerOptions);

            return request ?? throw new ValidationException(["Invalid command request format"]);
        }
        catch (JsonException ex)
        {
            throw new ValidationException([$"Failed to parse command request: {ex.Message}"]);
        }
    }

    private void ValidateCommand(string command)
    {
        if (string.IsNullOrWhiteSpace(command))
        {
            throw new ValidationException(["Command cannot be empty"]);
        }

        var commandName = command.Split(' ', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
        if (string.IsNullOrEmpty(commandName))
        {
            throw new ValidationException(["Invalid command format"]);
        }

        var isAllowed = _allowedCommands.Any(allowed =>
        {
            if (!allowed.EndsWith("*"))
            {
                return string.Equals(commandName, allowed, StringComparison.OrdinalIgnoreCase);
            }
            
            var prefix = allowed[..^1];
            return commandName.StartsWith(prefix, StringComparison.OrdinalIgnoreCase);
        });

        if (!isAllowed)
        {
            throw new ValidationException(errors:
            [
                $"Command '{commandName}' is not allowed. Allowed commands: {string.Join(", ", _allowedCommands)}"
            ]);
        }

        if (ContainsDangerousPatterns(command))
        {
            throw new ValidationException(["Command contains potentially dangerous patterns"]);
        }
    }

    private static bool ContainsDangerousPatterns(string command)
    {
        var dangerousPatterns = new[]
        {
            "invoke-expression",
            "iex",
            "invoke-command",
            "icm",
            "start-process",
            "saps",
            "new-object",
            "add-type",
            "remove-",
            "delete-",
            "clear-",
            "set-executionpolicy",
            "download",
            "upload",
            "webclient",
            "invoke-webrequest",
            "invoke-restmethod",
            "|",
            "&",
            ";",
            "&&",
            "||"
        };

        return dangerousPatterns.Any(pattern => command.Contains(pattern, StringComparison.OrdinalIgnoreCase));
    }

    private static async Task EnsureSessionAsync(CancellationToken cancellationToken)
    {
        await Task.Delay(100, cancellationToken);
    }

    private async Task<ExecutorResult> ExecuteCommandAsync(
        PowerShellCommandRequest commandRequest, 
        TimeSpan timeout, 
        CancellationToken cancellationToken)
    {
        try
        {
            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeoutCts.CancelAfter(timeout);

            await Task.Delay(200, timeoutCts.Token);
            
            var mockResult = GenerateMockResult(commandRequest.Command);
            
            return ExecutorResult.PowerShell(
                commandRequest.Command,
                mockResult.Stdout,
                mockResult.Stderr,
                mockResult.Result);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw new ExecutionFailedException("PowerShell execution was cancelled", isTransient: false);
        }
        catch (OperationCanceledException)
        {
            throw new ExecutionTimeoutException(timeout);
        }
        catch (Exception ex)
        {
            var isTransient = IsTransientPowerShellError(ex);
            throw new ExecutionFailedException($"PowerShell execution failed: {ex.Message}", ex, isTransient);
        }
    }

    private (string Stdout, string Stderr, object Result) GenerateMockResult(string command)
    {
        var commandName = command.Split(' ').FirstOrDefault()?.ToLowerInvariant();
        
        return commandName switch
        {
            "get-mailbox" => (
                "Name: John Doe, EmailAddress: john.doe@contoso.com\nName: Jane Smith, EmailAddress: jane.smith@contoso.com",
                string.Empty,
                new List<object>
                {
                    new { Name = "John Doe", EmailAddress = "john.doe@contoso.com", RecipientType = "UserMailbox" },
                    new { Name = "Jane Smith", EmailAddress = "jane.smith@contoso.com", RecipientType = "UserMailbox" }
                }
            ),
            "get-user" => (
                "DisplayName: John Doe, UserPrincipalName: john.doe@contoso.com",
                string.Empty,
                new { DisplayName = "John Doe", UserPrincipalName = "john.doe@contoso.com", Department = "IT" }
            ),
            "get-group" => (
                "Name: IT Group, Description: Information Technology Team",
                string.Empty,
                new { Name = "IT Group", Description = "Information Technology Team", GroupType = "Security" }
            ),
            _ => (
                $"Command '{command}' executed successfully (simulated)",
                string.Empty,
                new { Command = command, Status = "Success", Timestamp = systemClock.UtcNow }
            )
        };
    }

    private static bool IsTransientPowerShellError(Exception ex)
    {
        var message = ex.Message.ToLowerInvariant();
        
        return message.Contains("timeout") ||
               message.Contains("network") ||
               message.Contains("connection") ||
               message.Contains("throttl") ||
               message.Contains("temporary") ||
               message.Contains("service unavailable");
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }
        
        _sessionSemaphore.Dispose();
        _disposed = true;
    }
}