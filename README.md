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
- Serilog (structured logging — Console + rolling JSON file)

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
│           ├── CreateTodoDtoV2Validator.cs
│           └── UpdateTodoDtoValidator.cs
├── Extensions/
│   ├── ServiceCollectionExtensions.cs    # DI extension methods
│   ├── WebApplicationExtensions.cs       # Middleware & endpoint mapping
│   ├── RateLimitingExtensions.cs         # Rate limiting configuration
│   ├── LoggingExtensions.cs              # Correlation ID middleware
│   └── JsonArrayFormatter.cs             # Custom Serilog sink (rolling JSON array file)
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
| `Jwt__ExpirationHours` | Access token expiration in hours | No (default: `24`) |
| `Jwt__RefreshExpirationDays` | Refresh token expiration in days | No (default: `7`) |

> The application will **throw an exception** on startup if `Jwt__Secret` is not set.

> `Jwt__RefreshExpirationDays` is only configurable via User Secrets or environment variables — it is not present in `appsettings.json`.

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
    "ExpirationHours": 24,
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

## CORS Configuration

CORS origins are configured per environment. Development origins are defined in `appsettings.Development.json`. Production and staging origins must be set via environment variables — the app will **throw an exception** on startup if no origins are configured in non-development environments.

### Development

Origins are read from `appsettings.Development.json`:

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

In development, credentials are not required (`AllowCredentials` is not set).

### Production / Staging

Set origins via environment variables using ASP.NET Core's double-underscore array syntax:

**Linux / macOS:**
```bash
export Cors__AllowedOrigins__0=https://todo-app.yourdomain.com
export Cors__AllowedOrigins__1=https://staging-todo.yourdomain.com
```

**Windows PowerShell:**
```powershell
$env:Cors__AllowedOrigins__0 = "https://todo-app.yourdomain.com"
$env:Cors__AllowedOrigins__1 = "https://staging-todo.yourdomain.com"
```

**Docker Compose:**
```yaml
environment:
  - Cors__AllowedOrigins__0=https://todo-app.yourdomain.com
  - Cors__AllowedOrigins__1=https://staging-todo.yourdomain.com
```

**Kubernetes:**
```yaml
env:
  - name: Cors__AllowedOrigins__0
    value: "https://todo-app.yourdomain.com"
  - name: Cors__AllowedOrigins__1
    value: "https://staging-todo.yourdomain.com"
```

In production, `AllowCredentials()` is enabled — origins must use `https` and cannot be wildcards.

### Testing CORS

```bash
# Preflight request — origin diizinkan
curl -X OPTIONS http://localhost:5170/api/v1/todos \
  -H "Origin: http://localhost:3000" \
  -H "Access-Control-Request-Method: GET" \
  -H "Access-Control-Request-Headers: Authorization" \
  -v

# Expected response headers:
# Access-Control-Allow-Origin: http://localhost:3000
# Access-Control-Allow-Methods: GET
# Access-Control-Allow-Headers: Authorization
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

## Docker

### Prerequisites

- [Docker](https://docs.docker.com/get-docker/) installed

### Build & Run

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

Create a `docker-compose.yml` at the repo root:

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

### Health Check

The container exposes health endpoints used by Docker and Kubernetes:

| Endpoint | Purpose |
|----------|---------|
| `GET /api/v2/health/liveness` | Liveness probe — is the app running? |
| `GET /api/v2/health/readiness` | Readiness probe — is the DB reachable? |
| `GET /api/v2/health` | Combined health status |

---

## Kubernetes

Manifests are located in the `k8s/` directory.

```
k8s/
├── deployment.yaml   # 2 replicas, resource limits, liveness/readiness probes
├── service.yaml      # ClusterIP service on port 80 → 8080
└── ingress.yaml      # NGINX ingress for /api/v1, /api/v2, /swagger
```

### Deploy

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

### Update image

Edit `k8s/deployment.yaml` and update the `image` field:

```yaml
image: yourusername/todoapi:v1.0.0
```

Then re-apply:

```bash
kubectl apply -f k8s/deployment.yaml -n todoapi-dev
```

### Resource Limits

| Resource | Request | Limit |
|----------|---------|-------|
| CPU | 200m | 500m |
| Memory | 256Mi | 512Mi |

---

## API Versioning

This API exposes two versions. The key differences are that **v2 enforces rate limiting** and supports additional fields like `dueDate` on todo creation.

| Version | Base URL | Rate Limiting | DueDate Support |
|---------|----------|---------------|-----------------|
| v1 | `/api/v1` | No | No |
| v2 | `/api/v2` | Yes — 100 req/min per IP | Yes |

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

Request body (v1):
```json
{ "title": "Buy milk" }
```

Request body (v2 — supports optional `dueDate`):
```json
{
  "title": "Buy milk",
  "dueDate": "2026-04-01T10:00:00+07:00"
}
```

Constraints:
- `title`: 1–200 characters, required
- `dueDate` (v2 only): must be in the future, accepts any timezone offset (e.g. `+07:00`), stored as UTC

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
    "updatedAt": "2026-03-25T00:00:00Z",
    "dueDate": "2026-04-01T03:00:00Z"
  },
  "message": "Todo created successfully."
}
```

