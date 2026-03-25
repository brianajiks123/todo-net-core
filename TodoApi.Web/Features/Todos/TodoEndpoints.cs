using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using TodoApi.Web.Features.Todos;
using TodoApi.Web.Features.Todos.Filters;
using System.Security.Claims;

namespace TodoApi.Web;

public static class TodoEndpoints
{
    public static void MapTodoEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/todos")
            .WithTags("Todos")
            .RequireAuthorization();

        // GET /api/todos (pagination + filter + sort)
        group.MapGet("/", 
            async (ClaimsPrincipal user, [AsParameters] TodoQueryParams query, [FromServices] TodoService service) => 
            {
                var userId = int.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
                return Results.Ok(await service.GetPaged(query, userId));
            })
            .WithName("GetTodos")
            .WithSummary("Daftar todo dengan pagination, search & sorting");

        // GET /api/todos/{id}
        group.MapGet("/{id:int}", 
            async (int id, ClaimsPrincipal user, [FromServices] TodoService service) =>
            {
                var userId = int.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var todo = await service.GetById(id, userId);
                return todo is not null
                    ? Results.Ok(todo)
                    : Results.Problem(detail: $"Todo dengan ID {id} tidak ditemukan.", statusCode: 404);
            });

        // POST /api/todos
        group.MapPost("/", 
            async (CreateTodoDto dto, ClaimsPrincipal user, [FromServices] TodoService service) =>
            {
                var userId = int.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var created = await service.Create(dto.Title.Trim(), userId);
                return Results.Created($"/api/todos/{created.Id}", created);
            })
            .AddEndpointFilter<ValidationFilter<CreateTodoDto>>();

        // PUT /api/todos/{id}
        group.MapPut("/{id:int}", 
            async (int id, UpdateTodoDto dto, ClaimsPrincipal user, [FromServices] TodoService service) =>
            {
                var userId = int.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var result = await service.Update(id, dto.Title?.Trim(), dto.IsCompleted, userId);
                return result switch
                {
                    UpdateResult.Success => Results.NoContent(),
                    UpdateResult.NotFound => Results.Problem(detail: $"Todo dengan ID {id} tidak ditemukan.", statusCode: 404),
                    UpdateResult.InvalidTitle => Results.Problem(detail: "Judul tidak boleh kosong.", statusCode: 400),
                    _ => Results.Problem(statusCode: 500)
                };
            })
            .AddEndpointFilter<ValidationFilter<UpdateTodoDto>>();

        // DELETE /api/todos/{id}
        group.MapDelete("/{id:int}", 
            async (int id, ClaimsPrincipal user, [FromServices] TodoService service) =>
            {
                var userId = int.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var deleted = await service.Delete(id, userId);
                return deleted
                    ? Results.NoContent()
                    : Results.Problem(detail: $"Todo dengan ID {id} tidak ditemukan.", statusCode: 404);
            });
    }
}
