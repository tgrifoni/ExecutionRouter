using System.Net;
using ExecutionRouter.Application.Services;
using ExecutionRouter.Application.Common;
using ExecutionRouter.Application.Configuration;
using ExecutionRouter.Application.Validators;
using ExecutionRouter.Domain.Constants;
using ExecutionRouter.Domain.Entities;
using ExecutionRouter.Domain.Interfaces;
using ExecutionRouter.Infrastructure.Executors;
using ExecutionRouter.Infrastructure.Resilience;
using ExecutionRouter.Infrastructure.Observability;
using Microsoft.Extensions.Options;

namespace ExecutionRouter.Api.Extensions;

/// <summary>
/// Service collection extensions for dependency injection setup
/// </summary>
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddExecutionRouter(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<SecuritySettings>(configuration.GetSection(SecuritySettings.SectionName));
        services.Configure<HttpExecutorSettings>(configuration.GetSection(HttpExecutorSettings.SectionName));
        services.Configure<PowerShellExecutorSettings>(configuration.GetSection(PowerShellExecutorSettings.SectionName));
        services.Configure<ResilienceSettings>(configuration.GetSection(ResilienceSettings.SectionName));
        services.Configure<ObservabilitySettings>(configuration.GetSection(ObservabilitySettings.SectionName));
        
        services.AddSingleton<ISystemClock, SystemClock>();
        services.AddSingleton<IMetricsCollector, InMemoryMetricsCollector>();
        services.AddSingleton<StructuredLogger>();
        
        services.AddScoped<IConfigurationValidationService, ConfigurationValidationService>();
        services.AddScoped<IExecutorValidationService<HttpExecutorSettings>, HttpExecutorValidationService>();
        services.AddScoped<IExecutorValidationService<PowerShellExecutorSettings>, PowerShellExecutorValidationService>();
        
        services.AddScoped<IValidationService, ValidationService>();
        
        services
            .AddHttpClient<IExecutor, HttpExecutor>((serviceProvider, client) =>
            {
                var httpSettings = serviceProvider.GetRequiredService<IOptions<HttpExecutorSettings>>().Value;
                client.Timeout = TimeSpan.FromSeconds(httpSettings.DefaultTimeoutSeconds);
            })
            .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
            {
                AutomaticDecompression = DecompressionMethods.All
            });
        services.AddScoped<IExecutor, PowerShellExecutor>();
        
        services.AddScoped<IResiliencePolicy>(serviceProvider =>
        {
            var resilienceSettings = serviceProvider.GetRequiredService<IOptions<ResilienceSettings>>().Value;
            
            var retryConfig = RetryPolicyConfiguration.FromOptions(
                resilienceSettings.MaxRetryAttempts,
                resilienceSettings.BaseDelayMilliseconds,
                resilienceSettings.MaxDelayMilliseconds,
                resilienceSettings.BackoffMultiplier,
                resilienceSettings.UseJitter);
            var systemClock = serviceProvider.GetRequiredService<ISystemClock>();
            return new RetryResiliencePolicy(retryConfig, systemClock);
        });
        
        services.AddScoped<ExecutionOrchestrationService>();
        
        return services;
    }

    public static IServiceCollection AddExecutionRouterLogging(this IServiceCollection services)
    {
        services.AddLogging(builder =>
        {
            builder.AddJsonConsole(options =>
            {
                options.IncludeScopes = true;
                options.TimestampFormat = "yyyy-MM-dd HH:mm:ss.fff ";
                options.JsonWriterOptions = new System.Text.Json.JsonWriterOptions
                {
                    Indented = false
                };
            });
        });
        
        return services;
    }

    public static IServiceCollection AddExecutionRouterCors(this IServiceCollection services)
    {
        services.AddCors(options =>
        {
            options.AddDefaultPolicy(policy =>
            {
                policy
                    .AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .WithExposedHeaders(
                        Headers.ExecutionRouter.RequestId,
                        Headers.ExecutionRouter.CorrelationId, 
                        Headers.ExecutionRouter.Instance,
                        Headers.ExecutionRouter.AttemptCount,
                        Headers.ExecutionRouter.Duration);
            });
        });
        
        return services;
    }
}