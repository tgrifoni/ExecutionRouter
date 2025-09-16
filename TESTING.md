# Testing Strategy & Coverage

This document outlines the testing approach, scenarios, and coverage rationale for the ExecutionRouter project.

## Current Status

**⚠️ Tests are currently in progress** - This document describes the planned testing strategy and scenarios being implemented.

## Testing Philosophy

### Pyramid Structure
- **Unit Tests (70%)**: Fast, focused tests for individual components
- **Integration Tests (20%)**: Component interaction validation  
- **End-to-End Tests (10%)**: Full workflow validation

### Quality Gates
- Minimum 80% code coverage for critical paths
- All public APIs must have contract tests
- Resilience policies must have failure scenario tests
- Performance tests for timeout and retry behavior

## Test Scenarios Matrix

### HTTP Executor Tests

| Scenario | Executor | Expected Outcome | Transient? | Retries Used | Assertions |
|----------|----------|------------------|------------|--------------|------------|
| Valid GET request | HTTP | Success (200) | No | 0 | Response data, headers, timing |
| Valid POST with body | HTTP | Success (201) | No | 0 | Request forwarding, body preservation |
| Target timeout | HTTP | Timeout failure | Yes | 3 | Retry attempts, final timeout status |
| Target returns 500 | HTTP | Server error | Yes | 3 | Retry logic, error classification |
| Target returns 404 | HTTP | Client error | No | 0 | No retries, immediate failure |
| Network unavailable | HTTP | Connection failure | Yes | 3 | Network error handling |
| Invalid URL format | HTTP | Validation error | No | 0 | Input validation, error response |
| Large response body | HTTP | Success with truncation | No | 0 | Body size limits, truncation logic |

### PowerShell Executor Tests

| Scenario | Executor | Expected Outcome | Transient? | Retries Used | Assertions |
|----------|----------|------------------|------------|--------------|------------|
| Valid Get-Mailbox | PowerShell | Success with results | No | 0 | Command execution, output parsing |
| Valid Get-User | PowerShell | Success with results | No | 0 | Result serialization, timing |
| Invalid command | PowerShell | Validation error | No | 0 | Command allowlist enforcement |
| Session creation failure | PowerShell | Connection error | Yes | 2 | Session retry logic |
| Command timeout | PowerShell | Timeout failure | Yes | 3 | Timeout handling, cleanup |
| PowerShell not available | PowerShell | System error | No | 0 | Graceful degradation |
| Malformed PowerShell output | PowerShell | Parsing error | No | 0 | Error handling, logging |

### Validation Tests

| Scenario | Executor | Expected Outcome | Transient? | Retries Used | Assertions |
|----------|----------|------------------|------------|--------------|------------|
| Missing executor type | Any | Validation error | No | 0 | Default executor selection |
| Invalid executor type | Any | Validation error | No | 0 | Error response format |
| Request body too large | Any | Validation error | No | 0 | Size limit enforcement |
| Invalid timeout value | Any | Validation error | No | 0 | Timeout validation |
| Missing required headers | Any | Validation error | No | 0 | Header validation |

### Resilience Policy Tests

| Scenario | Executor | Expected Outcome | Transient? | Retries Used | Assertions |
|----------|----------|------------------|------------|--------------|------------|
| Transient failure → Success | Any | Success after retry | Yes | 2 | Retry count, success timing |
| Permanent transient failure | Any | Final failure | Yes | 3 | Max retries reached |
| Non-transient failure | Any | Immediate failure | No | 0 | No retry attempts |
| Retry with backoff | Any | Delayed retries | Yes | 3 | Exponential backoff timing |
| Jitter verification | Any | Variable retry timing | Yes | 3 | Jitter randomization |

## Test Implementation Strategy

### Unit Test Structure

```csharp
// Example test structure
[Fact]
public async Task ExecuteAsync_WithValidHttpRequest_ReturnsSuccessResponse()
{
    // Arrange
    var httpClient = CreateMockHttpClient();
    var executor = new HttpExecutor(httpClient, options);
    var request = CreateValidRequest();
    
    // Act
    var response = await executor.ExecuteAsync(request, cancellationToken);
    
    // Assert
    Assert.Equal(ExecutionStatus.Success, response.Status);
    Assert.NotNull(response.Result);
    Assert.True(response.DurationMilliseconds > 0);
}
```

### Integration Test Approach

#### HTTP Executor Integration
- Use `Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactory`
- Create in-memory test servers for controlled response scenarios
- No external network dependencies

#### PowerShell Executor Integration  
- Mock PowerShell runspace creation
- Use fake PowerShell commands with predictable outputs
- Test session lifecycle management

#### End-to-End API Tests
- Full request pipeline validation
- Response header verification
- Metrics collection validation

### Mock Strategy

#### Test Doubles Used
- **HttpClient Mocks**: Controlled HTTP responses for unit tests
- **PowerShell Runspace Mocks**: Fake PS execution for isolation
- **Time Provider Mocks**: Deterministic timing for retry tests
- **Logger Mocks**: Verification of logging behavior

#### Test Data Management
- Builder pattern for test request/response creation
- Fixture classes for common test scenarios
- JSON test data files for complex payloads

## Coverage Targets

### Critical Path Coverage (90%+)
- Request validation logic
- Executor selection and execution
- Resilience policy application
- Response envelope construction
- Error handling and classification

### Standard Coverage (80%+)
- Configuration binding and validation
- Metrics collection and aggregation
- Health check implementations
- Header processing and filtering

### Acceptable Coverage (60%+)
- Logging and observability features
- Performance monitoring code
- Development/debugging utilities

## Performance Test Scenarios

### Load Testing (Planned)
- Concurrent request handling (100 requests/second)
- Memory usage under sustained load
- Garbage collection impact measurement

### Stress Testing (Planned)
- Resource exhaustion scenarios
- Timeout behavior under load
- Retry storm prevention

### Chaos Testing (Planned)
- Network partition simulation
- Dependency failure injection
- Resource constraint testing

## Test Infrastructure

### Continuous Integration
- All tests run on pull request
- Code coverage reporting
- Performance regression detection

### Test Categories
- `[Fact]` - Standard unit tests
- `[Theory]` - Parameterized tests
- `[Integration]` - Integration test marker
- `[Performance]` - Performance test marker

## Known Test Gaps

### Current Limitations
1. **PowerShell Integration**: Real PowerShell testing requires Windows environment
2. **Performance Baselines**: Baseline metrics not yet established
3. **Security Testing**: Penetration testing scenarios not implemented
4. **Load Testing**: Sustained load scenarios require dedicated environment

### Planned Improvements
1. **Contract Testing**: OpenAPI schema validation
2. **Property-Based Testing**: Randomized input validation
3. **Mutation Testing**: Code quality verification
4. **Visual Testing**: API documentation accuracy

## Test Execution

### Local Development
```bash
# Run all tests
dotnet test

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"

# Run specific category
dotnet test --filter "Category=Integration"
```

### CI/CD Pipeline
```bash
# Build and test
dotnet build --configuration Release
dotnet test --configuration Release --logger trx --collect:"XPlat Code Coverage"
```

This testing strategy ensures comprehensive coverage of all critical functionality while maintaining fast feedback loops for developers.