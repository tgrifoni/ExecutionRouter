using ExecutionRouter.Domain.ValueObjects;

namespace ExecutionRouter.Domain.Entities;

/// <summary>
/// Represents the result of a remote execution
/// </summary>
public sealed class ExecutionResponse
{
    public RequestId RequestId { get; private set; }
    public CorrelationId? CorrelationId { get; private set; }
    public ExecutorType ExecutorType { get; private set; }
    public DateTime StartTime { get; private set; }
    public DateTime EndTime { get; private set; }
    public TimeSpan Duration => EndTime - StartTime;
    public ExecutionStatus Status { get; private set; }
    public IEnumerable<AttemptSummary> AttemptSummaries { get; private set; }
    public ExecutorResult Result { get; private set; }
    public string? ErrorMessage { get; private set; }

    private ExecutionResponse(
        RequestId requestId,
        CorrelationId? correlationId,
        ExecutorType executorType,
        DateTime startTime,
        DateTime endTime,
        ExecutionStatus status,
        IEnumerable<AttemptSummary> attemptSummaries,
        ExecutorResult result,
        string? errorMessage = null)
    {
        RequestId = requestId;
        CorrelationId = correlationId;
        ExecutorType = executorType;
        StartTime = startTime;
        EndTime = endTime;
        Status = status;
        AttemptSummaries = attemptSummaries;
        Result = result;
        ErrorMessage = errorMessage;
    }

    public static ExecutionResponse Success(
        RequestId requestId,
        CorrelationId? correlationId,
        ExecutorType executorType,
        DateTime startTime,
        DateTime endTime,
        IEnumerable<AttemptSummary> attemptSummaries,
        ExecutorResult result) => new
        (
            requestId,
            correlationId,
            executorType,
            startTime,
            endTime,
            ExecutionStatus.Success,
            attemptSummaries,
            result
        );

    public static ExecutionResponse Failure(
        RequestId requestId,
        CorrelationId? correlationId,
        ExecutorType executorType,
        DateTime startTime,
        DateTime endTime,
        IEnumerable<AttemptSummary> attemptSummaries,
        string errorMessage) => new
        (
            requestId,
            correlationId,
            executorType,
            startTime,
            endTime,
            ExecutionStatus.Failed,
            attemptSummaries,
            ExecutorResult.Empty,
            errorMessage
        );
}