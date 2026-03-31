using Microsoft.AspNetCore.Mvc;
using TodoApi.Web.Features.Todos.Filters;
using System.Security.Claims;

namespace TodoApi.Web;

public static class TodoEndpoints
{
    // Version 1
    public static void MapTodoEndpointsV1(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/todos")
            .WithTags("Todos v1")
            .RequireAuthorization();

        // GET /api/v1/todos
        group.MapGet("/",
            async (ClaimsPrincipal user, [AsParameters] TodoQueryParams query, [FromServices] TodoService service) =>
            {
                var userId = int.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var result = await service.GetPaged(query, userId);
                return Results.Ok(ApiResponse<PagedResult<TodoResponseDto>>.Ok(result));
            })
            .WithName("GetTodosV1")
            .WithSummary("Get paginated todo list (v1)");

        // GET /api/v1/todos/{id}
        group.MapGet("/{id:int}",
            async (int id, ClaimsPrincipal user, [FromServices] TodoService service) =>
            {
                var userId = int.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var todo = await service.GetById(id, userId);
                return todo is not null
                    ? Results.Ok(ApiResponse<TodoResponseDto>.Ok(todo))
                    : Results.NotFound(ApiResponse.Fail($"Todo with ID {id} not found."));
            })
            .WithName("GetTodoByIdV1")
            .WithSummary("Get todo by ID (v1)");

        // POST /api/v1/todos
        group.MapPost("/",
            async (CreateTodoDto dto, ClaimsPrincipal user, [FromServices] TodoService service) =>
            {
                var userId = int.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var created = await service.Create(dto.Title.Trim(), userId);
                return Results.Created(
                    $"/api/v1/todos/{created.Id}",
                    ApiResponse<TodoResponseDto>.Ok(created, "Todo created successfully."));
            })
            .AddEndpointFilter<ValidationFilter<CreateTodoDto>>()
            .WithName("CreateTodoV1")
            .WithSummary("Create a new todo (v1)");

        // PUT /api/v1/todos/{id}
        group.MapPut("/{id:int}",
            async (int id, UpdateTodoDto dto, ClaimsPrincipal user, [FromServices] TodoService service) =>
            {
                var userId = int.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var result = await service.Update(id, dto.Title?.Trim(), dto.IsCompleted, userId);
                return result.Result switch
                {
                    UpdateResult.Success      => Results.Ok(ApiResponse<TodoResponseDto>.Ok(result.Data!, "Todo updated successfully.")),
                    UpdateResult.NotFound     => Results.NotFound(ApiResponse.Fail($"Todo with ID {id} not found.")),
                    UpdateResult.InvalidTitle => Results.BadRequest(ApiResponse.Fail("Title must not be empty.")),
                    _                         => Results.Json(ApiResponse.Fail("An internal error occurred."), statusCode: 500)
                };
            })
            .AddEndpointFilter<ValidationFilter<UpdateTodoDto>>()
            .WithName("UpdateTodoV1")
            .WithSummary("Update a todo (v1)");

        // DELETE /api/v1/todos/{id}
        group.MapDelete("/{id:int}",
            async (int id, ClaimsPrincipal user, [FromServices] TodoService service) =>
            {
                var userId = int.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var deleted = await service.Delete(id, userId);
                return deleted
                    ? Results.Ok(ApiResponse.Ok("Todo deleted successfully."))
                    : Results.NotFound(ApiResponse.Fail($"Todo with ID {id} not found."));
            })
            .WithName("DeleteTodoV1")
            .WithSummary("Delete a todo (v1)");
    }

    // Version 2
    public static void MapTodoEndpointsV2(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/todos")
            .WithTags("Todos v2")
            .RequireAuthorization();

        // GET /api/v2/todos
        group.MapGet("/",
            async (ClaimsPrincipal user, [AsParameters] TodoQueryParams query, [FromServices] TodoService service) =>
            {
                var userId = int.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var result = await service.GetPaged(query, userId);
                return Results.Ok(ApiResponse<PagedResult<TodoResponseDto>>.Ok(result));
            })
            .WithName("GetTodosV2")
            .WithSummary("Get paginated todo list (v2)");

        // GET /api/v2/todos/{id}
        group.MapGet("/{id:int}",
            async (int id, ClaimsPrincipal user, [FromServices] TodoService service) =>
            {
                var userId = int.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var todo = await service.GetById(id, userId);
                return todo is not null
                    ? Results.Ok(ApiResponse<TodoResponseDto>.Ok(todo))
                    : Results.NotFound(ApiResponse.Fail($"Todo with ID {id} not found."));
            })
            .WithName("GetTodoByIdV2")
            .WithSummary("Get todo by ID (v2)");

        // POST /api/v2/todos
        group.MapPost("/",
            async (CreateTodoDtoV2 dto, ClaimsPrincipal user, [FromServices] TodoService service) =>
            {
                var userId = int.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var created = await service.CreateV2(dto, userId);
                return Results.Created(
                    $"/api/v2/todos/{created.Id}",
                    ApiResponse<TodoResponseDto>.Ok(created, "Todo created successfully."));
            })
            .AddEndpointFilter<ValidationFilter<CreateTodoDtoV2>>()
            .WithName("CreateTodoV2")
            .WithSummary("Create a new todo (v2)");

        // PUT /api/v2/todos/{id}
        group.MapPut("/{id:int}",
            async (int id, UpdateTodoDto dto, ClaimsPrincipal user, [FromServices] TodoService service) =>
            {
                var userId = int.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var result = await service.Update(id, dto.Title?.Trim(), dto.IsCompleted, userId);
                return result.Result switch
                {
                    UpdateResult.Success      => Results.Ok(ApiResponse<TodoResponseDto>.Ok(result.Data!, "Todo updated successfully.")),
                    UpdateResult.NotFound     => Results.NotFound(ApiResponse.Fail($"Todo with ID {id} not found.")),
                    UpdateResult.InvalidTitle => Results.BadRequest(ApiResponse.Fail("Title must not be empty.")),
                    _                         => Results.Json(ApiResponse.Fail("An internal error occurred."), statusCode: 500)
                };
            })
            .AddEndpointFilter<ValidationFilter<UpdateTodoDto>>()
            .WithName("UpdateTodoV2")
            .WithSummary("Update a todo (v2)");

        // DELETE /api/v2/todos/{id}
        group.MapDelete("/{id:int}",
            async (int id, ClaimsPrincipal user, [FromServices] TodoService service) =>
            {
                var userId = int.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var deleted = await service.Delete(id, userId);
                return deleted
                    ? Results.Ok(ApiResponse.Ok("Todo deleted successfully."))
                    : Results.NotFound(ApiResponse.Fail($"Todo with ID {id} not found."));
            })
            .WithName("DeleteTodoV2")
            .WithSummary("Delete a todo (v2)");
    }
}
