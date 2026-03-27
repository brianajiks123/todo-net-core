using Microsoft.AspNetCore.Mvc;
using TodoApi.Web.Features.Todos.Filters;

namespace TodoApi.Web.Features.Auth;

public static class AuthEndpoints
{
    // Version 1
    public static void MapAuthEndpointsV1(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/auth")
            .WithTags("Auth v1");

        // POST /api/v1/auth/register
        group.MapPost("/register",
            async (RegisterDto dto, [FromServices] AuthService service) =>
            {
                var result = await service.RegisterAsync(dto);
                return Results.Ok(ApiResponse<AuthResponseDto>.Ok(result, "Registrasi berhasil."));
            })
            .AddEndpointFilter<ValidationFilter<RegisterDto>>()
            .WithName("RegisterV1")
            .WithSummary("Registrasi user baru (v1)");

        // POST /api/v1/auth/login
        group.MapPost("/login",
            async (LoginDto dto, [FromServices] AuthService service) =>
            {
                var result = await service.LoginAsync(dto);
                return result is not null
                    ? Results.Ok(ApiResponse<AuthResponseDto>.Ok(result, "Login berhasil."))
                    : Results.Json(
                        ApiResponse.Fail("Username atau password salah."),
                        statusCode: StatusCodes.Status401Unauthorized);
            })
            .AddEndpointFilter<ValidationFilter<LoginDto>>()
            .WithName("LoginV1")
            .WithSummary("Login (v1)");

        // POST /api/v1/auth/refresh
        group.MapPost("/refresh",
            async (RefreshRequestDto dto, [FromServices] AuthService service) =>
            {
                var result = await service.RefreshTokenAsync(dto.RefreshToken);
                return result is not null
                    ? Results.Ok(ApiResponse<AuthResponseDto>.Ok(result, "Token berhasil diperbarui."))
                    : Results.Json(
                        ApiResponse.Fail("Refresh token tidak valid atau sudah kedaluwarsa."),
                        statusCode: StatusCodes.Status401Unauthorized);
            })
            .AddEndpointFilter<ValidationFilter<RefreshRequestDto>>()
            .WithName("RefreshTokenV1")
            .WithSummary("Refresh token (v1)");
    }

    // Version 2
    public static void MapAuthEndpointsV2(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/auth")
            .WithTags("Auth v2");

        // POST /api/v2/auth/register
        group.MapPost("/register",
            async (RegisterDto dto, [FromServices] AuthService service) =>
            {
                var result = await service.RegisterAsync(dto);
                return Results.Ok(ApiResponse<AuthResponseDto>.Ok(result, "Registrasi berhasil."));
            })
            .AddEndpointFilter<ValidationFilter<RegisterDto>>()
            .WithName("RegisterV2")
            .WithSummary("Registrasi user baru (v2)");

        // POST /api/v2/auth/login
        group.MapPost("/login",
            async (LoginDto dto, [FromServices] AuthService service) =>
            {
                var result = await service.LoginAsync(dto);
                return result is not null
                    ? Results.Ok(ApiResponse<AuthResponseDto>.Ok(result, "Login berhasil."))
                    : Results.Json(
                        ApiResponse.Fail("Username atau password salah."),
                        statusCode: StatusCodes.Status401Unauthorized);
            })
            .AddEndpointFilter<ValidationFilter<LoginDto>>()
            .WithName("LoginV2")
            .WithSummary("Login (v2)");

        // POST /api/v2/auth/refresh
        group.MapPost("/refresh",
            async (RefreshRequestDto dto, [FromServices] AuthService service) =>
            {
                var result = await service.RefreshTokenAsync(dto.RefreshToken);
                return result is not null
                    ? Results.Ok(ApiResponse<AuthResponseDto>.Ok(result, "Token berhasil diperbarui."))
                    : Results.Json(
                        ApiResponse.Fail("Refresh token tidak valid atau sudah kedaluwarsa."),
                        statusCode: StatusCodes.Status401Unauthorized);
            })
            .AddEndpointFilter<ValidationFilter<RefreshRequestDto>>()
            .WithName("RefreshTokenV2")
            .WithSummary("Refresh token (v2)");
    }
}
