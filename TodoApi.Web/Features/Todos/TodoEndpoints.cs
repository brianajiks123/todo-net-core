using Microsoft.AspNetCore.Http.HttpResults;
using TodoApi.Web.Features.Todos;

namespace TodoApi.Web;

public static class TodoEndpoints
{
    public static void MapTodoEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/todos")
                         .WithTags("Todos");

        // GET /api/todos
        group.MapGet("/", (TodoService service)
            => Results.Ok(service.GetAll()));

        // GET /api/todos/{id}
        group.MapGet("/{id:int}", (int id, TodoService service) =>
        {
            var todo = service.GetById(id);
            return todo is not null
                ? Results.Ok(todo)
                : Results.Problem(
                    detail: $"Todo dengan ID {id} tidak ditemukan.",
                    statusCode: StatusCodes.Status404NotFound,
                    title: "Not Found",
                    type: "https://httpstatuses.com/404"
            );
        });

        // POST /api/todos
        group.MapPost("/", (CreateTodoDto dto, TodoService service) =>
        {
            var created = service.Create(dto.Title.Trim());
            return Results.Created($"/api/todos/{created.Id}", created);
        });

        // PUT /api/todos/{id}
        group.MapPut("/{id:int}", (int id, UpdateTodoDto dto, TodoService service) =>
        {
            var updated = service.Update(id, dto.Title, dto.IsCompleted);
            return updated ? Results.NoContent() : Results.NotFound();
        });

        // DELETE /api/todos/{id}
        group.MapDelete("/{id:int}", (int id, TodoService service) =>
        {
            var deleted = service.Delete(id);
            return deleted 
                ? Results.NoContent() 
                : Results.Problem(
                    detail: $"Todo dengan ID {id} tidak ditemukan.",
                    statusCode: StatusCodes.Status404NotFound,
                    title: "Not Found"
            );
        });
    }
}
