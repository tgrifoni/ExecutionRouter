using ExecutionRouter.Application.Configuration;
using ExecutionRouter.Application.Validators;
using ExecutionRouter.Domain.Entities;
using ExecutionRouter.Domain.ValueObjects;
using Microsoft.Extensions.Options;
using Moq;

namespace ExecutionRouter.Tests.Unit.Application.Validators;

public class ValidationServiceTests
{
    private readonly Mock<IOptions<SecuritySettings>> _mockSecurityOptions;
    private readonly ValidationService _validationService;
    private readonly SecuritySettings _securitySettings;

    public ValidationServiceTests()
    {
        _securitySettings = new SecuritySettings
        {
            AllowedMethods = ["GET", "POST", "PUT", "DELETE"],
            BlockedHeaders = ["X-Blocked-Header"],
            MaxRequestBodySizeBytes = 1024,
            MaxHeaderCount = 10,
            MaxHeaderValueLength = 100,
            MaxQueryParameterCount = 10,
            MaxQueryParameterValueLength = 100,
            MaxTimeoutSeconds = 60,
            MaxPathLength = 256,
            SensitiveHeaders = ["Authorization", "Cookie"],
            SensitiveQueryParameters = ["password", "token"],
            ValidateRequestBody = true,
            AllowedContentTypes = ["application/json", "text/plain"]
        };

        _mockSecurityOptions = new Mock<IOptions<SecuritySettings>>();
        _mockSecurityOptions.Setup(x => x.Value).Returns(_securitySettings);
        
        _validationService = new ValidationService(_mockSecurityOptions.Object);
    }

