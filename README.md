# TodoApi

A simple REST API for todo management built with **ASP.NET Core 10 Minimal API**, **Entity Framework Core**, and **SQLite**. Includes JWT authentication with refresh token support, API versioning, and rate limiting.

---

## Tech Stack

- .NET 10 / ASP.NET Core Minimal API
- Entity Framework Core 10 + SQLite
- JWT Bearer Authentication (`Microsoft.AspNetCore.Authentication.JwtBearer`)
- BCrypt.Net-Next (password hashing)
- FluentValidation 12
- AutoMapper 16
- Swashbuckle (Swagger UI) 7

## Project Structure

```
TodoApi.Web/
├── Features/
│   ├── Auth/
│   │   ├── User.cs                        # User entity
│   │   ├── RefreshToken.cs                # Refresh token entity
│   │   ├── AuthService.cs                 # Register, login, refresh logic
│   │   ├── AuthEndpoints.cs               # Auth route handlers (v1 & v2)
│   │   ├── Dtos.cs                        # Auth DTOs (Register, Login, AuthResponse, RefreshRequest)
│   │   ├── Repositories/
│   │   │   ├── IUserRepository.cs
│   │   │   ├── UserRepository.cs
│   │   │   ├── IRefreshTokenRepository.cs
│   │   │   └── RefreshTokenRepository.cs
│   │   └── Validators/
│   │       ├── LoginDtoValidator.cs
│   │       ├── RegisterDtoValidator.cs
│   │       └── RefreshRequestDtoValidator.cs
│   └── Todos/
│       ├── Todo.cs                        # Entity model (with soft delete & audit trail)
│       ├── TodoDbContext.cs               # EF Core DbContext
│       ├── TodoService.cs                 # Business logic & queries
│       ├── TodoEndpoints.cs               # Minimal API route handlers (v1 & v2)
│       ├── Dtos.cs                        # Request/Response DTOs
│       ├── TodoQueryParams.cs             # Query params + PagedResult<T>
│       ├── Filters/
│       │   └── ValidationFilter.cs        # FluentValidation endpoint filter
│       ├── Mapping/
│       │   └── TodoProfile.cs             # AutoMapper profile
│       ├── Repositories/
│       │   ├── ITodoRepository.cs
│       │   └── TodoRepository.cs
│       ├── UnitOfWork/
│       │   ├── IUnitOfWork.cs
│       │   └── UnitOfWork.cs
│       └── Validators/
│           ├── CreateTodoDtoValidator.cs
│           └── UpdateTodoDtoValidator.cs
├── Extensions/
│   ├── ServiceCollectionExtensions.cs    # DI extension methods
│   ├── WebApplicationExtensions.cs       # Middleware & endpoint mapping
│   └── RateLimitingExtensions.cs         # Rate limiting configuration
├── ApiResponse.cs                         # Standardized response envelope
├── Program.cs                             # App entry point & middleware
├── appsettings.json
├── appsettings.Development.json
├── appsettings.Production.json
└── todos.db                               # SQLite database file (auto-generated)
```

---

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

On validation failure (400 Bad Request):
```json
{
  "success": false,
  "message": "Validation failed.",
  "errors": {
    "fieldName": ["error message 1", "error message 2"]
  }
}
```

For endpoints that don't return data (e.g. DELETE), the non-generic `ApiResponse` is used:
```json
{
  "success": true,
  "message": "Todo deleted successfully."
}
```

