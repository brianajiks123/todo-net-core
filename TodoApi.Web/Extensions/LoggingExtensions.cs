using Microsoft.AspNetCore.Http;
using Serilog;
using Serilog.Context;
using System.Security.Claims;

namespace TodoApi.Web.Extensions;

public static class LoggingExtensions
{
    /// <summary>
    /// Add Correlation ID (X-Correlation-ID) to each request.
    /// Automatic generate GUID if not sent by client.
    /// CorrelationId will be automatic show in all log Serilog.
    /// </summary>
    public static IApplicationBuilder UseCorrelationId(this IApplicationBuilder app)
    {
        return app.Use(async (context, next) =>
        {
            var correlationId = context.Request.Headers["X-Correlation-ID"].ToString();

            if (string.IsNullOrWhiteSpace(correlationId))
                correlationId = Guid.NewGuid().ToString();

            // Save in response header to client understand
            context.Response.OnStarting(() =>
            {
                context.Response.Headers["X-Correlation-ID"] = correlationId;
                return Task.CompletedTask;
            });

            // Push to Serilog LogContext (will be show in all log while the request)
            using (LogContext.PushProperty("CorrelationId", correlationId))
            {
                await next();
            }
        });
    }
}
