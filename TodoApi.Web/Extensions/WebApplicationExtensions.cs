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
                        Title = "An internal server error occurred.",
                        Detail = app.Environment.IsDevelopment()
                            ? exception.Message
                            : "Something went wrong. Please try again later.",
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
            if (context.Items.ContainsKey("ValidationHandled")) return;

            string? message = context.Response.StatusCode switch
            {
                // 4xx Client Errors
                400 => "Bad request. Please check the submitted data.",
                401 => "Unauthorized. Token is invalid or not provided.",
                402 => "Payment required to access this resource.",
                403 => "Forbidden. You do not have permission to access this resource.",
                404 => $"Resource '{context.Request.Path}' not found.",
                405 => $"HTTP method '{context.Request.Method}' is not allowed for this endpoint.",
                406 => "The requested response format is not supported.",
                407 => "Proxy authentication required.",
                408 => "Request timed out. Please try again.",
                409 => "Conflict. The resource already exists or there is a data conflict.",
                410 => "This resource is no longer available.",
                411 => "Header 'Content-Length' is required.",
                412 => "Precondition failed.",
                413 => "Payload too large.",
                414 => "URI too long.",
                415 => "Unsupported media type.",
                416 => "Requested range not satisfiable.",
                417 => "Expectation failed.",
                418 => "I'm a teapot. (RFC 2324)",
                421 => "Misdirected request.",
                422 => "Unprocessable content. Please check the request body.",
                423 => "Resource is locked.",
                424 => "Failed dependency.",
                425 => "Too early. The server is unwilling to process this request.",
                426 => "Upgrade required.",
                428 => "Precondition required.",
                429 => "Too many requests. Please try again later.",
                431 => "Request header fields too large.",
                451 => "Unavailable for legal reasons.",

                // 5xx Server Errors
                501 => "Not implemented.",
                502 => "Bad gateway. Received an invalid response from the upstream server.",
                503 => "Service unavailable. Please try again later.",
                504 => "Gateway timeout. The upstream server did not respond.",
                505 => "HTTP version not supported.",
                506 => "Variant also negotiates.",
                507 => "Insufficient storage.",
                508 => "Loop detected.",
                510 => "Not extended.",
                511 => "Network authentication required.",

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
                        .RequireRateLimiting("v2-ip-limit")
                        .AddEndpointFilter<LoggingAndPerformanceFilter>();

        apiV2.MapAuthEndpointsV2();
        apiV2.MapTodoEndpointsV2();

        return app;
    }
}
