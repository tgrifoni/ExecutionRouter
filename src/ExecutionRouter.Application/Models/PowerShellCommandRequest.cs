namespace ExecutionRouter.Application.Models;

/// <summary>
/// Request model for PowerShell commands
/// </summary>
public sealed record PowerShellCommandRequest
{
    public string Command { get; init; } = string.Empty;
    public string? Filter { get; init; }
    public int? MaxResults { get; init; }
    public Dictionary<string, object>? Parameters { get; init; }
}