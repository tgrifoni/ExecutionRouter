# ExecutionRouter - Multi-stage Dockerfile
# Produces a small runtime image with the ExecutionRouter API service

# Build stage - Use the full SDK for building
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files for dependency restore
COPY src/ExecutionRouter.Domain/ExecutionRouter.Domain.csproj ./src/ExecutionRouter.Domain/
COPY src/ExecutionRouter.Application/ExecutionRouter.Application.csproj ./src/ExecutionRouter.Application/
COPY src/ExecutionRouter.Infrastructure/ExecutionRouter.Infrastructure.csproj ./src/ExecutionRouter.Infrastructure/
COPY src/ExecutionRouter.Api/ExecutionRouter.Api.csproj ./src/ExecutionRouter.Api/

# Restore dependencies for the API project
RUN dotnet restore src/ExecutionRouter.Api/ExecutionRouter.Api.csproj

# Copy all source code
COPY src/ ./src/

# Publish the API project directly
RUN dotnet publish src/ExecutionRouter.Api/ExecutionRouter.Api.csproj -c Release --no-restore -o /app/publish /p:UseAppHost=false

# Runtime stage - Use the minimal ASP.NET Core runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Create a non-root user for security
RUN addgroup --system --gid 1001 executionrouter && \
    adduser --system --uid 1001 --ingroup executionrouter executionrouter

# Copy the published application from build stage
COPY --from=build /app/publish .

# Install curl
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

# Set ownership of the application directory
RUN chown -R executionrouter:executionrouter /app

# Switch to non-root user
USER executionrouter

# Configure ASP.NET Core to listen on all interfaces
ENV ASPNETCORE_HTTP_PORTS=8080
ENV ASPNETCORE_ENVIRONMENT=Production

# Expose the port
EXPOSE 8080



# Health check endpoint
HEALTHCHECK --interval=30s --timeout=10s --start-period=30s --retries=3 \
    CMD curl -f http://localhost:8080/health || exit 1

# Set the entrypoint
ENTRYPOINT ["dotnet", "ExecutionRouter.Api.dll"]