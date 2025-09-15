using System.ComponentModel.DataAnnotations;

namespace ExecutionRouter.Application.Configuration;

/// <summary>
/// PowerShell executor configuration
/// </summary>
public class PowerShellExecutorSettings
{
    /// <summary>
    /// Enable PowerShell executor
    /// </summary>
    public bool Enabled { get; init; } = true;

    /// <summary>
    /// Maximum PowerShell execution timeout in seconds
    /// </summary>
    [Range(1, 300)]
    public int MaxExecutionTimeoutSeconds { get; init; } = 60;

    /// <summary>
    /// Allowed PowerShell cmdlets
    /// </summary>
    [Required]
    public IEnumerable<string> AllowedCmdlets { get; init; } =
    [
        "Get-Date",
        "Get-Process",
        "Get-Service",
        "Test-Connection",
        "Get-ChildItem",
        "Get-Content",
        "Write-Output",
        "Write-Host"
    ];

    /// <summary>
    /// Blocked PowerShell cmdlets (blacklist)
    /// </summary>
    [Required]
    public IEnumerable<string> BlockedCmdlets { get; init; } =
    [
        "Remove-Item",
        "Stop-Process",
        "Stop-Service",
        "Set-ExecutionPolicy",
        "Invoke-Expression",
        "Invoke-Command",
        "Start-Process"
    ];

    /// <summary>
    /// Enable Exchange Online module
    /// </summary>
    public bool EnableExchangeOnline { get; init; } = false;

    /// <summary>
    /// PowerShell execution policy
    /// </summary>
    public string ExecutionPolicy { get; init; } = "RemoteSigned";
}