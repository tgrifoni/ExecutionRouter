namespace ExecutionRouter.Domain.Constants;

/// <summary>
/// Contains all executor type constant values used throughout the ExecutionRouter application
/// </summary>
public static class ExecutorTypes
{
    /// <summary>
    /// HTTP executor type identifier
    /// </summary>
    public const string Http = "http";
    
    /// <summary>
    /// PowerShell executor type identifier
    /// </summary>
    public const string PowerShell = "powershell";
}