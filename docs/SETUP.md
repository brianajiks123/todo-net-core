# Setup Guide

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Docker](https://docs.docker.com/get-docker/) (optional, for containerized deployment)

## Local Development

```bash
# 1. Clone the repository
git clone <repo-url>
cd TodoApi

# 2. Set the JWT Secret (required)
dotnet user-secrets set "Jwt:Secret" "your-super-secret-key-minimum-32-chars" --project TodoApi.Web

# 3. Apply database migrations
dotnet ef database update --project TodoApi.Web

# 4. Run the application
dotnet run --project TodoApi.Web
```

App runs at: `http://localhost:5170`
Swagger UI: `http://localhost:5170/swagger` (Development only)

## Configuration

Configuration is read in this priority order:

1. `appsettings.json`
2. `appsettings.{Environment}.json`
3. User Secrets (Development only)
4. Environment Variables
5. Command-line arguments

### Required Variables

| Key | Description | Required |
|-----|-------------|----------|
| `Jwt__Secret` | Secret key for signing JWT tokens (min. 32 chars) | Yes |
| `Jwt__Issuer` | JWT token issuer | No (default: `todo-api`) |
| `Jwt__Audience` | JWT token audience | No (default: `todo-api`) |
| `Jwt__ExpirationHours` | Access token expiration in hours | No (default: `24`) |
| `Jwt__RefreshExpirationDays` | Refresh token expiration in days | No (default: `7`) |

> The app will throw an exception on startup if `Jwt__Secret` is not set.

### Option 1: User Secrets (Recommended for Development)

```bash
dotnet user-secrets set "Jwt:Secret" "your-super-secret-key-minimum-32-chars" --project TodoApi.Web
dotnet user-secrets set "Jwt:ExpirationHours" "1" --project TodoApi.Web
dotnet user-secrets set "Jwt:RefreshExpirationDays" "7" --project TodoApi.Web

# List all stored secrets
dotnet user-secrets list --project TodoApi.Web
```

### Option 2: Environment Variables

**Linux / macOS:**
```bash
export Jwt__Secret="your-super-secret-key-minimum-32-chars"
dotnet run --project TodoApi.Web
```

**Windows (PowerShell):**
```powershell
$env:Jwt__Secret = "your-super-secret-key-minimum-32-chars"
dotnet run --project TodoApi.Web
```

## CORS Configuration

Development origins are defined in `appsettings.Development.json`:

```json
{
  "Cors": {
    "AllowedOrigins": [
      "http://localhost:3000",
      "http://localhost:5173"
    ]
  }
}
```

For production/staging, set origins via environment variables. The app will throw on startup if no origins are configured in non-development environments.

**Linux / macOS:**
```bash
export Cors__AllowedOrigins__0=https://todo-app.yourdomain.com
```

**Windows PowerShell:**
```powershell
$env:Cors__AllowedOrigins__0 = "https://todo-app.yourdomain.com"
```

## Docker

```bash
# Build image (run from repo root)
docker build -t todoapi:latest ./TodoApi.Web

# Run container
docker run -d \
  -p 8080:8080 \
  -e Jwt__Secret="your-super-secret-key-minimum-32-chars" \
  -e ASPNETCORE_ENVIRONMENT=Production \
  --name todoapi \
  todoapi:latest
```

App runs at: `http://localhost:8080`

### Docker Compose

```yaml
services:
  todoapi:
    build:
      context: ./TodoApi.Web
    ports:
      - "8080:8080"
    environment:
      - Jwt__Secret=your-super-secret-key-minimum-32-chars
      - Jwt__Issuer=todo-api
      - Jwt__Audience=todo-api
      - Jwt__ExpirationHours=24
      - Jwt__RefreshExpirationDays=7
      - ASPNETCORE_ENVIRONMENT=Production
    restart: unless-stopped
```

```bash
docker compose up -d
```

## Kubernetes

Manifests are in the `k8s/` directory.

```bash
# Create namespace
kubectl create namespace todoapi-dev

# Create secret for JWT
kubectl create secret generic todoapi-secrets \
  --from-literal=Jwt__Secret="your-super-secret-key-minimum-32-chars" \
  -n todoapi-dev

# Apply manifests
kubectl apply -f k8s/ -n todoapi-dev

# Check status
kubectl get pods -n todoapi-dev
```

### Resource Limits

| Resource | Request | Limit |
|----------|---------|-------|
| CPU | 200m | 500m |
| Memory | 256Mi | 512Mi |

### Health Endpoints

| Endpoint | Purpose |
|----------|---------|
| `GET /api/v2/health/liveness` | Liveness probe |
| `GET /api/v2/health/readiness` | Readiness probe (DB check) |
| `GET /api/v2/health` | Combined health status |

## Running Tests

```bash
dotnet test TodoApi.Web.Tests
```
