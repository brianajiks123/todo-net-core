using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using TodoApi.Web.Features.Todos;
using TodoApi.Web.Features.Todos.Filters;

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
            async ([AsParameters] TodoQueryParams query, 
                    [FromServices] TodoService service) => 
            Results.Ok(await service.GetPaged(query)))
            .WithName("GetTodos")
            .WithSummary("Daftar todo dengan pagination, search & sorting");

        // GET /api/todos/{id}
        group.MapGet("/{id:int}", 
            async (int id, [FromServices] TodoService service) =>
        {
            var todo = await service.GetById(id);
            return todo is not null
                ? Results.Ok(todo)
                : Results.Problem(detail: $"Todo dengan ID {id} tidak ditemukan.", statusCode: 404);
        });

        // POST /api/todos
        group.MapPost("/", 
            async (CreateTodoDto dto, [FromServices] TodoService service) =>
        {
            var created = await service.Create(dto.Title.Trim());
            return Results.Created($"/api/todos/{created.Id}", created);
        })
        .AddEndpointFilter<ValidationFilter<CreateTodoDto>>();

        // PUT /api/todos/{id}
        group.MapPut("/{id:int}", 
            async (int id, UpdateTodoDto dto, [FromServices] TodoService service) =>
        {
            var success = await service.Update(id, dto.Title?.Trim(), dto.IsCompleted);
            return success
                ? Results.NoContent()
                : Results.Problem(detail: $"Todo dengan ID {id} tidak ditemukan.", statusCode: 404);
        })
        .AddEndpointFilter<ValidationFilter<UpdateTodoDto>>();

        // DELETE /api/todos/{id}
        group.MapDelete("/{id:int}", 
            async (int id, [FromServices] TodoService service) =>
        {
            var deleted = await service.Delete(id);
            return deleted
                ? Results.NoContent()
                : Results.Problem(detail: $"Todo dengan ID {id} tidak ditemukan.", statusCode: 404);
        });
    }
}
