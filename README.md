# TodoApi

A REST API for todo management built with **ASP.NET Core 10 Minimal API**, **Entity Framework Core**, and **SQLite**.

Includes JWT authentication with refresh token support, API versioning (v1/v2), rate limiting, structured logging with Serilog, health checks, and a background service for overdue todo reminders.

## Quick Start

```bash
# Set JWT secret (required)
dotnet user-secrets set "Jwt:Secret" "your-super-secret-key-minimum-32-chars" --project TodoApi.Web

# Apply migrations
dotnet ef database update --project TodoApi.Web

# Run
dotnet run --project TodoApi.Web
```

App: `http://localhost:5170` | Swagger: `http://localhost:5170/swagger`

## Documentation

| Doc | Description |
|-----|-------------|
| [ARCHITECTURE.md](docs/ARCHITECTURE.md) | Tech stack, project structure, API versioning, response format, DB migrations |
| [SETUP.md](docs/SETUP.md) | Local dev, configuration, Docker, Kubernetes, CORS, running tests |
| [FEATURES.md](docs/FEATURES.md) | Authentication, API endpoints, rate limiting, logging, background services |

## Tech Stack

- .NET 10 / ASP.NET Core Minimal API
- Entity Framework Core 10 + SQLite
- JWT Bearer + Refresh Token (BCrypt.Net)
- FluentValidation 12 · AutoMapper 16
- Serilog · Swashbuckle (Swagger UI) 7
- xUnit 2 · Moq · WebApplicationFactory
