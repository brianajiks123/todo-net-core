# Architecture

## Tech Stack

- .NET 10 / ASP.NET Core Minimal API
- Entity Framework Core 10 + SQLite
- JWT Bearer Authentication (`Microsoft.AspNetCore.Authentication.JwtBearer`)
- BCrypt.Net-Next (password hashing)
- FluentValidation 12
- AutoMapper 16
- Swashbuckle (Swagger UI) 7
- Serilog (structured logging — Console + rolling JSON file)
- xUnit 2 + Moq + `Microsoft.AspNetCore.Mvc.Testing` (testing)

## Project Structure

```
TodoApi.Web/
├── Features/
│   ├── Auth/
│   │   ├── User.cs                        # User entity
│   │   ├── RefreshToken.cs                # Refresh token entity
│   │   ├── AuthService.cs                 # Register, login, refresh logic
│   │   ├── AuthEndpoints.cs               # Auth route handlers (v1 & v2)
│   │   ├── Dtos.cs                        # Auth DTOs
│   │   ├── Repositories/
│   │   └── Validators/
│   └── Todos/
│       ├── Todo.cs                        # Entity model (soft delete & audit trail)
│       ├── TodoDbContext.cs               # EF Core DbContext
│       ├── TodoService.cs                 # Business logic & queries
│       ├── TodoEndpoints.cs               # Minimal API route handlers (v1 & v2)
│       ├── Dtos.cs                        # Request/Response DTOs
│       ├── TodoQueryParams.cs             # Query params + PagedResult<T>
│       ├── Filters/
│       ├── Mapping/
│       ├── Repositories/
│       ├── UnitOfWork/
│       └── Validators/
├── BackgroundServices/
│   └── ReminderService.cs                # Hosted service — checks overdue todos
├── Extensions/
│   ├── ServiceCollectionExtensions.cs    # DI extension methods
│   ├── WebApplicationExtensions.cs       # Middleware & endpoint mapping
│   ├── RateLimitingExtensions.cs         # Rate limiting configuration
│   ├── LoggingExtensions.cs              # Correlation ID middleware
│   └── JsonArrayFormatter.cs             # Custom Serilog sink
├── ApiResponse.cs                        # Standardized response envelope
└── Program.cs                            # App entry point & middleware
```

## API Versioning

| Version | Base URL | Rate Limiting | DueDate Support |
|---------|----------|---------------|-----------------|
| v1 | `/api/v1` | No | No |
| v2 | `/api/v2` | Yes — 100 req/min per IP | Yes |

Both versions are documented in Swagger UI under separate tabs.

## Response Format

All endpoints return a consistent `ApiResponse<T>` envelope:

```json
{
  "success": true,
  "data": { ... },
  "message": "Optional message"
}
```

On failure:
```json
{
  "success": false,
  "message": "Error description"
}
```

On validation failure (400):
```json
{
  "success": false,
  "message": "Validation failed.",
  "errors": {
    "fieldName": ["error message"]
  }
}
```

Unhandled exceptions are returned in [RFC 9457 ProblemDetails](https://www.rfc-editor.org/rfc/rfc9457) format:

```json
{
  "type": "https://httpstatuses.com/500",
  "title": "An internal server error occurred.",
  "status": 500,
  "detail": "...",
  "instance": "POST /api/v1/todos",
  "traceId": "...",
  "timestamp": "2026-03-25T00:00:00Z"
}
```

## Database Migrations

| Migration | Description |
|-----------|-------------|
| `20260325031127_AddUserOwnershipToTodos` | Add `UserId` FK to todos |
| `20260325073704_AddSoftDeleteToTodo` | Add `IsDeleted` flag |
| `20260325082755_AddAuditTrailToTodo` | Add `CreatedBy`, `UpdatedBy`, `UpdatedAt` |
| `20260331131832_AddDueDateToTodo` | Add `DueDate` (nullable) to todos |

```bash
# Apply all pending migrations
dotnet ef database update --project TodoApi.Web

# Add a new migration
dotnet ef migrations add <MigrationName> --project TodoApi.Web
```
