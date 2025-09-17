# ExecutionRouter

A production-grade, extensible remote request execution service that routes client requests to different execution backends (HTTP and PowerShell) with built-in resilience, observability, and security features.

## Architecture Overview

```
                            ExecutionRouter System Architecture
    
    ┌─────────────────────────────────────────────────────────────────────────────┐
    │                                 API Layer                                   │
    └─────────────────────────────────────────────────────────────────────────────┘
              │                        │                        │
              ▼                        ▼                        ▼
    ┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
    │ ExecutionCtrl   │    │  HealthCtrl     │    │   Middleware    │
    │ /api/{**path}   │    │ /health,/metrics│    │ Logging,Errors  │
    └─────────────────┘    └─────────────────┘    └─────────────────┘
              │                                             │
              ▼                                             ▼
    ┌─────────────────────────────────────────────────────────────────────────────┐
    │                        Application Services                                 │
    │  ┌─────────────────┐  ┌─────────────────┐  ┌─────────────────┐              │
    │  │ Orchestration   │  │   Validation    │  │   Metrics       │              │
    │  │    Service      │  │    Service      │  │  Collection     │              │
    │  └─────────────────┘  └─────────────────┘  └─────────────────┘              │
    └─────────────────────────────────────────────────────────────────────────────┘
              │                        │
              ▼                        ▼
    ┌─────────────────────────────────────────────────────────────────────────────┐
    │                            Infrastructure                                   │
    │                                                                             │
    │  ┌─────────────────────────────────────────────────────────────────────┐    │
    │  │                        Resilience Layer                             │    │
    │  │  ┌─────────────────┐  ┌─────────────────┐  ┌─────────────────┐      │    │
    │  │  │  Retry Policy   │  │ Timeout Policy  │  │ Circuit Breaker │      │    │
    │  │  │ Exp. Backoff    │  │ Per-Request     │  │   (Future)      │      │    │
    │  │  │   + Jitter      │  │   Timeouts      │  │                 │      │    │
    │  │  └─────────────────┘  └─────────────────┘  └─────────────────┘      │    │
    │  └─────────────────────────────────────────────────────────────────────┘    │
    │                                  │                                          │
    │                                  ▼                                          │
    │  ┌─────────────────────────────────────────────────────────────────────┐    │
    │  │                         Executors                                   │    │
    │  │                                                                     │    │
    │  │   ┌─────────────────┐              ┌─────────────────┐              │    │
    │  │   │  HTTP Executor  │              │   PowerShell    │              │    │
    │  │   │                 │              │    Executor     │              │    │
    │  │   │ • Forwards HTTP │              │ • Exchange      │              │    │
    │  │   │   requests      │              │   Online        │              │    │
    │  │   │ • Header filter │              │ • Command       │              │    │
    │  │   │ • Query merge   │              │   allowlist     │              │    │
    │  │   │                 │              │ • Session mgmt  │              │    │
    │  │   └─────────────────┘              └─────────────────┘              │    │
    │  │            │                                │                       │    │
    │  │            ▼                                ▼                       │    │
    │  │   ┌─────────────────┐              ┌─────────────────┐              │    │
    │  │   │  External APIs  │              │   PowerShell    │              │    │
    │  │   │  Web Services   │              │    Runspaces    │              │    │
    │  │   └─────────────────┘              └─────────────────┘              │    │
    │  └─────────────────────────────────────────────────────────────────────┘    │
    └─────────────────────────────────────────────────────────────────────────────┘
                                       │
                                       ▼
    ┌─────────────────────────────────────────────────────────────────────────────┐
    │                         Observability                                       │
    │  ┌─────────────────┐  ┌─────────────────┐  ┌─────────────────┐              │
    │  │ Structured      │  │     Metrics     │  │   Distributed   │              │
    │  │   Logging       │  │   Collection    │  │    Tracing      │              │
    │  │ (JSON Format)   │  │   (In-Memory)   │  │   (Headers)     │              │
    │  └─────────────────┘  └─────────────────┘  └─────────────────┘              │
    └─────────────────────────────────────────────────────────────────────────────┘
    
    Request Flow: Client → API → Orchestration → Resilience → Executor → Target
    Response Flow: Target → Executor → Resilience → Orchestration → API → Client
```

### Core Components

- **API Layer**: Catch-all routing (`/api/{**path}`) for all HTTP verbs
- **Orchestration Service**: Request validation, executor selection, and resilience policies
- **Executors**: Pluggable execution backends (HTTP, PowerShell)
- **Resilience**: Polly-based retry policies with exponential backoff and jitter
- **Observability**: Structured logging, metrics collection, and traceability

## Design Decisions & Trade-offs

### Architecture Pattern
**Choice**: Clean Architecture with Domain-Driven Design

**Rationale**: Separation of concerns, testability, and extensibility for future executors

**Trade-off**: Increased complexity vs. maintainability

### Retry Strategy
**Backoff Formula**: `baseDelay * (2^attemptNumber) + jitter`
- Base delay: 1000ms (configurable)
- Max delay: 30000ms (configurable)  
- Jitter: ±25% randomization to prevent thundering herd

**Transient Classification**: Network timeouts, 5XX responses, specific exceptions

