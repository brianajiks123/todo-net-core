using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;

namespace TodoApi.Web.Extensions;

public static class RateLimitingExtensions
{
    public static IServiceCollection AddCustomRateLimiting(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            // Custom Policy for API v2: 100 request per minute per IP
            options.AddPolicy("v2-ip-limit", httpContext =>
            {
                var ipAddress = httpContext.Connection.RemoteIpAddress?.ToString() 
                                ?? httpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault() 
                                ?? "unknown";

                return RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: ipAddress,
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 100,                    // max: 100 request
                        Window = TimeSpan.FromMinutes(1),     // 1 minute
                        QueueLimit = 0,                       // auto reject if more
                    });
            });

            // Custom response
            options.OnRejected = async (context, cancellationToken) =>
            {
                context.HttpContext.Response.ContentType = "application/json";
                context.HttpContext.Response.Headers["Retry-After"] = "60";

                var response = ApiResponse.Fail(
                    "Too many request. Max 100 request per minute per IP. Please, try again after 1 minute.");

                await context.HttpContext.Response.WriteAsync(
                    System.Text.Json.JsonSerializer.Serialize(response), 
                    cancellationToken);
            };
        });

        return services;
    }
}