Response `400 Bad Request` (due date in the past):
```json
{
  "success": false,
  "message": "Validation failed.",
  "errors": {
    "DueDate": ["Due date must be in the future."]
  }
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

## Logging

This API uses **Serilog** for structured logging with two sinks:

- Console — human-readable output during development
- File — rolling JSON logs written to `logs/log-{date}.json` (excluded from git), using a custom `JsonArrayFileSink` that writes structured log entries as a valid JSON array

Log level is configured via `appsettings.json` under the `Serilog` key:

```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "Microsoft.AspNetCore": "Warning",
        "Microsoft.EntityFrameworkCore": "Warning"
      }
    }
  }
}
```

### Correlation ID

Every request is automatically assigned a `CorrelationId` (UUID v4). If the client sends an `X-Correlation-ID` header, that value is used instead. The ID is:

- Propagated through all Serilog log entries for the duration of the request
- Returned to the client via the `X-Correlation-ID` response header

### Request / Response Body Logging (v2 only)

API v2 uses `UseRequestResponseBodyLogging` middleware to capture request and response bodies:

- Request body is captured for `application/json` content types and attached to the Serilog diagnostic context
- Response body is only captured when the status code is `>= 400` (errors only), to avoid logging large successful payloads
- Sensitive fields (`password`, `secret`, `token`, `refreshToken`, `key`, `credential`, `auth`) in the response body are automatically masked before being written to logs

### Request Logging (v2 only)

API v2 logs each request/response via `UseSerilogRequestLogging`, including:

- Method, path, status code, elapsed time
- `CorrelationId` and `UserId` (or `anonymous` if unauthenticated)
- `RequestBody` (JSON payloads only)
- `ResponseBody` (error responses only, with sensitive data masked)

### Custom JSON File Sink

Log files are written using a custom `JsonArrayFileSink` that produces valid JSON arrays (not newline-delimited JSON). Each log entry is a structured object with `Timestamp`, `Level`, `Message`, `Exception` (if any), and `Properties`. Files roll daily and are named `log-{yyyyMMdd}.json`.

---

## Background Services

### ReminderService

A hosted background service that runs every **5 minutes** and checks for overdue todos. A todo is considered overdue when:
- `IsCompleted = false`
- `IsDeleted = false`
- `DueDate` is set and has passed (compared in UTC)

Overdue todos are logged as warnings:

```
REMINDER - OVERDUE TODO: Id=5, User=john, Title='Buy milk', DueDate=2026-04-01 03:00 UTC
```

#### Testing Overdue Todos

Since the API validates that `dueDate` must be in the future, insert a test record directly via SQLite:

```bash
sqlite3 TodoApi.Web/todos.db "INSERT INTO Todos (Title, IsCompleted, CreatedAt, CreatedBy, UpdatedBy, UpdatedAt, IsDeleted, UserId, DueDate) VALUES ('Overdue Test', 0, datetime('now'), 1, 1, datetime('now'), 0, 1, datetime('now', '-1 hour'));"
```

The warning will appear in logs within the next 5-minute check cycle. To speed up testing, temporarily change `_interval` in `ReminderService.cs` to `TimeSpan.FromSeconds(10)`.

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
| `20260331131832_AddDueDateToTodo` | Add `DueDate` (nullable) to todos |
