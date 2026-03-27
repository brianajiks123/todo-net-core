using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using TodoApi.Web.Features.Auth;

namespace TodoApi.Web.Extensions;

public static class WebApplicationExtensions
{
    public static WebApplication UseGlobalExceptionHandler(this WebApplication app)
    {
        app.UseExceptionHandler(errorApp =>
        {
            errorApp.Run(async context =>
            {
                var exception = context.Features.Get<IExceptionHandlerPathFeature>()?.Error;
                if (exception is null) return;

                Console.Error.WriteLine($"Unhandled exception: {exception}");

                var problemDetailsService = context.RequestServices.GetRequiredService<IProblemDetailsService>();
                await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
                {
                    HttpContext = context,
                    ProblemDetails = new ProblemDetails
                    {
                        Status = StatusCodes.Status500InternalServerError,
                        Title = "Terjadi kesalahan internal server",
                        Detail = app.Environment.IsDevelopment()
                            ? exception.Message
                            : "Terjadi kesalahan. Silakan coba lagi nanti.",
                        Type = "https://httpstatuses.com/500"
                    },
                    Exception = exception
                });
            });
        });

        return app;
    }

    public static WebApplication UseHttpErrorResponses(this WebApplication app)
    {
        app.Use(async (context, next) =>
        {
            await next();

            if (context.Response.HasStarted) return;

            string? message = context.Response.StatusCode switch
            {
                // 4xx Client Errors
                400 => "Permintaan tidak valid. Periksa kembali data yang dikirim.",
                401 => "Akses ditolak. Token tidak valid atau tidak ditemukan.",
                402 => "Pembayaran diperlukan untuk mengakses resource ini.",
                403 => "Anda tidak memiliki izin untuk mengakses resource ini.",
                404 => $"Resource '{context.Request.Path}' tidak ditemukan.",
                405 => $"HTTP method '{context.Request.Method}' tidak diizinkan untuk endpoint ini.",
                406 => "Format respons yang diminta tidak didukung.",
                407 => "Autentikasi proxy diperlukan.",
                408 => "Waktu permintaan habis. Silakan coba lagi.",
                409 => "Konflik data. Resource sudah ada atau terjadi bentrokan.",
                410 => "Resource ini sudah tidak tersedia secara permanen.",
                411 => "Header 'Content-Length' diperlukan.",
                412 => "Prasyarat permintaan gagal.",
                413 => "Ukuran payload terlalu besar.",
                414 => "URL terlalu panjang.",
                415 => "Tipe media tidak didukung.",
                416 => "Range yang diminta tidak dapat dipenuhi.",
                417 => "Ekspektasi permintaan gagal.",
                418 => "Saya adalah teko. (RFC 2324)",
                421 => "Permintaan diarahkan ke server yang salah.",
                422 => "Data tidak dapat diproses. Periksa kembali isi permintaan.",
                423 => "Resource sedang terkunci.",
                424 => "Permintaan gagal karena ketergantungan sebelumnya gagal.",
                425 => "Server tidak bersedia memproses permintaan ini terlalu awal.",
                426 => "Diperlukan upgrade protokol.",
                428 => "Permintaan harus bersifat kondisional.",
                429 => "Terlalu banyak permintaan. Silakan coba lagi nanti.",
                431 => "Header permintaan terlalu besar.",
                451 => "Resource tidak tersedia karena alasan hukum.",

                // 5xx Server Errors
                500 => "Terjadi kesalahan internal server.",
                501 => "Fitur ini belum diimplementasikan.",
                502 => "Gateway menerima respons tidak valid dari server upstream.",
                503 => "Layanan tidak tersedia. Silakan coba lagi nanti.",
                504 => "Gateway timeout. Server upstream tidak merespons.",
                505 => "Versi HTTP tidak didukung.",
                506 => "Terjadi kesalahan negosiasi konten.",
                507 => "Penyimpanan server tidak mencukupi.",
                508 => "Terdeteksi loop tak terbatas saat memproses permintaan.",
                510 => "Ekstensi lebih lanjut diperlukan untuk memenuhi permintaan.",
                511 => "Autentikasi jaringan diperlukan.",

                _ => null
            };

            if (message is not null)
            {
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync(JsonSerializer.Serialize(ApiResponse.Fail(message)));
            }
        });

        return app;
    }

    public static WebApplication MapApiEndpoints(this WebApplication app)
    {
        // ==================== VERSION 1 ====================
        var apiV1 = app.MapGroup("/api/v1");

        apiV1.MapAuthEndpointsV1();
        apiV1.MapTodoEndpointsV1();

        // ==================== VERSION 2 ====================
        var apiV2 = app.MapGroup("/api/v2")
                        .RequireRateLimiting("v2-ip-limit");

        apiV2.MapAuthEndpointsV2();
        apiV2.MapTodoEndpointsV2();

        return app;
    }
}
