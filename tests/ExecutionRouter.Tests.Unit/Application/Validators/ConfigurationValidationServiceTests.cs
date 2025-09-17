using ExecutionRouter.Application.Configuration;
using ExecutionRouter.Application.Validators;

namespace ExecutionRouter.Tests.Unit.Application.Validators;

public class ConfigurationValidationServiceTests
{
    private readonly ConfigurationValidationService _validationService;

    public ConfigurationValidationServiceTests()
    {
        _validationService = new ConfigurationValidationService();
    }

    [Fact]
    public void ValidateConfiguration_WithValidSettings_ReturnsValid()
    {
        // Arrange
        var securitySettings = CreateValidSecuritySettings();
        var httpSettings = CreateValidHttpExecutorSettings();
        var powershellSettings = CreateValidPowerShellExecutorSettings();
        var resilienceSettings = CreateValidResilienceSettings();
        var observabilitySettings = CreateValidObservabilitySettings();

        // Act
        var result = _validationService.ValidateConfiguration(
            securitySettings, httpSettings, powershellSettings, resilienceSettings, observabilitySettings);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void ValidateConfiguration_WithInvalidSecuritySettings_ReturnsInvalid()
    {
        // Arrange
        var securitySettings = new SecuritySettings { MaxRequestBodySizeBytes = 100 }; // Too small
        var httpSettings = CreateValidHttpExecutorSettings();
        var powershellSettings = CreateValidPowerShellExecutorSettings();
        var resilienceSettings = CreateValidResilienceSettings();
        var observabilitySettings = CreateValidObservabilitySettings();

        // Act
        var result = _validationService.ValidateConfiguration(
            securitySettings, httpSettings, powershellSettings, resilienceSettings, observabilitySettings);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("MaxRequestBodySizeBytes"));
    }

    [Fact]
    public void ValidateConfiguration_WithInvalidHttpSettings_ReturnsInvalid()
    {
        // Arrange
        var securitySettings = CreateValidSecuritySettings();
        var httpSettings = new HttpExecutorSettings { MaxConcurrentRequests = 0 }; // Too small
        var powershellSettings = CreateValidPowerShellExecutorSettings();
        var resilienceSettings = CreateValidResilienceSettings();
        var observabilitySettings = CreateValidObservabilitySettings();

        // Act
        var result = _validationService.ValidateConfiguration(
            securitySettings, httpSettings, powershellSettings, resilienceSettings, observabilitySettings);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("MaxConcurrentRequests"));
    }

    [Fact]
    public void ValidateConfiguration_WithInvalidPowerShellSettings_ReturnsInvalid()
    {
        // Arrange
        var securitySettings = CreateValidSecuritySettings();
        var httpSettings = CreateValidHttpExecutorSettings();
        var powershellSettings = new PowerShellExecutorSettings { MaxExecutionTimeoutSeconds = 0 }; // Too small
        var resilienceSettings = CreateValidResilienceSettings();
        var observabilitySettings = CreateValidObservabilitySettings();

        // Act
        var result = _validationService.ValidateConfiguration(
            securitySettings, httpSettings, powershellSettings, resilienceSettings, observabilitySettings);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("MaxExecutionTimeoutSeconds"));
    }

    [Fact]
    public void ValidateConfiguration_WithInvalidResilienceSettings_ReturnsInvalid()
    {
        // Arrange
        var securitySettings = CreateValidSecuritySettings();
        var httpSettings = CreateValidHttpExecutorSettings();
        var powershellSettings = CreateValidPowerShellExecutorSettings();
        var resilienceSettings = new ResilienceSettings
        {
            BaseDelayMilliseconds = 10000,
            MaxDelayMilliseconds = 5000 // BaseDelay > MaxDelay
        };
        var observabilitySettings = CreateValidObservabilitySettings();

        // Act
        var result = _validationService.ValidateConfiguration(
            securitySettings, httpSettings, powershellSettings, resilienceSettings, observabilitySettings);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("BaseDelayMilliseconds must be less than MaxDelayMilliseconds"));
    }

    #region Helper Methods

    private static SecuritySettings CreateValidSecuritySettings()
    {
        return new SecuritySettings();
    }

    private static HttpExecutorSettings CreateValidHttpExecutorSettings()
    {
        return new HttpExecutorSettings();
    }

    private static PowerShellExecutorSettings CreateValidPowerShellExecutorSettings()
    {
        return new PowerShellExecutorSettings();
    }

    private static ResilienceSettings CreateValidResilienceSettings()
    {
        return new ResilienceSettings();
    }

    private static ObservabilitySettings CreateValidObservabilitySettings()
    {
        return new ObservabilitySettings();
    }

    #endregion
}