    [Fact]
    public void ValidateRequest_WithValidRequest_ReturnsValid()
    {
        // Arrange
        var request = CreateValidExecutionRequest(ExecutorType.Http);

        // Act
        var result = _validationService.ValidateRequest(request);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void ValidateRequest_WithInvalidMethod_ReturnsInvalid(string? method)
    {
        // Arrange
        var request = CreateValidExecutionRequest(ExecutorType.Http);
        request = ExecutionRequest.Create(
            request.RequestId,
            request.CorrelationId!,
            request.ExecutorType,
            method!,
            request.Path,
            request.TimeoutSeconds,
            request.QueryParameters,
            request.Headers,
            request.Body);

        // Act
        var result = _validationService.ValidateRequest(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("Method is required"));
    }

    [Fact]
    public void ValidateRequest_WithUnsupportedMethod_ReturnsInvalid()
    {
        // Arrange
        var request = CreateValidExecutionRequest(ExecutorType.Http);
        request = ExecutionRequest.Create(
            request.RequestId,
            request.CorrelationId!,
            request.ExecutorType,
            "UNSUPPORTED",
            request.Path,
            request.TimeoutSeconds,
            request.QueryParameters,
            request.Headers,
            request.Body);

        // Act
        var result = _validationService.ValidateRequest(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("Unsupported HTTP method"));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void ValidateRequest_WithInvalidPath_ReturnsInvalid(string? path)
    {
        // Arrange
        var request = CreateValidExecutionRequest(ExecutorType.Http);
        request = ExecutionRequest.Create(
            request.RequestId,
            request.CorrelationId!,
            request.ExecutorType,
            request.Method,
            path!,
            request.TimeoutSeconds,
            request.QueryParameters,
            request.Headers,
            request.Body);

        // Act
        var result = _validationService.ValidateRequest(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("Path is required"));
    }

    [Fact]
    public void ValidateRequest_WithTooLongPath_ReturnsInvalid()
    {
        // Arrange
        var longPath = new string('a', _securitySettings.MaxPathLength + 1);
        var request = CreateValidExecutionRequest(ExecutorType.Http);
        request = ExecutionRequest.Create(
            request.RequestId,
            request.CorrelationId!,
            request.ExecutorType,
            request.Method,
            longPath,
            request.TimeoutSeconds,
            request.QueryParameters,
            request.Headers,
            request.Body);

        // Act
        var result = _validationService.ValidateRequest(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("Path too long"));
    }

    [Fact]
    public void ValidateRequest_WithTooLargeBody_ReturnsInvalid()
    {
        // Arrange
        var largeBody = new string('x', (int)(_securitySettings.MaxRequestBodySizeBytes + 1));
        var request = CreateValidExecutionRequest(ExecutorType.Http);
        request = ExecutionRequest.Create(
            request.RequestId,
            request.CorrelationId!,
            request.ExecutorType,
            request.Method,
            request.Path,
            request.TimeoutSeconds,
            request.QueryParameters,
            request.Headers,
            largeBody);

        // Act
        var result = _validationService.ValidateRequest(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("Request body too large"));
    }

    [Fact]
    public void ValidateRequest_WithBlockedHeader_ReturnsInvalid()
    {
        // Arrange
        var blockedHeaderKey = "X-Blocked-Header";
        var headers = new Dictionary<string, string>
        {
            ["Content-Type"] = "application/json",
            [blockedHeaderKey] = "some-value"
        };
        
        var request = CreateValidExecutionRequest(ExecutorType.Http);
        request = ExecutionRequest.Create(
            request.RequestId,
            request.CorrelationId!,
            request.ExecutorType,
            request.Method,
            request.Path,
            request.TimeoutSeconds,
            request.QueryParameters,
            headers,
            request.Body);

        // Act
        var result = _validationService.ValidateRequest(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains($"Header '{blockedHeaderKey}' is not allowed"));
    }

    [Fact]
    public void ValidateRequest_WithTooManyHeaders_ReturnsInvalid()
    {
        // Arrange
        var headers = new Dictionary<string, string>();
        for (var i = 0; i <= _securitySettings.MaxHeaderCount; i++)
        {
            headers[$"Header-{i}"] = "value";
        }
        
        var request = CreateValidExecutionRequest(ExecutorType.Http);
        request = ExecutionRequest.Create(
            request.RequestId,
            request.CorrelationId!,
            request.ExecutorType,
            request.Method,
            request.Path,
            request.TimeoutSeconds,
            request.QueryParameters,
            headers,
            request.Body);

        // Act
        var result = _validationService.ValidateRequest(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("Too many headers"));
    }

    [Fact]
    public void ValidateRequest_WithTooLongHeaderValue_ReturnsInvalid()
    {
        // Arrange
        var longValue = new string('x', _securitySettings.MaxHeaderValueLength + 1);
        var headers = new Dictionary<string, string>
        {
            ["Content-Type"] = "application/json",
            ["X-Long-Header"] = longValue
        };
        
        var request = CreateValidExecutionRequest(ExecutorType.Http);
        request = ExecutionRequest.Create(
            request.RequestId,
            request.CorrelationId!,
            request.ExecutorType,
            request.Method,
            request.Path,
            request.TimeoutSeconds,
            request.QueryParameters,
            headers,
            request.Body);

        // Act
        var result = _validationService.ValidateRequest(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("too long"));
    }

    [Fact]
    public void ValidateRequest_WithTooManyQueryParameters_ReturnsInvalid()
    {
        // Arrange
        var queryParams = new Dictionary<string, string>();
        for (var i = 0; i <= _securitySettings.MaxQueryParameterCount; i++)
        {
            queryParams[$"param{i}"] = "value";
        }
        
        var request = CreateValidExecutionRequest(ExecutorType.Http);
        request = ExecutionRequest.Create(
            request.RequestId,
            request.CorrelationId!,
            request.ExecutorType,
            request.Method,
            request.Path,
            request.TimeoutSeconds,
            queryParams,
            request.Headers,
            request.Body);

        // Act
        var result = _validationService.ValidateRequest(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("Too many query parameters"));
    }

    [Fact]
    public void ValidateRequest_WithHttpExecutorType_PerformsHttpValidation()
    {
        // Arrange
        var request = CreateValidExecutionRequest(ExecutorType.Http);

        // Act
        var result = _validationService.ValidateRequest(request);

        // Assert
        Assert.True(result.IsValid); // Should pass basic validation
    }

    [Fact]
    public void ValidateRequest_WithPowerShellExecutorType_PerformsPowerShellValidation()
    {
        // Arrange
        var request = CreateValidExecutionRequest(ExecutorType.PowerShell);

        // Act
        var result = _validationService.ValidateRequest(request);

        // Assert
        Assert.True(result.IsValid); // Should pass basic validation
    }

    private static ExecutionRequest CreateValidExecutionRequest(ExecutorType executorType)
    {
        return ExecutionRequest.Create(
            RequestId.Generate().Value,
            CorrelationId.Generate().Value,
            executorType,
            "POST",
            "/api/test",
            TimeSpan.FromSeconds(30),
            new Dictionary<string, string> { ["param1"] = "value1" },
            new Dictionary<string, string> { ["Content-Type"] = "application/json" },
            "{\"test\": \"data\"}");
    }
}