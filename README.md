# TodoApi

A simple REST API for todo management built with **ASP.NET Core 10 Minimal API**, **Entity Framework Core**, and **SQLite**. Includes JWT authentication.

---

## Tech Stack

- .NET 10 / ASP.NET Core Minimal API
- Entity Framework Core 10 + SQLite
- JWT Bearer Authentication (`Microsoft.AspNetCore.Authentication.JwtBearer`)
- FluentValidation
- AutoMapper
- Swashbuckle (Swagger UI)

## Project Structure

```
TodoApi.Web/
├── Features/
│   └── Todos/
│       ├── Todo.cs                    # Entity model
│       ├── TodoDbContext.cs           # EF Core DbContext + seed data
│       ├── TodoService.cs             # Business logic & queries
│       ├── TodoEndpoints.cs           # Minimal API route handlers
│       ├── Dtos.cs                    # Request DTOs (Create, Update)
│       ├── TodoQueryParams.cs         # Query params + PagedResult<T>
│       ├── Filters/
│       │   └── ValidationFilter.cs   # FluentValidation endpoint filter
│       ├── Mapping/
│       │   └── TodoProfile.cs        # AutoMapper profile
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
│   └── ServiceCollectionExtensions.cs  # DI extension methods
├── Program.cs                           # App entry point & middleware
├── appsettings.json
├── appsettings.Development.json
├── appsettings.Production.json
└── todos.db                             # SQLite database file (auto-generated)
```

---

## Environment Variables & Configuration

The application reads configuration from `appsettings.json`, environment variables, and (optionally) User Secrets.

### Required Configuration

| Key | Description | Required |
|-----|-------------|----------|
| `Jwt__Secret` | Secret key for signing JWT tokens (min. 32 characters) | Yes |
| `Jwt__Issuer` | JWT token issuer | No (default: `todo-api`) |
| `Jwt__Audience` | JWT token audience | No (default: `todo-api`) |
| `Jwt__ExpirationHours` | Token expiration duration in hours | No (default: `24`) |

> The application will **throw an exception** on startup if `Jwt__Secret` is not set.

---

### Option 1: User Secrets (Recommended for Development)

User Secrets are not committed to version control, making them ideal for local development.

```bash
# Initialize (already configured in .csproj, skip if already done)
dotnet user-secrets init --project TodoApi.Web

# Set JWT secret
dotnet user-secrets set "Jwt:Secret" "your-super-secret-key-minimum-32-chars" --project TodoApi.Web

# Optional: override other values
dotnet user-secrets set "Jwt:Issuer" "todo-api" --project TodoApi.Web
dotnet user-secrets set "Jwt:Audience" "todo-api" --project TodoApi.Web
dotnet user-secrets set "Jwt:ExpirationHours" "24" --project TodoApi.Web

# List all stored secrets
dotnet user-secrets list --project TodoApi.Web
```

---

### Option 2: Environment Variables

ASP.NET Core reads environment variables using `__` (double underscore) as a separator instead of `:`.

**Linux / macOS:**
```bash
export Jwt__Secret="your-super-secret-key-minimum-32-chars"
export Jwt__Issuer="todo-api"
export Jwt__Audience="todo-api"
export Jwt__ExpirationHours="24"

dotnet run --project TodoApi.Web
```

**Windows (Command Prompt):**
```cmd
set Jwt__Secret=your-super-secret-key-minimum-32-chars
set Jwt__Issuer=todo-api
dotnet run --project TodoApi.Web
```

**Windows (PowerShell):**
```powershell
$env:Jwt__Secret = "your-super-secret-key-minimum-32-chars"
$env:Jwt__Issuer = "todo-api"
dotnet run --project TodoApi.Web
```

---

### Option 3: appsettings.json (Not Recommended for Secrets)

For non-sensitive values, you can set them directly in `appsettings.json`. **Do not store secrets here if the file is committed to a repository.**

```json
{
  "Jwt": {
    "Secret": "",
    "Issuer": "todo-api",
    "Audience": "todo-api",
    "ExpirationHours": 24
  }
}
```

---

### Option 4: Docker / Docker Compose

```yaml
# docker-compose.yml
services:
  todoapi:
    build: .
    environment:
      - Jwt__Secret=your-super-secret-key-minimum-32-chars
      - Jwt__Issuer=todo-api
      - Jwt__Audience=todo-api
      - ASPNETCORE_ENVIRONMENT=Production
    ports:
      - "8080:8080"
```

---

### Configuration Priority

ASP.NET Core reads configuration in the following order (later sources override earlier ones):

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

2. Set the JWT Secret (choose one of the options above, example using User Secrets)

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

Swagger UI available at: `http://localhost:5170/swagger` (Development mode only)

---

## Authentication

This API uses **JWT Bearer Authentication**. All `/api/todos` endpoints require a valid token.

### Login

```http
POST /api/auth/login
Content-Type: application/json

{
  "username": "admin",
  "password": "password"
}
```

Response:
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
}
```

### Using the Token

Include the token in the `Authorization` header for every request to `/api/todos`:

```http
Authorization: Bearer <token>
```

In Swagger UI, click the **Authorize** button and enter the token in the format `Bearer <token>`.

---

## API Endpoints

Base URL: `/api`

| Method | Endpoint              | Auth | Description                              |
|--------|-----------------------|------|------------------------------------------|
| POST   | `/api/auth/login`     | No   | Login and retrieve a JWT token           |
| GET    | `/api/todos`          | Yes  | List todos (pagination, search, sorting) |
| GET    | `/api/todos/{id}`     | Yes  | Get a todo by ID                         |
| POST   | `/api/todos`          | Yes  | Create a new todo                        |
| PUT    | `/api/todos/{id}`     | Yes  | Update a todo                            |
| DELETE | `/api/todos/{id}`     | Yes  | Delete a todo                            |

### GET /api/todos — Query Parameters

| Parameter | Type   | Default | Description                                              |
|-----------|--------|---------|----------------------------------------------------------|
| `page`    | int    | 1       | Page number (min: 1)                                     |
| `size`    | int    | 10      | Items per page (1–50)                                    |
| `search`  | string | -       | Filter by title (case-insensitive)                       |
| `sort`    | string | -       | Sort by `title`, `createdat`, `iscompleted` + `:desc`    |

Example: `GET /api/todos?page=1&size=5&search=milk&sort=createdat:desc`

### Paginated Response

```json
{
  "items": [...],
  "page": 1,
  "size": 10,
  "totalItems": 25,
  "totalPages": 3,
  "hasNext": true,
  "hasPrevious": false
}
```

### POST /api/todos — Request Body

```json
{
  "title": "Buy milk"
}
```

### PUT /api/todos/{id} — Request Body

All fields are optional (partial update):

```json
{
  "title": "Buy UHT milk",
  "isCompleted": true
}
```

---

## Error Handling

All errors are returned in [RFC 9457 ProblemDetails](https://www.rfc-editor.org/rfc/rfc9457) format:

```json
{
  "type": "https://httpstatuses.com/500",
  "title": "An internal server error occurred",
  "status": 500,
  "detail": "...",
  "instance": "POST /api/todos",
  "traceId": "...",
  "timestamp": "2026-03-23T00:00:00Z"
}
```

In Development mode, the response also includes `exceptionType` to aid debugging.

---

## Seed Data

The database is initialized with 2 records:

| ID | Title     | IsCompleted |
|----|-----------|-------------|
| 1  | Buy milk  | false       |
| 2  | Call mom  | true        |
