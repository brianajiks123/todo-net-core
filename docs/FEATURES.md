# Features

## Authentication

JWT Bearer Authentication with Refresh Token support. All `/todos` endpoints require a valid access token. There is no default user — register first.

### Register

```http
POST /api/v1/auth/register
Content-Type: application/json

{
  "username": "yourname",
  "password": "yourpassword"
}
```

Constraints: username 3–50 chars, password min. 8 chars. Returns `201 Created` with `accessToken` and `refreshToken`.

### Login

```http
POST /api/v1/auth/login
Content-Type: application/json

{
  "username": "yourname",
  "password": "yourpassword"
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

> When a refresh token is used, it is immediately revoked and a new one is issued.

### Using the Access Token

```http
Authorization: Bearer <accessToken>
```

In Swagger UI, click **Authorize** and enter `Bearer <accessToken>`.

## API Endpoints

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| POST | `/auth/register` | No | Register a new user |
| POST | `/auth/login` | No | Login and get tokens |
| POST | `/auth/refresh` | No | Refresh access token |
| GET | `/todos` | Yes | List todos (pagination, search, sort) |
| GET | `/todos/{id}` | Yes | Get a todo by ID |
| POST | `/todos` | Yes | Create a new todo |
| PUT | `/todos/{id}` | Yes | Update a todo |
| DELETE | `/todos/{id}` | Yes | Soft-delete a todo |

> Each user can only access their own todos.

## Todo Endpoints

### GET /todos

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `page` | int | 1 | Page number (min: 1) |
| `size` | int | 10 | Items per page (1–50) |
| `search` | string | - | Filter by title (case-insensitive) |
| `sort` | string | - | Sort by `title`, `createdat`, `iscompleted` — append `:desc` for descending |

Example: `GET /api/v1/todos?page=1&size=5&search=milk&sort=createdat:desc`

### POST /todos

v1 — title only:
```json
{ "title": "Buy milk" }
```

v2 — supports optional `dueDate`:
```json
{
  "title": "Buy milk",
  "dueDate": "2026-04-01T10:00:00+07:00"
}
```

`dueDate` must be in the future. Accepts any timezone offset, stored as UTC.

### DELETE /todos/{id}

Performs a **soft delete** — record is marked `IsDeleted = true` and excluded from all queries, but not physically removed.

## Rate Limiting (v2 only)

Fixed Window rate limit per IP:

- Limit: **100 requests per minute**
- Rejection: `429 Too Many Requests` with `Retry-After: 60` header

## Logging

Serilog with two sinks:
- Console — human-readable output
- File — rolling JSON logs at `logs/log-{date}.json` via custom `JsonArrayFileSink`

### Correlation ID

Every request gets a `CorrelationId` (UUID v4). If the client sends `X-Correlation-ID`, that value is used. The ID is propagated through all log entries and returned via the `X-Correlation-ID` response header.

### Request / Response Logging (v2 only)

- Request body captured for `application/json` content types
- Response body captured only for status `>= 400` (errors only)
- Sensitive fields (`password`, `secret`, `token`, `refreshToken`, `key`, `credential`, `auth`) are automatically masked in logs

## Background Services

### ReminderService

Periodically checks for overdue todos. A todo is overdue when:
- `IsCompleted = false`
- `IsDeleted = false`
- `DueDate` is set and has passed (UTC)

Default check interval: `TimeSpan.FromSeconds(10)` (dev) — change to `TimeSpan.FromMinutes(5)` for production.

Overdue todos are logged as warnings:
```
REMINDER - OVERDUE TODO: Id=5, User=john, Title='Buy milk', DueDate=2026-04-01 03:00 UTC
```

#### Testing Overdue Todos

Since the API validates `dueDate` must be in the future, insert a test record directly:

```bash
sqlite3 TodoApi.Web/todos.db "INSERT INTO Todos (Title, IsCompleted, CreatedAt, CreatedBy, UpdatedBy, UpdatedAt, IsDeleted, UserId, DueDate) VALUES ('Overdue Test', 0, datetime('now'), 1, 1, datetime('now'), 0, 1, datetime('now', '-1 hour'));"
```

## Testing

Tests are in `TodoApi.Web.Tests/` using xUnit.

| Package | Purpose |
|---------|---------|
| xUnit 2 | Test runner & assertions |
| Moq | Mocking dependencies for unit tests |
| `Microsoft.AspNetCore.Mvc.Testing` | `WebApplicationFactory` for integration tests |
| `Microsoft.EntityFrameworkCore.InMemory` | In-memory DB for integration tests |

### Unit Tests — `TodoServiceTests`

| Test | Description |
|------|-------------|
| `CreateV2_Should_Create_Todo_And_Return_ResponseDto` | Verifies todo is created and `SaveChangesAsync` called once |
| `GetById_Should_Return_Null_When_Todo_Not_Found` | Verifies `null` returned when todo not found |

### Integration Tests — `TodoEndpointsIntegrationTests`

| Test | Description |
|------|-------------|
| `HealthCheck_Returns_Healthy_Status` | `GET /api/v2/health` returns `Healthy` |
| `Full_Auth_And_Todo_Flow_Works` | Register → Login → Create Todo with JWT |

`CustomWebApplicationFactory` replaces SQLite with EF Core InMemory DB and patches JWT signing key for consistent token validation.
