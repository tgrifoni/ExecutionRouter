namespace ExecutionRouter.Application.Validators;

public interface IExecutorValidationService<in TExecutorSettings>
{
    /// <summary>
    /// Validate if a target is allowed
    /// </summary>
    bool IsTargetAllowed(string? target, TExecutorSettings settings);
}