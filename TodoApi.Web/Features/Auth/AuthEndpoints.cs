using Microsoft.AspNetCore.Mvc;
using TodoApi.Web.Features.Todos.Filters;

namespace TodoApi.Web.Features.Auth;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/auth")
            .WithTags("Auth");

        // POST /api/auth/register
        group.MapPost("/register",
            async (RegisterDto dto, [FromServices] AuthService service) =>
            {
                var result = await service.RegisterAsync(dto);
                return Results.Ok(ApiResponse<AuthResponseDto>.Ok(result, "Registrasi berhasil."));
            })
            .AddEndpointFilter<ValidationFilter<RegisterDto>>()
            .WithSummary("Registrasi user baru");

        // POST /api/auth/login
        group.MapPost("/login",
            async (LoginDto dto, [FromServices] AuthService service) =>
            {
                var result = await service.LoginAsync(dto);
                return result is not null
                    ? Results.Ok(ApiResponse<AuthResponseDto>.Ok(result, "Login berhasil."))
                    : Results.Json(ApiResponse.Fail("Username atau password salah."), statusCode: StatusCodes.Status401Unauthorized);
            })
            .AddEndpointFilter<ValidationFilter<LoginDto>>()
            .WithSummary("Login dan dapatkan Access Token + Refresh Token");

        // POST /api/auth/refresh
        group.MapPost("/refresh",
            async (RefreshRequestDto dto, [FromServices] AuthService service) =>
            {
                var result = await service.RefreshTokenAsync(dto.RefreshToken);
                return result is not null
                    ? Results.Ok(ApiResponse<AuthResponseDto>.Ok(result, "Token berhasil diperbarui."))
                    : Results.Json(ApiResponse.Fail("Refresh token tidak valid atau sudah kedaluwarsa."), statusCode: StatusCodes.Status401Unauthorized);
            })
            .AddEndpointFilter<ValidationFilter<RefreshRequestDto>>()
            .WithSummary("Refresh Access Token menggunakan Refresh Token");
    }
}
