using Microsoft.EntityFrameworkCore;
using Serilog;
using Swashbuckle.AspNetCore.SwaggerUI;
using System.Security.Claims;
using System.Text.RegularExpressions;
using TodoApi.Web;
using TodoApi.Web.Extensions;
var builder = WebApplication.CreateBuilder(args);

// Serilog configuration (Structured + Console + File JSON)
builder.Host.UseSerilog((ctx, lc) => lc
    .ReadFrom.Configuration(ctx.Configuration)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Application", "TodoApi")
    .Enrich.WithProperty("Environment", ctx.HostingEnvironment.EnvironmentName)
    .WriteTo.Console()
    .WriteTo.Sink(new JsonArrayFileSink("logs", "log-")));

// Services
builder.Services.AddSwaggerWithJwt();
builder.Services.AddValidation();
builder.Services.AddDbContext<TodoDbContext>(options =>
    options.UseSqlite("Data Source=todos.db"));
builder.Services.AddTodoServices();
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddCustomProblemDetails();
builder.Services.AddCustomRateLimiting();

var allowedOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins")
    .Get<string[]>() ?? [];

if (!builder.Environment.IsDevelopment() && allowedOrigins.Length == 0)
    throw new InvalidOperationException("CORS allowed origins must be configured for non-development environments. Set 'Cors__AllowedOrigins__0', 'Cors__AllowedOrigins__1', etc. as environment variables.");

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        if (builder.Environment.IsDevelopment())
        {
            policy.WithOrigins(allowedOrigins)
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        }
        else
        {
            policy.WithOrigins(allowedOrigins)
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials();
        }
    });
});

builder.Services.AddHealthChecks()
    // Liveness
    .AddCheck("self", () => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy(), tags: ["liveness"])

    // Readiness
    .AddDbContextCheck<TodoDbContext>(name: "database", tags: ["readiness", "db"])
    .AddCheck("todo-api-ready", () => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy(), tags: ["readiness"]);

var app = builder.Build();

// Middleware
app.UseCors("AllowFrontend");

if (app.Environment.IsDevelopment())
{
    app.UseStaticFiles();
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Todo API v1");
        options.SwaggerEndpoint("/swagger/v2/swagger.json", "Todo API v2");
        options.RoutePrefix = "swagger";
        options.DisplayRequestDuration();
        options.DefaultModelsExpandDepth(-1);
        options.DocExpansion(DocExpansion.List);
        options.EnableDeepLinking();
        options.EnableFilter();
        options.InjectStylesheet("/swagger/custom.css");
        options.InjectJavascript("/swagger/custom.js");
        options.ConfigObject.AdditionalItems.Add("theme", "dark");
    });
}

// Correlation ID for all request
app.UseCorrelationId();

// HTTP logging + serilog request logging only for API v2
app.UseWhen(ctx => ctx.Request.Path.StartsWithSegments("/api/v2"), v2App =>
{
    v2App.UseRequestResponseBodyLogging();

    v2App.UseSerilogRequestLogging(options =>
    {
        options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.000}ms | CorrelationId: {CorrelationId} | UserId: {UserId}";

        options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
        {
            var userId = httpContext.User?.Claims
                .FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier || c.Type == "sub")?.Value ?? "anonymous";

            diagnosticContext.Set("UserId", userId);

            // Request Body
            if (httpContext.Items.TryGetValue("RequestBody", out var req) && req is string rb)
                diagnosticContext.Set("RequestBody", rb);

            // Response Body only if error
            if (httpContext.Items.TryGetValue("ResponseBody", out var resp) && resp is string rs)
                diagnosticContext.Set("ResponseBody", MaskSensitiveData(rs));
        };
    });
});

app.UseGlobalExceptionHandler();
app.UseHttpErrorResponses();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();

// Liveness Probe
app.MapHealthChecks("/api/v2/health/liveness", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("liveness"),
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        var result = new
        {
            status = report.Status.ToString(),
            checks = report.Entries.Select(e => new
            {
                name = e.Key,
                status = e.Value.Status.ToString(),
                description = e.Value.Description
            })
        };
        await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(result));
    }
});

// Readiness Probe
app.MapHealthChecks("/api/v2/health/readiness", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("readiness"),
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        var result = new
        {
            status = report.Status.ToString(),
            checks = report.Entries.Select(e => new
            {
                name = e.Key,
                status = e.Value.Status.ToString(),
                description = e.Value.Description,
                duration = e.Value.Duration
            }),
            totalDuration = report.TotalDuration
        };
        await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(result));
    }
});

app.MapHealthChecks("/api/v2/health", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        var result = new
        {
            status = report.Status.ToString(),
            environment = app.Environment.EnvironmentName,
            checks = report.Entries.Select(e => new
            {
                name = e.Key,
                status = e.Value.Status.ToString(),
                description = e.Value.Description
            })
        };
        await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(result));
    }
});

app.MapApiEndpoints();

app.Run();

// Helper: Mask Sensitive Data
static string MaskSensitiveData(string? input)
{
    if (string.IsNullOrWhiteSpace(input)) return "";

    var result = input;
    var keywords = new[] { "password", "secret", "token", "refreshToken", "key", "credential", "auth" };

    foreach (var kw in keywords)
    {
        result = System.Text.RegularExpressions.Regex.Replace(
            result,
            $@"(?i)(""{kw}"":\s*""[^""]*"")",
            $@"$1 (masked)",
            System.Text.RegularExpressions.RegexOptions.IgnoreCase);
    }

    return result;
}
