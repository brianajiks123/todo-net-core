# ==================== Stage 1: Build ====================
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy csproj & restore (layer caching)
COPY TodoApi.Web.csproj .
RUN dotnet restore

# Copy all source code
COPY . .

# Publish app to folder /app/publish
RUN dotnet publish -c Release -o /app/publish --no-restore

# ==================== Stage 2: Runtime (Final Image) ====================
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final

# Create user non-root for safety
RUN adduser --disabled-password --gecos "" appuser
USER appuser

WORKDIR /app

# Copy result of publish from previous stage
COPY --from=build /app/publish .

# Expose port app (.NET 8+ default 8080)
EXPOSE 8080

# Environment variable
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

# Healthcheck in Docker level
HEALTHCHECK --interval=30s --timeout=5s --start-period=10s --retries=3 \
  CMD curl -f http://localhost:8080/api/v2/health/liveness || exit 1

ENTRYPOINT ["dotnet", "TodoApi.Web.dll"]
