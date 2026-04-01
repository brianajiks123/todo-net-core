# ==================== Stage 1: Build ====================
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy csproj & restore (layer caching)
COPY TodoApi.Web.csproj .
RUN dotnet restore

# Copy all source code
COPY . .

# Publish to /app/publish
RUN dotnet publish -c Release -o /app/publish --no-restore

# ==================== Stage 2: Runtime (Final Image) ====================
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final

# Install wget for healthcheck (curl is not exist in the base image)
USER root
RUN apt-get update && apt-get install -y --no-install-recommends wget \
    && rm -rf /var/lib/apt/lists/*

# Running as non-root user
RUN adduser --disabled-password --gecos "" appuser
USER appuser

WORKDIR /app

# Copy result of publish fro previous stage
COPY --from=build /app/publish .

# Port default .NET 8+
EXPOSE 8080

# Environment variables
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

# Healthcheck using wget (available in Debian-based aspnet image)
HEALTHCHECK --interval=30s --timeout=5s --start-period=15s --retries=3 \
  CMD wget -qO- http://localhost:8080/api/v2/health/liveness || exit 1

ENTRYPOINT ["dotnet", "TodoApi.Web.dll"]
