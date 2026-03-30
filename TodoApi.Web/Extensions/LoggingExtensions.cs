using Microsoft.AspNetCore.Http;
using Serilog;
using Serilog.Context;
using System.Text;

namespace TodoApi.Web.Extensions;

public static class LoggingExtensions
{
    // Correlation ID
    public static IApplicationBuilder UseCorrelationId(this IApplicationBuilder app)
    {
        return app.Use(async (context, next) =>
        {
            var correlationId = context.Request.Headers["X-Correlation-ID"].FirstOrDefault()
                              ?? Guid.NewGuid().ToString();

            context.Response.OnStarting(() =>
            {
                context.Response.Headers["X-Correlation-ID"] = correlationId;
                return Task.CompletedTask;
            });

            using (LogContext.PushProperty("CorrelationId", correlationId))
            {
                await next();
            }
        });
    }

    // Middleware to capture Request & Response Body (only for API v2)
    public static IApplicationBuilder UseRequestResponseBodyLogging(this IApplicationBuilder app)
    {
        return app.Use(async (context, next) =>
        {
            // Capture request body
            if (context.Request.ContentLength > 0 &&
                context.Request.ContentType?.Contains("application/json", StringComparison.OrdinalIgnoreCase) == true)
            {
                context.Request.EnableBuffering();
                using var reader = new StreamReader(context.Request.Body, Encoding.UTF8, leaveOpen: true);
                var body = await reader.ReadToEndAsync();
                context.Request.Body.Position = 0;
                context.Items["RequestBody"] = body.Trim();
            }

            // Capture response body only if error
            var originalBody = context.Response.Body;
            await using var memStream = new MemoryStream();
            context.Response.Body = memStream;

            try
            {
                await next();

                // Only read response if status error (>= 400) & response not sent
                if (context.Response.StatusCode >= 400 && !context.Response.HasStarted)
                {
                    memStream.Position = 0;
                    using var reader = new StreamReader(memStream, Encoding.UTF8, leaveOpen: true);
                    var responseText = await reader.ReadToEndAsync();
                    context.Items["ResponseBody"] = responseText.Trim();
                }
            }
            finally
            {
                // Always restore stream
                try
                {
                    if (!context.Response.HasStarted)
                    {
                        memStream.Position = 0;
                        await memStream.CopyToAsync(originalBody);
                    }
                }
                catch (ObjectDisposedException)
                {
                    // 
                }
                catch (Exception ex) when (ex is InvalidOperationException or ObjectDisposedException)
                {
                    // 
                }
            }
        });
    }
}
