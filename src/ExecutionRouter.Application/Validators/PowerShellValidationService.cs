using ExecutionRouter.Application.Configuration;
using Microsoft.Extensions.Logging;

namespace ExecutionRouter.Application.Validators;

public class PowerShellExecutorValidationService(ILogger<PowerShellExecutorValidationService> logger)
    : IExecutorValidationService<PowerShellExecutorSettings>
{
    public bool IsTargetAllowed(string? cmdlet, PowerShellExecutorSettings settings)
    {
        if (string.IsNullOrWhiteSpace(cmdlet))
        {
            return false;
        }

        var cmdletName = cmdlet.Split(' ', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault()?.Trim();
        if (string.IsNullOrWhiteSpace(cmdletName))
        {
            return false;
        }

        if (settings.BlockedCmdlets.Any(blocked => string.Equals(blocked, cmdletName, StringComparison.OrdinalIgnoreCase)))
        {
            logger.LogWarning("PowerShell cmdlet {Cmdlet} is blocked", cmdletName);
            return false;
        }

        if (settings.AllowedCmdlets.Any(allowed => string.Equals(allowed, cmdletName, StringComparison.OrdinalIgnoreCase)))
        {
            return true;
        }
        
        logger.LogWarning("PowerShell cmdlet {Cmdlet} is not in the allowed list", cmdletName);
        return false;

    }
}