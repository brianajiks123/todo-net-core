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
                : Results.NotFound();
        });

        // POST /api/todos
        group.MapPost("/", async (HttpContext ctx, TodoService service) =>
        {
            var dto = await ctx.Request.ReadFromJsonAsync<CreateTodoDto>();
            if (dto is null || string.IsNullOrWhiteSpace(dto.Title))
                return Results.BadRequest("Title is required");

            var created = service.Create(dto.Title.Trim());
            return Results.Created($"/api/todos/{created.Id}", created);
        });

        // PUT /api/todos/{id}
        group.MapPut("/{id:int}", async (int id, HttpContext ctx, TodoService service) =>
        {
            var dto = await ctx.Request.ReadFromJsonAsync<UpdateTodoDto>();
            var success = service.Update(id, dto?.Title, dto?.IsCompleted);
            return success ? Results.NoContent() : Results.NotFound();
        });

        // DELETE /api/todos/{id}
        group.MapDelete("/{id:int}", (int id, TodoService service) =>
        {
            var deleted = service.Delete(id);
            return deleted ? Results.NoContent() : Results.NotFound();
        });
    }
}

// DTOs
record CreateTodoDto(string Title);
record UpdateTodoDto(string? Title, bool? IsCompleted);
