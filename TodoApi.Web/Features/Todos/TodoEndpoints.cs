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
            .WithSummary("Daftar todo dengan pagination (v1)");

        // GET /api/v1/todos/{id}
        group.MapGet("/{id:int}",
            async (int id, ClaimsPrincipal user, [FromServices] TodoService service) =>
            {
                var userId = int.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var todo = await service.GetById(id, userId);
                return todo is not null
                    ? Results.Ok(ApiResponse<TodoResponseDto>.Ok(todo))
                    : Results.NotFound(ApiResponse.Fail($"Todo dengan ID {id} tidak ditemukan."));
            })
            .WithName("GetTodoByIdV1")
            .WithSummary("Detail todo (v1)");

        // POST /api/v1/todos
        group.MapPost("/",
            async (CreateTodoDto dto, ClaimsPrincipal user, [FromServices] TodoService service) =>
            {
                var userId = int.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var created = await service.Create(dto.Title.Trim(), userId);
                return Results.Created(
                    $"/api/v1/todos/{created.Id}",
                    ApiResponse<TodoResponseDto>.Ok(created, "Todo berhasil dibuat."));
            })
            .AddEndpointFilter<ValidationFilter<CreateTodoDto>>()
            .WithName("CreateTodoV1")
            .WithSummary("Buat todo baru (v1)");

        // PUT /api/v1/todos/{id}
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
            .WithName("UpdateTodoV1")
            .WithSummary("Perbarui todo (v1)");

        // DELETE /api/v1/todos/{id}
        group.MapDelete("/{id:int}",
            async (int id, ClaimsPrincipal user, [FromServices] TodoService service) =>
            {
                var userId = int.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var deleted = await service.Delete(id, userId);
                return deleted
                    ? Results.Ok(ApiResponse.Ok("Todo berhasil dihapus."))
                    : Results.NotFound(ApiResponse.Fail($"Todo dengan ID {id} tidak ditemukan."));
            })
            .WithName("DeleteTodoV1")
            .WithSummary("Hapus todo (v1)");
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
            .WithSummary("Daftar todo dengan pagination (v2)");

        // GET /api/v2/todos/{id}
        group.MapGet("/{id:int}",
            async (int id, ClaimsPrincipal user, [FromServices] TodoService service) =>
            {
                var userId = int.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var todo = await service.GetById(id, userId);
                return todo is not null
                    ? Results.Ok(ApiResponse<TodoResponseDto>.Ok(todo))
                    : Results.NotFound(ApiResponse.Fail($"Todo dengan ID {id} tidak ditemukan."));
            })
            .WithName("GetTodoByIdV2")
            .WithSummary("Detail todo (v2)");

        // POST /api/v2/todos
        group.MapPost("/",
            async (CreateTodoDto dto, ClaimsPrincipal user, [FromServices] TodoService service) =>
            {
                var userId = int.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var created = await service.Create(dto.Title.Trim(), userId);
                return Results.Created(
                    $"/api/v2/todos/{created.Id}",
                    ApiResponse<TodoResponseDto>.Ok(created, "Todo berhasil dibuat."));
            })
            .AddEndpointFilter<ValidationFilter<CreateTodoDto>>()
            .WithName("CreateTodoV2")
            .WithSummary("Buat todo baru (v2)");

        // PUT /api/v2/todos/{id}
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
            .WithName("UpdateTodoV2")
            .WithSummary("Perbarui todo (v2)");

        // DELETE /api/v2/todos/{id}
        group.MapDelete("/{id:int}",
            async (int id, ClaimsPrincipal user, [FromServices] TodoService service) =>
            {
                var userId = int.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var deleted = await service.Delete(id, userId);
                return deleted
                    ? Results.Ok(ApiResponse.Ok("Todo berhasil dihapus."))
                    : Results.NotFound(ApiResponse.Fail($"Todo dengan ID {id} tidak ditemukan."));
            })
            .WithName("DeleteTodoV2")
            .WithSummary("Hapus todo (v2)");
    }
}
