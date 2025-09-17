using ExecutionRouter.Application.Configuration;
using ExecutionRouter.Application.Validators;
using Microsoft.Extensions.Logging;
using Moq;

namespace ExecutionRouter.Tests.Unit.Application.Validators;

public class PowerShellExecutorValidationServiceTests
{
    private readonly Mock<ILogger<PowerShellExecutorValidationService>> _mockLogger;
    private readonly PowerShellExecutorValidationService _validationService;
    private readonly PowerShellExecutorSettings _settings;

    public PowerShellExecutorValidationServiceTests()
    {
        _mockLogger = new Mock<ILogger<PowerShellExecutorValidationService>>();
        _validationService = new PowerShellExecutorValidationService(_mockLogger.Object);
        
        _settings = new PowerShellExecutorSettings
        {
            AllowedCmdlets = ["Get-Date", "Get-Process", "Get-Service", "Get-*", "Write-Output"],
            BlockedCmdlets = ["Remove-Item", "Stop-Process", "Invoke-Expression"]
        };
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void IsTargetAllowed_WithNullOrEmptyCommand_ReturnsFalse(string? command)
    {
        // Act
        var result = _validationService.IsTargetAllowed(command, _settings);

        // Assert
        Assert.False(result);
    }

    [Theory]
    [InlineData("Get-Date")]
    [InlineData("GET-DATE")]
    [InlineData("get-date")]
    [InlineData("Get-Process")]
    [InlineData("Get-Service")]
    [InlineData("Write-Output")]
    public void IsTargetAllowed_WithAllowedCmdlet_ReturnsTrue(string command)
    {
        // Act
        var result = _validationService.IsTargetAllowed(command, _settings);

        // Assert
        Assert.True(result);
    }

    [Theory]
    [InlineData("Get-Date -Format 'yyyy-MM-dd'")]
    [InlineData("Get-Process | Where-Object Name -eq 'notepad'")]
    [InlineData("Write-Output 'Hello World'")]
    public void IsTargetAllowed_WithAllowedCmdletAndParameters_ReturnsTrue(string command)
    {
        // Act
        var result = _validationService.IsTargetAllowed(command, _settings);

        // Assert
        Assert.True(result);
    }

    [Theory]
    [InlineData("Remove-Item")]
    [InlineData("REMOVE-ITEM")]
    [InlineData("remove-item")]
    [InlineData("Stop-Process")]
    [InlineData("Invoke-Expression")]
    public void IsTargetAllowed_WithBlockedCmdlet_ReturnsFalse(string command)
    {
        // Act
        var result = _validationService.IsTargetAllowed(command, _settings);

        // Assert
        Assert.False(result);
        
        // Verify warning was logged
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("is blocked")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Theory]
    [InlineData("Remove-Item C:\\temp\\file.txt")]
    [InlineData("Stop-Process -Name notepad -Force")]
    [InlineData("Invoke-Expression 'Get-Date'")]
    public void IsTargetAllowed_WithBlockedCmdletAndParameters_ReturnsFalse(string command)
    {
        // Act
        var result = _validationService.IsTargetAllowed(command, _settings);

        // Assert
        Assert.False(result);
    }

    [Theory]
    [InlineData("Set-Location")]
    [InlineData("New-Item")]
    [InlineData("Test-Connection")]
    public void IsTargetAllowed_WithNotAllowedCmdlet_ReturnsFalse(string command)
    {
        // Act
        var result = _validationService.IsTargetAllowed(command, _settings);

        // Assert
        Assert.False(result);
        
        // Verify warning was logged
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("not in the allowed list")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Theory]
    [InlineData("   Get-Date   ")]
    [InlineData("\tGet-Process\t")]
    [InlineData("\nGet-Service\n")]
    public void IsTargetAllowed_WithWhitespaceAroundCommand_HandlesCorrectly(string command)
    {
        // Act
        var result = _validationService.IsTargetAllowed(command, _settings);

        // Assert
        Assert.True(result);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\t\n")]
    public void IsTargetAllowed_WithOnlyWhitespace_ReturnsFalse(string command)
    {
        // Act
        var result = _validationService.IsTargetAllowed(command, _settings);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsTargetAllowed_BlockedCmdletTakesPrecedenceOverAllowed_ReturnsFalse()
    {
        // Arrange - Add a cmdlet to both allowed and blocked lists
        var settings = new PowerShellExecutorSettings
        {
            AllowedCmdlets = ["Get-Process", "Remove-Item"],
            BlockedCmdlets = ["Remove-Item"]
        };

        // Act
        var result = _validationService.IsTargetAllowed("Remove-Item", settings);

        // Assert
        Assert.False(result, "Blocked cmdlets should take precedence over allowed cmdlets");
    }

    [Fact]
    public void IsTargetAllowed_WithEmptyAllowedList_ReturnsFalse()
    {
        // Arrange
        var settings = new PowerShellExecutorSettings
        {
            AllowedCmdlets = [],
            BlockedCmdlets = ["Remove-Item"]
        };

        // Act
        var result = _validationService.IsTargetAllowed("Get-Date", settings);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsTargetAllowed_WithComplexCommandLine_ExtractsFirstCmdletCorrectly()
    {
        // Arrange
        var complexCommand = "Get-Process | Where-Object {$_.ProcessName -eq 'notepad'} | Select-Object ProcessName, Id";

        // Act
        var result = _validationService.IsTargetAllowed(complexCommand, _settings);

        // Assert
        Assert.True(result, "Should extract 'Get-Process' as the first cmdlet and validate it");
    }
}