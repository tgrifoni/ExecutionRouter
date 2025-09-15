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
        /// Header containing the executor type
        /// </summary>
        public const string ExecutorType = "X-ExecutionRouter-ExecutorType";
        
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
        /// Accept header
        /// </summary>
        public const string Accept = "accept";
        
        /// <summary>
        /// Accept-Encoding header
        /// </summary>
        public const string AcceptEncoding = "accept-encoding";
        
        /// <summary>
        /// Accept-Language header
        /// </summary>
        public const string AcceptLanguage = "accept-language";
        
        /// <summary>
        /// Authorization header
        /// </summary>
        public const string Authorization = "authorization";
        
        /// <summary>
        /// Cache-Control header
        /// </summary>
        public const string CacheControl = "cache-control";
        
        /// <summary>
        /// Connection header
        /// </summary>
        public const string Connection = "connection";
        
        /// <summary>
        /// Cookie header
        /// </summary>
        public const string Cookie = "cookie";
        
        /// <summary>
        /// Set-Cookie header
        /// </summary>
        public const string SetCookie = "set-cookie";
        
        /// <summary>
        /// Content-Length header
        /// </summary>
        public const string ContentLength = "content-length";
        
        /// <summary>
        /// Content-Type header
        /// </summary>
        public const string ContentType = "content-type";
        
        /// <summary>
        /// Host header
        /// </summary>
        public const string Host = "host";
        
        /// <summary>
        /// If-Match header
        /// </summary>
        public const string IfMatch = "if-match";
        
        /// <summary>
        /// If-Modified-Since header
        /// </summary>
        public const string IfModifiedSince = "if-modified-since";
        
        /// <summary>
        /// If-None-Match header
        /// </summary>
        public const string IfNoneMatch = "if-none-match";
        
        /// <summary>
        /// If-Unmodified-Since header
        /// </summary>
        public const string IfUnmodifiedSince = "if-unmodified-since";
        
        /// <summary>
        /// Proxy-Authorization header
        /// </summary>
        public const string ProxyAuthorization = "proxy-authorization";
        
        /// <summary>
        /// Transfer-Encoding header
        /// </summary>
        public const string TransferEncoding = "transfer-encoding";
        
        /// <summary>
        /// Upgrade header
        /// </summary>
        public const string Upgrade = "upgrade";
        
        /// <summary>
        /// User-Agent header
        /// </summary>
        public const string UserAgent = "User-Agent";
    }
    
    /// <summary>
    /// Common X-* and custom headers
    /// </summary>
    public static class Extended
    {
        /// <summary>
        /// X-Request-Id header (standard request ID)
        /// </summary>
        public const string XRequestId = "X-Request-Id";
        
        /// <summary>
        /// X-Correlation-Id header (standard correlation ID)
        /// </summary>
        public const string XCorrelationId = "X-Correlation-Id";
        
        /// <summary>
        /// X-Request-Timeout header
        /// </summary>
        public const string XRequestTimeout = "X-Request-Timeout";
        
        /// <summary>
        /// X-Requested-With header
        /// </summary>
        public const string XRequestedWith = "x-requested-with";
        
        /// <summary>
        /// X-Forwarded-Authorization header
        /// </summary>
        public const string XForwardedAuthorization = "x-forwarded-authorization";
        
        /// <summary>
        /// X-Forwarded-For header
        /// </summary>
        public const string XForwardedFor = "x-forwarded-for";
        
        /// <summary>
        /// X-Real-IP header
        /// </summary>
        public const string XRealIp = "x-real-ip";
        
        /// <summary>
        /// X-API-Key header
        /// </summary>
        public const string XApiKey = "x-api-key";
        
        /// <summary>
        /// X-Auth-Token header
        /// </summary>
        public const string XAuthToken = "x-auth-token";
    }
}