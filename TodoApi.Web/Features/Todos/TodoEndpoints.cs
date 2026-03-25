using Microsoft.AspNetCore.Mvc;
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

        // GET /api/todos
        group.MapGet("/",
            async (ClaimsPrincipal user, [AsParameters] TodoQueryParams query, [FromServices] TodoService service) =>
            {
                var userId = int.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var result = await service.GetPaged(query, userId);
                return Results.Ok(ApiResponse<PagedResult<TodoResponseDto>>.Ok(result));
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
                    ? Results.Ok(ApiResponse<TodoResponseDto>.Ok(todo))
                    : Results.NotFound(ApiResponse.Fail($"Todo dengan ID {id} tidak ditemukan."));
            })
            .WithSummary("Detail todo berdasarkan ID");

        // POST /api/todos
        group.MapPost("/",
            async (CreateTodoDto dto, ClaimsPrincipal user, [FromServices] TodoService service) =>
            {
                var userId = int.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var created = await service.Create(dto.Title.Trim(), userId);
                return Results.Created(
                    $"/api/todos/{created.Id}",
                    ApiResponse<TodoResponseDto>.Ok(created, "Todo berhasil dibuat."));
            })
            .AddEndpointFilter<ValidationFilter<CreateTodoDto>>()
            .WithSummary("Buat todo baru");

        // PUT /api/todos/{id}
        group.MapPut("/{id:int}",
            async (int id, UpdateTodoDto dto, ClaimsPrincipal user, [FromServices] TodoService service) =>
            {
                var userId = int.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var result = await service.Update(id, dto.Title?.Trim(), dto.IsCompleted, userId);
                return result.Result switch
                {
                    UpdateResult.Success      => Results.Ok(ApiResponse<TodoResponseDto>.Ok(result.Data!, "Todo berhasil diperbarui.")),
                    UpdateResult.NotFound     => Results.NotFound(ApiResponse.Fail($"Todo dengan ID {id} tidak ditemukan.")),
                    UpdateResult.InvalidTitle => Results.BadRequest(ApiResponse.Fail("Judul tidak boleh kosong.")),
                    _                         => Results.Json(ApiResponse.Fail("Terjadi kesalahan internal."), statusCode: 500)
                };
            })
            .AddEndpointFilter<ValidationFilter<UpdateTodoDto>>()
            .WithSummary("Perbarui todo");

        // DELETE /api/todos/{id}
        group.MapDelete("/{id:int}",
            async (int id, ClaimsPrincipal user, [FromServices] TodoService service) =>
            {
                var userId = int.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var deleted = await service.Delete(id, userId);
                return deleted
                    ? Results.Ok(ApiResponse.Ok("Todo berhasil dihapus."))
                    : Results.NotFound(ApiResponse.Fail($"Todo dengan ID {id} tidak ditemukan."));
            })
            .WithSummary("Hapus todo");
    }
}
