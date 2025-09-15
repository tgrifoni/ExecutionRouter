namespace ExecutionRouter.Domain.Constants;

/// <summary>
/// Contains all constant values used throughout the ExecutionRouter application
/// </summary>
public static class Headers
{
    /// <summary>
    /// Custom HTTP headers used by ExecutionRouter
    /// </summary>
    public static class ExecutionRouter
    {
        /// <summary>
        /// Header containing the unique request ID
        /// </summary>
        public const string RequestId = "X-ExecutionRouter-RequestId";
        
        /// <summary>
        /// Header containing the correlation ID for request tracing
        /// </summary>
        public const string CorrelationId = "X-ExecutionRouter-CorrelationId";
        
        /// <summary>
        /// Header containing the instance/machine name that processed the request
        /// </summary>
        public const string Instance = "X-ExecutionRouter-Instance";
        
        /// <summary>
        /// Header containing the number of retry attempts made
        /// </summary>
        public const string AttemptCount = "X-ExecutionRouter-AttemptCount";
        
        /// <summary>
        /// Header containing the total execution duration
        /// </summary>
        public const string Duration = "X-ExecutionRouter-Duration";
    }
    
    /// <summary>
    /// Standard HTTP headers
    /// </summary>
    public static class Standard
    {
        /// <summary>
        /// Content-Type header
        /// </summary>
        public const string ContentType = "content-type";
    }
}