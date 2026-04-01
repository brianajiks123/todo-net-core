using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Security.Claims;

namespace TodoApi.Web.Features.Todos.Filters;

public class LoggingAndPerformanceFilter : IEndpointFilter
{
    private readonly ILogger<LoggingAndPerformanceFilter> _logger;

    public LoggingAndPerformanceFilter(ILogger<LoggingAndPerformanceFilter> logger)
    {
        _logger = logger;
    }

    public async ValueTask<object?> InvokeAsync(
        EndpointFilterInvocationContext context,
        EndpointFilterDelegate next)
    {
        var stopwatch = Stopwatch.StartNew();
        var httpContext = context.HttpContext;

        var endpoint = httpContext.GetEndpoint();
        var endpointName = endpoint?.DisplayName 
                        ?? "Unknown";

        var nameMetadata = endpoint?.Metadata.GetMetadata<Microsoft.AspNetCore.Routing.EndpointNameMetadata>();
        if (nameMetadata is not null)
        {
            endpointName = nameMetadata.EndpointName;
        }

        var method = httpContext.Request.Method;
        var path = httpContext.Request.Path.ToString();

        var userId = httpContext.User?.Claims
            .FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier || c.Type == "sub")
            ?.Value ?? "anonymous";

        var correlationId = httpContext.Request.Headers["X-Correlation-ID"].FirstOrDefault()
                         ?? "unknown";

        _logger.LogInformation(
            "🚀 [START] {Method} {Path} | Endpoint: {EndpointName} | User: {UserId} | CorrelationId: {CorrelationId}",
            method, path, endpointName, userId, correlationId);

        try
        {
            var result = await next(context);

            stopwatch.Stop();
            var elapsedMs = stopwatch.ElapsedMilliseconds;

            _logger.LogInformation(
                "✅ [END] {Method} {Path} | Endpoint: {EndpointName} | Completed in {ElapsedMs}ms | Status: {StatusCode} | User: {UserId} | CorrelationId: {CorrelationId}",
                method, path, endpointName, elapsedMs, httpContext.Response.StatusCode, userId, correlationId);

            if (!httpContext.Response.HasStarted)
            {
                httpContext.Response.Headers["X-Response-Time-Ms"] = elapsedMs.ToString();
            }

            return result;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            var elapsedMs = stopwatch.ElapsedMilliseconds;

            _logger.LogError(ex,
                "❌ [ERROR] {Method} {Path} | Endpoint: {EndpointName} | Failed after {ElapsedMs}ms | User: {UserId} | CorrelationId: {CorrelationId}",
                method, path, endpointName, elapsedMs, userId, correlationId);

            throw;
        }
    }
}
