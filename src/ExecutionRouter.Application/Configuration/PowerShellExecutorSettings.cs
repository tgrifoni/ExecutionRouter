using System.ComponentModel.DataAnnotations;

namespace ExecutionRouter.Application.Configuration;

/// <summary>
/// PowerShell executor configuration
/// </summary>
public class PowerShellExecutorSettings
{
    public const string SectionName = "ExecutionRouter:PowerShellExecutor";
    
    /// <summary>
    /// Enable PowerShell executor
    /// </summary>
    [Required]
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
    public HashSet<string> AllowedCmdlets { get; init; } = 
    [
        "Get-ChildItem",
        "Get-Content",
        "Get-Location",
        "Get-Date",
        "Get-History",
        "Get-Process",
        "Get-Service",
        "Test-Connection",
        "Write-Host",
        "Write-Output"
    ];

    /// <summary>
    /// Blocked PowerShell cmdlets (blacklist)
    /// </summary>
    [Required]
    public HashSet<string> BlockedCmdlets { get; init; } = 
    [
        "Remove-Item",
        "Stop-Process",
        "Stop-Service",
        "Set-ExecutionPolicy",
        "Invoke-Expression",
        "Invoke-Command",
        "Start-Process",
        "New-Item",
        "Set-Content",
        "Clear-Content",
        "Remove-Variable",
        "Set-Variable"
    ];

    /// <summary>
    /// Enable Exchange Online module
    /// </summary>
    [Required]
    public bool EnableExchangeOnline { get; init; } = false;

    /// <summary>
    /// PowerShell execution policy
    /// </summary>
    public string ExecutionPolicy { get; init; } = "RemoteSigned";
}