using ExecutionRouter.Domain.Entities;
using ExecutionRouter.Domain.Interfaces;

namespace ExecutionRouter.Application.Services;

/// <summary>
/// Main orchestration service for executing requests
/// </summary>
public sealed class ExecutionOrchestrationService
{
    private readonly Dictionary<string, IExecutor> _executors;
    private readonly IResiliencePolicy _resiliencePolicy;
    private readonly IValidationService _validationService;
    private readonly IMetricsCollector _metricsCollector;
    private readonly ISystemClock _systemClock;

    public ExecutionOrchestrationService(
        IEnumerable<IExecutor> executors,
        IResiliencePolicy resiliencePolicy,
        IValidationService validationService,
        IMetricsCollector metricsCollector,
        ISystemClock systemClock)
    {
        _executors = executors.ToDictionary(e => e.ExecutorType.Value, e => e);
        _resiliencePolicy = resiliencePolicy;
        _validationService = validationService;
        _metricsCollector = metricsCollector;
        _systemClock = systemClock;
    }

    /// <summary>
    /// Executes a request with full orchestration (validation, resilience, metrics)
    /// </summary>
    public async Task<ExecutionResponse> ExecuteAsync(ExecutionRequest request, CancellationToken cancellationToken = default)
    {
        var startTime = _systemClock.UtcNow;
        _metricsCollector.RecordRequest();

        try
        {
            var validationResult = _validationService.ValidateRequest(request);
            if (!validationResult.IsValid)
            {
                var endTime = _systemClock.UtcNow;
                _metricsCollector.RecordFailure(isTransient: false);
                _metricsCollector.RecordLatency(endTime - startTime);
                
                return ExecutionResponse.Failure(
                    request.RequestId,
                    request.CorrelationId,
                    request.ExecutorType,
                    startTime,
                    endTime,
                    attemptSummaries: [],
                    errorMessage: $"Validation failed: {string.Join(", ", validationResult.Errors)}");
            }

            if (!_executors.TryGetValue(request.ExecutorType.Value, out var executor))
            {
                var endTime = _systemClock.UtcNow;
                _metricsCollector.RecordFailure(isTransient: false);
                _metricsCollector.RecordLatency(endTime - startTime);
                
                return ExecutionResponse.Failure(
                    request.RequestId,
                    request.CorrelationId,
                    request.ExecutorType,
                    startTime,
                    endTime,
                    attemptSummaries: [],
                    errorMessage: $"No executor found for type: {request.ExecutorType}");
            }

            var policyContext = new PolicyExecutionContext(request.RequestId, request.ExecutorType, request.TimeoutSeconds);
            var policyResult = await _resiliencePolicy.ExecuteAsync(
                ct => executor.ExecuteAsync(request, ct),
                policyContext,
                cancellationToken);

            var finalEndTime = _systemClock.UtcNow;
            _metricsCollector.RecordLatency(finalEndTime - startTime);

            if (policyResult.IsSuccess)
            {
                _metricsCollector.RecordSuccess();
                return ExecutionResponse.Success(
                    request.RequestId,
                    request.CorrelationId,
                    request.ExecutorType,
                    startTime,
                    finalEndTime,
                    policyResult.AttemptSummaries,
                    policyResult.Result);
            }
            
            _metricsCollector.RecordFailure(isTransient: false);
            return ExecutionResponse.Failure(
                request.RequestId,
                request.CorrelationId,
                request.ExecutorType,
                startTime,
                finalEndTime,
                policyResult.AttemptSummaries,
                policyResult.ErrorMessage ?? "Execution failed");
        }
        catch (Exception ex)
        {
            var endTime = _systemClock.UtcNow;
            _metricsCollector.RecordFailure(isTransient: false);
            _metricsCollector.RecordLatency(endTime - startTime);
            
            return ExecutionResponse.Failure(
                request.RequestId,
                request.CorrelationId,
                request.ExecutorType,
                startTime,
                endTime,
                attemptSummaries: [],
                errorMessage: $"Unexpected error: {ex.Message}");
        }
    }
}