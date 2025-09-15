using ExecutionRouter.Api.Extensions;
using ExecutionRouter.Api.Middleware;
using ExecutionRouter.Application.Configuration;
using ExecutionRouter.Application.Validators;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddEnvironmentVariables("EXECUTION_ROUTER_");

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    { 
        Title = "ExecutionRouter API", 
        Version = "v1",
        Description = "Remote Request Execution Service - A service for routing and executing requests"
    });
});

builder.Services.AddExecutionRouter(builder.Configuration);
builder.Services.AddExecutionRouterLogging();
builder.Services.AddExecutionRouterCors();

var app = builder.Build();

var startupLogger = app.Services.GetRequiredService<ILogger<Program>>();
try
{
    using var scope = app.Services.CreateScope();
    var configValidationService = scope.ServiceProvider.GetRequiredService<IConfigurationValidationService>();
    var validationResult = configValidationService.ValidateConfiguration(app.Services.GetRequiredService<IOptions<SecuritySettings>>().Value,
        app.Services.GetRequiredService<IOptions<HttpExecutorSettings>>().Value,
        app.Services.GetRequiredService<IOptions<PowerShellExecutorSettings>>().Value,
        app.Services.GetRequiredService<IOptions<ResilienceSettings>>().Value,
        app.Services.GetRequiredService<IOptions<ObservabilitySettings>>().Value);
    if (!validationResult.IsValid)
    {
        startupLogger.LogError("Configuration validation failed: {Errors}", validationResult.GetErrorSummary());
        throw new InvalidOperationException($"Configuration validation failed: {validationResult.GetErrorSummary()}");
    }
}
catch (Exception ex)
{
    startupLogger.LogError(ex, "Failed to validate configuration on startup");
    throw;
}

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "ExecutionRouter API v1");
    c.RoutePrefix = string.Empty;
});

app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseMiddleware<RequestLoggingMiddleware>();

app.UseCors();
app.UseRouting();
app.MapControllers();

startupLogger.LogInformation("ExecutionRouter API starting up");
startupLogger.LogInformation("Environment: {Environment}", app.Environment.EnvironmentName);
startupLogger.LogInformation("Version: {Version}", typeof(Program).Assembly.GetName().Version);

app.Run();

// Make Program accessible for integration testing
public partial class Program { }
