using ExecutionRouter.Domain.Entities;

namespace ExecutionRouter.Domain.Interfaces;

/// <summary>
/// Defines the contract for validation services
/// </summary>
public interface IValidationService
{
    /// <summary>
    /// Validates an execution request
    /// </summary>
    /// <param name="request">The request to validate</param>
    /// <returns>Validation result</returns>
    ValidationResult ValidateRequest(ExecutionRequest request);
}