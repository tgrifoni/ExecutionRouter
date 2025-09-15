using System.ComponentModel.DataAnnotations;
using ExecutionRouter.Application.Configuration;
using ExecutionRouter.Application.Models;

namespace ExecutionRouter.Application.Validators;

/// <summary>
/// Configuration validation service implementation
/// </summary>
public class ConfigurationValidationService : IConfigurationValidationService
{
    public ConfigurationValidationResult ValidateConfiguration(ExecutionConfiguration configuration)
    {
        var validationErrors = new List<string>();

        var securityValidation = ValidateSecuritySettings(configuration.Security);
        if (!securityValidation.IsValid)
        {
            validationErrors.AddRange(securityValidation.Errors);
        }

        var httpValidation = ValidateHttpExecutorSettings(configuration.HttpExecutor);
        if (!httpValidation.IsValid)
        {
            validationErrors.AddRange(httpValidation.Errors);
        }

        var psValidation = ValidatePowerShellSettings(configuration.PowerShellExecutor);
        if (!psValidation.IsValid)
        {
            validationErrors.AddRange(psValidation.Errors);
        }

        var resilienceValidation = ValidateResilienceSettings(configuration.Resilience);
        if (!resilienceValidation.IsValid)
        {
            validationErrors.AddRange(resilienceValidation.Errors);
        }

        var observabilityValidation = ValidateObservabilitySettings(configuration.Observability);
        if (!observabilityValidation.IsValid)
        {
            validationErrors.AddRange(observabilityValidation.Errors);
        }

        return new ConfigurationValidationResult(validationErrors);
    }

    private static ConfigurationValidationResult ValidateSecuritySettings(SecuritySettings settings)
    {
        var errors = ValidateDataAnnotations(settings, "Security");

        return new ConfigurationValidationResult(errors);
    }

    private static ConfigurationValidationResult ValidateHttpExecutorSettings(HttpExecutorSettings settings)
    {
        var errors = ValidateDataAnnotations(settings, "HttpExecutor");

        return new ConfigurationValidationResult(errors);
    }

    private static ConfigurationValidationResult ValidatePowerShellSettings(PowerShellExecutorSettings settings)
    {
        var errors = ValidateDataAnnotations(settings, "PowerShellExecutor");

        return new ConfigurationValidationResult(errors);
    }

    private static ConfigurationValidationResult ValidateResilienceSettings(ResilienceSettings settings)
    {
        var errors = ValidateDataAnnotations(settings, "Resilience");

        if (settings.BaseDelayMilliseconds >= settings.MaxDelayMilliseconds)
        {
            errors.Add("Resilience.BaseDelayMilliseconds must be less than MaxDelayMilliseconds");
        }

        return new ConfigurationValidationResult(errors);
    }

    private static ConfigurationValidationResult ValidateObservabilitySettings(ObservabilitySettings settings)
    {
        var errors = ValidateDataAnnotations(settings, "Observability");

        return new ConfigurationValidationResult(errors);
    }

    private static List<string> ValidateDataAnnotations<T>(T settings, string prefix)
    {
        if (settings is null)
        {
            throw new ArgumentNullException(nameof(settings));
        }
        
        var validationContext = new ValidationContext(settings);
        var validationResults = new List<ValidationResult>();

        if (Validator.TryValidateObject(settings, validationContext, validationResults, true))
        {
            return [];
        }

        var validationErrors = validationResults
            .Select(validationResult => new
            {
                validationResult,
                memberNames =
                    validationResult.MemberNames.Any()
                        ? string.Join(", ", validationResult.MemberNames.Select(m => $"{prefix}.{m}"))
                        : prefix
            })
            .Select(t => $"{t.memberNames}: {t.validationResult.ErrorMessage}")
            .ToList();
        return validationErrors;
    }
}