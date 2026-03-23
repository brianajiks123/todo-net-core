# TodoApi

A simple REST API for todo management built with **ASP.NET Core 10 Minimal API**, **Entity Framework Core**, and **SQLite**.

---

## Tech Stack

- .NET 10 / ASP.NET Core Minimal API
- Entity Framework Core 10 + SQLite
- Swashbuckle (Swagger UI)
- Built-in Validation (`AddValidation`)

## Project Structure

```
TodoApi.Web/
├── Features/
│   └── Todos/
│       ├── Todo.cs              # Entity model
│       ├── TodoDbContext.cs     # EF Core DbContext + seed data
│       ├── TodoService.cs       # Business logic & queries
│       ├── TodoEndpoints.cs     # Minimal API route handlers
│       ├── Dtos.cs              # Request DTOs (Create, Update)
│       └── TodoQueryParams.cs   # Query params + PagedResult<T>
├── Extensions/
│   └── ServiceCollectionExtensions.cs  # DI extension methods
├── Program.cs                   # App entry point & middleware
└── todos.db                     # SQLite database file
```

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

2. Apply database migrations (skip if `todos.db` already exists)

```bash
dotnet ef database update --project TodoApi.Web
```

3. Run the application

```bash
dotnet run --project TodoApi.Web
```

App runs at: `http://localhost:5170`

Swagger UI available at: `http://localhost:5170/swagger` (Development mode only)

---

## API Endpoints

Base URL: `/api/todos`

| Method | Endpoint          | Description                              |
|--------|-------------------|------------------------------------------|
| GET    | `/api/todos`      | List todos (pagination, search, sorting) |
| GET    | `/api/todos/{id}` | Get a todo by ID                         |
| POST   | `/api/todos`      | Create a new todo                        |
| PUT    | `/api/todos/{id}` | Update a todo                            |
| DELETE | `/api/todos/{id}` | Delete a todo                            |

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
