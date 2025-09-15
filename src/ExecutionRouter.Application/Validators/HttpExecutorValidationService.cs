using System.Text.RegularExpressions;
using ExecutionRouter.Application.Configuration;
using Microsoft.Extensions.Logging;

namespace ExecutionRouter.Application.Validators;

public class HttpExecutorValidationService(ILogger<HttpExecutorValidationService> logger)
    : IExecutorValidationService<HttpExecutorSettings>
{
    public bool IsTargetAllowed(string? targetUrl, HttpExecutorSettings settings)
    {
        if (string.IsNullOrWhiteSpace(targetUrl))
        {
            return false;
        }

        try
        {
            foreach (var blockedPattern in settings.BlockedTargetPatterns)
            {
                if (!Regex.IsMatch(targetUrl, blockedPattern, RegexOptions.IgnoreCase))
                {
                    continue;
                }
                
                logger.LogWarning("Target URL {TargetUrl} blocked by pattern {Pattern}", targetUrl, blockedPattern);
                return false;
            }

            if (settings.AllowedTargetPatterns.Any(allowedPattern => Regex.IsMatch(targetUrl, allowedPattern, RegexOptions.IgnoreCase)))
            {
                return true;
            }

            logger.LogWarning("Target URL {TargetUrl} not matching any allowed patterns", targetUrl);
            return false;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error validating target URL {TargetUrl}", targetUrl);
            return false;
        }
    }
}