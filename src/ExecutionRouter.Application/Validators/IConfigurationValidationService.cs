using ExecutionRouter.Application.Configuration;
using ExecutionRouter.Application.Models;

namespace ExecutionRouter.Application.Validators;

/// <summary>
/// Service for validating configuration settings
/// </summary>
public interface IConfigurationValidationService
{
    /// <summary>
    /// Validate the execution configuration
    /// </summary>
    ConfigurationValidationResult ValidateConfiguration(ExecutionConfiguration configuration);
}