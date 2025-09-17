using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using ExecutionRouter.Application.Models;
using ExecutionRouter.Domain.ValueObjects;
using Microsoft.AspNetCore.Mvc.Testing;

namespace ExecutionRouter.Tests.Integration;

public class ExecutionControllerIntegrationTests(WebApplicationFactory<Program> factory)
    : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task ExecuteRequest_WithValidHttpRequest_ReturnsSuccessResult()
    {
        // Arrange
        var requestId = RequestId.Generate().Value;
        var correlationId = CorrelationId.Generate().Value;
        
        // Build the request with proper headers using the catch-all route
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/https://httpbin.org/get?executor=http");
        request.Headers.Add("X-Request-Id", requestId);
        request.Headers.Add("X-Correlation-Id", correlationId);

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        var responseContent = await response.Content.ReadAsStringAsync();
        Assert.True(response.StatusCode == HttpStatusCode.OK, $"Expected OK, got {response.StatusCode}. Response: {responseContent}");
        
        var result = JsonSerializer.Deserialize<ExecutionResponseDto>(responseContent, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        Assert.NotNull(result);
        Assert.Equal(requestId, result.RequestId);
        Assert.Equal(correlationId, result.CorrelationId);
        Assert.Equal("success", result.Status);
        Assert.NotNull(result.Result);
        Assert.NotNull(result.Result.StatusCode);
        Assert.Equal(200, result.Result.StatusCode);
        Assert.NotNull(result.Result.Body);
        Assert.Contains("httpbin.org", result.Result.Body);
    }

    [Fact]
    public async Task ExecuteRequest_WithValidPowerShellRequest_ReturnsSuccessResult()
    {
        // Arrange
        var requestId = RequestId.Generate().Value;
        var correlationId = CorrelationId.Generate().Value;
        
        var commandRequest = new { Command = "Get-Date" };
        
        // Build the request with proper headers using the catch-all route
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/test?executor=powershell");
        request.Headers.Add("X-Request-Id", requestId);
        request.Headers.Add("X-Correlation-Id", correlationId);
        request.Content = JsonContent.Create(commandRequest);

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        var responseContent = await response.Content.ReadAsStringAsync();
        Assert.True(response.StatusCode == HttpStatusCode.OK, $"Expected OK, got {response.StatusCode}. Response: {responseContent}");
        
        var result = JsonSerializer.Deserialize<ExecutionResponseDto>(responseContent, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        Assert.NotNull(result);
        Assert.Equal(requestId, result.RequestId);
        Assert.Equal(correlationId, result.CorrelationId);
        Assert.Equal("success", result.Status);
        Assert.NotNull(result.Result);
        Assert.NotNull(result.Result.Body);
        Assert.Contains("Command", result.Result.Body); // PowerShell executor returns mock data containing "Command"
    }
}