**Non-transient**: 4XX client errors, validation failures, authentication errors

### Executor Selection
**Choice**: Header-based (`X-ExecutionRouter-ExecutorType`) with query parameter fallback

**Rationale**: Explicit control while maintaining URL-based routing compatibility

**Trade-off**: Client complexity vs. routing flexibility

### State Management
**Choice**: Stateless per-request processing with in-memory metrics

**Rationale**: Simplicity, scalability, and cloud-native compatibility

**Trade-off**: No persistent session reuse vs. operational simplicity

## Resilience Strategy

### Retry Classification
- **Transient Failures**: 
  - Network timeouts (`TaskCancelledException`, `TimeoutException`)
  - HTTP 5XX responses (server errors)
  - Specific PowerShell connection failures
  - DNS resolution failures

- **Non-transient Failures**:
  - HTTP 4XX responses (client errors)  
  - Validation errors (`ValidationException`)
  - Authentication/authorization failures
  - Malformed requests

### Backoff Implementation
```csharp
delay = Math.Min(
    baseDelayMs * Math.Pow(2, attemptNumber), 
    maxDelayMs
) + jitter(-25% to +25%)
```

### Circuit Breaker Pattern
Ready for implementation via Polly's circuit breaker with configurable failure thresholds and recovery periods.

## Security Considerations

### Implemented
- Input size limits (configurable, default 10MB)
- Request timeout limits (configurable, default 120s)
- Non-root container execution (UID 1001)
- Header filtering to prevent header injection
- PowerShell command allowlisting (Exchange Online cmdlets only)

### Current Limitations
- No authentication/authorization layer
- Secrets may appear in logs (basic masking implemented)
- No rate limiting (extension point available)
- PowerShell sessions not encrypted (uses system defaults)

### Recommendations
- Implement OAuth2/JWT authentication
- Add API key management
- Deploy behind reverse proxy with TLS termination
- Implement comprehensive audit logging

## Testing Approach

### Current Status
**Tests are currently in progress** - comprehensive test suite being developed.

### Planned Coverage
- **Unit Tests**: Validation logic, retry policies, executor selection, response envelope construction
- **Integration Tests**: End-to-end HTTP execution with in-memory test servers
- **Contract Tests**: Request/response schema validation
- **Resilience Tests**: Timeout handling, retry scenarios, failure classification

### Notable Gaps (To Be Addressed)
- PowerShell executor integration tests (requires mock PS sessions)
- Performance/load testing scenarios
- Chaos engineering tests for resilience validation

## How to Run

### Local Development (.NET)
```bash
cd src/ExecutionRouter.Api
dotnet run
# API available at https://localhost:7052
```

### Docker
```bash
# Using Docker Compose (recommended)
docker-compose up --build

# Direct Docker run
docker build -t executionrouter:latest .
docker run -p 7052:8080 executionrouter:latest

# API available at http://localhost:7052
```

## Sample Requests

### HTTP Executor
```bash
# GET request forwarding
curl "http://localhost:7052/api/https://www.google.com" \
  -H "X-ExecutionRouter-ExecutorType: http"
```

### PowerShell Executor
```bash
# Exchange Online mailbox listing
curl "http://localhost:7052/api/mailboxes?command=Get-Mailbox" \
  -H "X-ExecutionRouter-ExecutorType: powershell"

# With correlation tracking
curl "http://localhost:7052/api/users?command=Get-User" \
  -H "X-ExecutionRouter-ExecutorType: powershell" \
  -H "X-Correlation-Id: tracking-123"
```

### Health & Metrics
```bash
# Health check
curl http://localhost:7052/health

# Detailed health
curl http://localhost:7052/health/detailed

# Metrics
curl http://localhost:7052/metrics
```

## Key Endpoints

- `GET /health` - Basic health check
- `GET /health/detailed` - Health with dependency checks
- `GET /metrics` - Request metrics and system information
- `{METHOD} /api/{**path}` - Catch-all execution endpoint

## Configuration

All settings externalized via environment variables with `ExecutionRouter__` prefix:

```bash
ExecutionRouter__Security__MaxRequestBodySizeBytes=10485760
ExecutionRouter__Resilience__MaxRetryAttempts=3
ExecutionRouter__HttpExecutor__DefaultTimeoutSeconds=30
ExecutionRouter__PowerShellExecutor__Enabled=true
ExecutionRouter__Observability__LogLevel=Information
```

## If I Had More Time...

- Improve unit/integration test suite and improve code coverage
- Improve retry policy
- Leverage SpecFlow for scenario-based testing
- Clean up code and refactor to remove duplication and unnecessary code
- Create DTO for error response
- Add circuit breaker with Polly
- Add rate limiting with Polly
- Create PowerShell session pooling and reuse mechanisms
- Expand OpenAPI documentation
- Implement distributed tracing with OpenTelemetry
- Create load testing scenarios and performance benchmarks
- Refactor executor selection to be even more extensible
- Refactor executors to abstract validation logic
- Improve health and metrics endpoints
- Implement comprehensive authentication/authorization
- Add support for additional executors (database, message queues)
- Build adaptive retry strategies based on response analysis
