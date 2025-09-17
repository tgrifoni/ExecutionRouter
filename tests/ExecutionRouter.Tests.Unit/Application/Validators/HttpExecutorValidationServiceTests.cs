using ExecutionRouter.Application.Configuration;
using ExecutionRouter.Application.Validators;
using Microsoft.Extensions.Logging;
using Moq;

namespace ExecutionRouter.Tests.Unit.Application.Validators;

public class HttpExecutorValidationServiceTests
{
    private readonly Mock<ILogger<HttpExecutorValidationService>> _mockLogger;
    private readonly HttpExecutorValidationService _validationService;
    private readonly HttpExecutorSettings _settings;

    public HttpExecutorValidationServiceTests()
    {
        _mockLogger = new Mock<ILogger<HttpExecutorValidationService>>();
        _validationService = new HttpExecutorValidationService(_mockLogger.Object);
        
        _settings = new HttpExecutorSettings
        {
            AllowedTargetPatterns = 
            [
                @"^https://api\.example\.com/.*",
                @"^https://httpbin\.org/.*",
                @"^https://.*\.contoso\.com/.*"
            ],
            BlockedTargetPatterns = 
            [
                @".*admin.*",
                @".*localhost.*",
                @".*127\.0\.0\.1.*"
            ]
        };
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void IsTargetAllowed_WithNullOrEmptyUrl_ReturnsFalse(string? url)
    {
        // Act
        var result = _validationService.IsTargetAllowed(url, _settings);

        // Assert
        Assert.False(result);
    }

    [Theory]
    [InlineData("https://api.example.com/users")]
    [InlineData("https://api.example.com/products/123")]
    [InlineData("https://httpbin.org/get")]
    [InlineData("https://httpbin.org/post")]
    [InlineData("https://sub.contoso.com/api/data")]
    [InlineData("https://app.contoso.com/service")]
    public void IsTargetAllowed_WithAllowedUrls_ReturnsTrue(string url)
    {
        // Act
        var result = _validationService.IsTargetAllowed(url, _settings);

        // Assert
        Assert.True(result);
    }

    [Theory]
    [InlineData("https://api.example.com/admin")]
    [InlineData("https://httpbin.org/admin/users")]
    [InlineData("https://localhost:8080/api")]
    [InlineData("https://127.0.0.1:3000/service")]
    [InlineData("http://localhost/admin")]
    public void IsTargetAllowed_WithBlockedUrls_ReturnsFalse(string url)
    {
        // Act
        var result = _validationService.IsTargetAllowed(url, _settings);

        // Assert
        Assert.False(result);
        
        // Verify warning was logged
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("blocked by pattern")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Theory]
    [InlineData("https://malicious.com/api")]
    [InlineData("https://evil.org/data")]
    [InlineData("http://badsite.net/service")]
    public void IsTargetAllowed_WithNotAllowedUrls_ReturnsFalse(string url)
    {
        // Act
        var result = _validationService.IsTargetAllowed(url, _settings);

        // Assert
        Assert.False(result);
        
        // Verify warning was logged
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("not matching any allowed patterns")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void IsTargetAllowed_BlockedPatternTakesPrecedence_ReturnsFalse()
    {
        // Arrange - URL matches both allowed and blocked patterns
        var url = "https://api.example.com/admin/users";

        // Act
        var result = _validationService.IsTargetAllowed(url, _settings);

        // Assert
        Assert.False(result, "Blocked patterns should take precedence over allowed patterns");
    }

    [Theory]
    [InlineData("HTTPS://API.EXAMPLE.COM/USERS")]
    [InlineData("https://API.EXAMPLE.COM/users")]
    [InlineData("https://api.EXAMPLE.com/USERS")]
    public void IsTargetAllowed_WithDifferentCasing_HandlesCaseInsensitively(string url)
    {
        // Act
        var result = _validationService.IsTargetAllowed(url, _settings);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsTargetAllowed_WithInvalidRegexPattern_ReturnsFalse()
    {
        // Arrange - Create settings with invalid regex
        var invalidSettings = new HttpExecutorSettings
        {
            AllowedTargetPatterns = [@"[invalid regex pattern"],
            BlockedTargetPatterns = []
        };

        // Act
        var result = _validationService.IsTargetAllowed("https://example.com", invalidSettings);

        // Assert
        Assert.False(result);
        
        // Verify error was logged
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error validating target URL")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void IsTargetAllowed_WithEmptyAllowedPatterns_ReturnsFalse()
    {
        // Arrange
        var settings = new HttpExecutorSettings
        {
            AllowedTargetPatterns = [],
            BlockedTargetPatterns = [@".*admin.*"]
        };

        // Act
        var result = _validationService.IsTargetAllowed("https://api.example.com/users", settings);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsTargetAllowed_WithEmptyBlockedPatterns_AllowsBasedOnAllowedOnly()
    {
        // Arrange  
        var settings = new HttpExecutorSettings
        {
            AllowedTargetPatterns = [@"^https://api\.example\.com/.*"],
            BlockedTargetPatterns = []
        };

        // Act & Assert - Should allow matching URL
        var allowedResult = _validationService.IsTargetAllowed("https://api.example.com/users", settings);
        Assert.True(allowedResult);

        // Act & Assert - Should reject non-matching URL
        var rejectedResult = _validationService.IsTargetAllowed("https://malicious.com/api", settings);
        Assert.False(rejectedResult);
    }

    [Theory]
    [InlineData("https://api.example.com/users?id=123&name=test")]
    [InlineData("https://httpbin.org/get?param1=value1&param2=value2")]
    public void IsTargetAllowed_WithQueryParameters_ValidatesCorrectly(string url)
    {
        // Act
        var result = _validationService.IsTargetAllowed(url, _settings);

        // Assert
        Assert.True(result);
    }

    [Theory]
    [InlineData("https://api.example.com/users#section1")]
    [InlineData("https://httpbin.org/get#top")]
    public void IsTargetAllowed_WithFragment_ValidatesCorrectly(string url)
    {
        // Act
        var result = _validationService.IsTargetAllowed(url, _settings);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsTargetAllowed_WithComplexRegexPatterns_WorksCorrectly()
    {
        // Arrange - More complex regex patterns
        var complexSettings = new HttpExecutorSettings
        {
            AllowedTargetPatterns = 
            [
                @"^https://api\.(staging|prod)\.example\.com/(v[1-9]\d*)/.*",
                @"^https://[a-zA-Z0-9\-]+\.contoso\.com/api/.*"
            ],
            BlockedTargetPatterns = 
            [
                @".*/internal/.*",
                @".*/(admin|management)/.*"
            ]
        };

        var service = new HttpExecutorValidationService(_mockLogger.Object);

        // Act & Assert - Should allow complex valid URLs
        Assert.True(service.IsTargetAllowed("https://api.staging.example.com/v1/users", complexSettings));
        Assert.True(service.IsTargetAllowed("https://api.prod.example.com/v2/products", complexSettings));
        Assert.True(service.IsTargetAllowed("https://app-service.contoso.com/api/data", complexSettings));

        // Act & Assert - Should block internal/admin URLs
        Assert.False(service.IsTargetAllowed("https://api.staging.example.com/v1/internal/users", complexSettings));
        Assert.False(service.IsTargetAllowed("https://api.prod.example.com/v2/admin/config", complexSettings));
    }
}