Unhandled exceptions are returned in [RFC 9457 ProblemDetails](https://www.rfc-editor.org/rfc/rfc9457) format (see [Error Handling](#error-handling)).

---

## Environment Variables & Configuration

The application reads configuration from `appsettings.json`, environment variables, and (optionally) User Secrets.

### Required Configuration

| Key | Description | Required |
|-----|-------------|----------|
| `Jwt__Secret` | Secret key for signing JWT tokens (min. 32 characters) | Yes |
| `Jwt__Issuer` | JWT token issuer | No (default: `todo-api`) |
| `Jwt__Audience` | JWT token audience | No (default: `todo-api`) |
| `Jwt__ExpirationHours` | Access token expiration in hours | No (default: `1`) |
| `Jwt__RefreshExpirationDays` | Refresh token expiration in days | No (default: `7`) |

> The application will **throw an exception** on startup if `Jwt__Secret` is not set.

---

### Option 1: User Secrets (Recommended for Development)

```bash
# Initialize (already configured in .csproj, skip if already done)
dotnet user-secrets init --project TodoApi.Web

# Set JWT secret
dotnet user-secrets set "Jwt:Secret" "your-super-secret-key-minimum-32-chars" --project TodoApi.Web

# Optional: override other values
dotnet user-secrets set "Jwt:Issuer" "todo-api" --project TodoApi.Web
dotnet user-secrets set "Jwt:Audience" "todo-api" --project TodoApi.Web
dotnet user-secrets set "Jwt:ExpirationHours" "1" --project TodoApi.Web
dotnet user-secrets set "Jwt:RefreshExpirationDays" "7" --project TodoApi.Web

# List all stored secrets
dotnet user-secrets list --project TodoApi.Web
```

---

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

---

### Option 3: appsettings.json (Not Recommended for Secrets)

```json
{
  "Jwt": {
    "Secret": "",
    "Issuer": "todo-api",
    "Audience": "todo-api",
    "ExpirationHours": 1,
    "RefreshExpirationDays": 7
  }
}
```

---

### Option 4: Docker / Docker Compose

```yaml
services:
  todoapi:
    build: .
    environment:
      - Jwt__Secret=your-super-secret-key-minimum-32-chars
      - Jwt__Issuer=todo-api
      - Jwt__Audience=todo-api
      - Jwt__RefreshExpirationDays=7
      - ASPNETCORE_ENVIRONMENT=Production
    ports:
      - "8080:8080"
```

---

### Configuration Priority

1. `appsettings.json`
2. `appsettings.{Environment}.json`
3. User Secrets (Development only)
4. Environment Variables
5. Command-line arguments

---

## Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)

### Steps

1. Clone the repository

```bash
git clone <repo-url>
cd TodoApi
```

2. Set the JWT Secret

```bash
dotnet user-secrets set "Jwt:Secret" "your-super-secret-key-minimum-32-chars" --project TodoApi.Web
```

3. Apply database migrations (skip if `todos.db` already exists)

```bash
dotnet ef database update --project TodoApi.Web
```

4. Run the application

```bash
dotnet run --project TodoApi.Web
```

App runs at: `http://localhost:5170`  
Swagger UI: `http://localhost:5170/swagger` (Development only)

---

## API Versioning

This API exposes two versions with identical functionality. The key difference is that **v2 enforces rate limiting**.

| Version | Base URL | Rate Limiting |
|---------|----------|---------------|
| v1 | `/api/v1` | No |
| v2 | `/api/v2` | Yes — 100 req/min per IP |

Both versions are documented in Swagger UI under separate tabs.

---

## Rate Limiting (v2 only)

API v2 applies a **Fixed Window** rate limit per IP address:

- Limit: **100 requests per minute**
- Rejection status: `429 Too Many Requests`
- Response includes `Retry-After: 60` header

```json
{
  "success": false,
  "message": "Too many requests. Maximum 100 requests per minute per IP. Please try again after 1 minute."
}
```

---

## Authentication

This API uses **JWT Bearer Authentication** with **Refresh Token** support. All `/todos` endpoints require a valid access token.

There is no default user — you must register first.

### Register

```http
POST /api/v1/auth/register
Content-Type: application/json

{
  "username": "yourname",
  "password": "yourpassword"
}
```

Constraints: username 3–50 chars, password min. 8 chars.

Response `201 Created`:
```json
{
  "success": true,
  "data": {
    "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "refreshToken": "base64-encoded-refresh-token..."
  },
  "message": "Registration successful."
}
```

### Login

```http
POST /api/v1/auth/login
Content-Type: application/json

{
  "username": "yourname",
  "password": "yourpassword"
}
```

Response `200 OK`:
```json
{
  "success": true,
  "data": {
    "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "refreshToken": "base64-encoded-refresh-token..."
  },
  "message": "Login successful."
}
```

Response `401 Unauthorized` (wrong credentials):
```json
{
  "success": false,
  "message": "Invalid username or password."
}
```

### Refresh Token

```http
POST /api/v1/auth/refresh
Content-Type: application/json

{
  "refreshToken": "base64-encoded-refresh-token..."
}
```

Response `200 OK`:
```json
{
  "success": true,
  "data": {
    "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "refreshToken": "new-base64-encoded-refresh-token..."
  },
  "message": "Token refreshed successfully."
}
```

Response `401 Unauthorized` (invalid/expired token):
```json
{
  "success": false,
  "message": "Refresh token is invalid or has expired."
}
```

> When a refresh token is used, it is immediately **revoked** and a new one is issued.

### Using the Access Token

```http
Authorization: Bearer <accessToken>
```

In Swagger UI, click **Authorize** and enter `Bearer <accessToken>`.

---

## API Endpoints

Base URL: `/api/v1` or `/api/v2`

| Method | Endpoint               | Auth | Description                              |
|--------|------------------------|------|------------------------------------------|
| POST   | `/auth/register`       | No   | Register a new user                      |
| POST   | `/auth/login`          | No   | Login and retrieve access + refresh token|
| POST   | `/auth/refresh`        | No   | Refresh access token using refresh token |
| GET    | `/todos`               | Yes  | List todos (pagination, search, sorting) |
| GET    | `/todos/{id}`          | Yes  | Get a todo by ID                         |
| POST   | `/todos`               | Yes  | Create a new todo                        |
| PUT    | `/todos/{id}`          | Yes  | Update a todo                            |
| DELETE | `/todos/{id}`          | Yes  | Soft-delete a todo                       |

> Each user can only access their own todos. Todos are scoped to the authenticated user.

---

### GET /todos

Query Parameters:

| Parameter | Type   | Default | Description                                                    |
|-----------|--------|---------|----------------------------------------------------------------|
| `page`    | int    | 1       | Page number (min: 1)                                           |
| `size`    | int    | 10      | Items per page (1–50)                                          |
| `search`  | string | -       | Filter by title (case-insensitive)                             |
| `sort`    | string | -       | Sort by `title`, `createdat`, `iscompleted`, append `:desc` for descending |

Example: `GET /api/v1/todos?page=1&size=5&search=milk&sort=createdat:desc`

Response `200 OK`:
```json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": 1,
        "title": "Buy milk",
        "isCompleted": false,
        "createdAt": "2026-03-25T00:00:00Z",
        "createdBy": 1,
        "updatedBy": 1,
        "updatedAt": "2026-03-25T00:00:00Z"
      }
    ],
    "page": 1,
    "size": 10,
    "totalItems": 1,
    "totalPages": 1,
    "hasNext": false,
    "hasPrevious": false
  },
  "message": null
}
```

---

### GET /todos/{id}

Response `200 OK`:
```json
{
  "success": true,
  "data": {
    "id": 1,
    "title": "Buy milk",
    "isCompleted": false,
    "createdAt": "2026-03-25T00:00:00Z",
    "createdBy": 1,
    "updatedBy": 1,
    "updatedAt": "2026-03-25T00:00:00Z"
  },
  "message": null
}
```

Response `404 Not Found`:
```json
{
  "success": false,
  "message": "Todo with ID 1 not found."
}
```

---

### POST /todos

Request body:
```json
{ "title": "Buy milk" }
```

Constraints: title 1–200 characters.

Response `201 Created`:
```json
{
  "success": true,
  "data": {
    "id": 3,
    "title": "Buy milk",
    "isCompleted": false,
    "createdAt": "2026-03-25T00:00:00Z",
    "createdBy": 1,
    "updatedBy": 1,
    "updatedAt": "2026-03-25T00:00:00Z"
  },
  "message": "Todo created successfully."
}
```

---

### PUT /todos/{id}

All fields are optional (partial update):
```json
{
  "title": "Buy UHT milk",
  "isCompleted": true
}
```

Constraints: title 1–200 characters (if provided).

Response `200 OK`:
```json
{
  "success": true,
  "data": {
    "id": 1,
    "title": "Buy UHT milk",
    "isCompleted": true,
    "createdAt": "2026-03-25T00:00:00Z",
    "createdBy": 1,
    "updatedBy": 1,
    "updatedAt": "2026-03-25T12:00:00Z"
  },
  "message": "Todo updated successfully."
}
```

Response `404 Not Found`:
```json
{
  "success": false,
  "message": "Todo with ID 1 not found."
}
```

Response `400 Bad Request` (empty title):
```json
{
  "success": false,
  "message": "Title must not be empty."
}
```

---

### DELETE /todos/{id}

Performs a **soft delete** — the record is marked as deleted (`IsDeleted = true`) and excluded from all queries, but not physically removed from the database.

Response `200 OK`:
```json
{
  "success": true,
  "message": "Todo deleted successfully."
}
```

Response `404 Not Found`:
```json
{
  "success": false,
  "message": "Todo with ID 1 not found."
}
```

---

## Error Handling

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

In Development mode, the response also includes `exceptionType` to aid debugging.

HTTP error responses (4xx/5xx without an unhandled exception) are returned as `ApiResponse` with a descriptive message.

---

## Database Migrations

The project uses EF Core migrations with SQLite.

```bash
# Apply all pending migrations
dotnet ef database update --project TodoApi.Web

# Add a new migration
dotnet ef migrations add <MigrationName> --project TodoApi.Web
```

Applied migrations:

| Migration | Description |
|-----------|-------------|
| `20260325031127_AddUserOwnershipToTodos` | Add `UserId` FK to todos |
| `20260325073704_AddSoftDeleteToTodo` | Add `IsDeleted` flag |
| `20260325082755_AddAuditTrailToTodo` | Add `CreatedBy`, `UpdatedBy`, `UpdatedAt` |
