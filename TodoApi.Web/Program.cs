using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.SwaggerUI;
using TodoApi.Web;
using TodoApi.Web.Extensions;
using System.Security.Claims;
using Serilog;
using Microsoft.AspNetCore.Http;

var builder = WebApplication.CreateBuilder(args);

// Serilog configuration (Structured + Console + File JSON)
builder.Host.UseSerilog((ctx, lc) => lc
    .ReadFrom.Configuration(ctx.Configuration)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Application", "TodoApi")
    .Enrich.WithProperty("Environment", ctx.HostingEnvironment.EnvironmentName)
    .WriteTo.Console()
    .WriteTo.File(
        path: "logs/log-.json",
        rollingInterval: RollingInterval.Day,
        formatter: new Serilog.Formatting.Json.JsonFormatter()));

// Services
builder.Services.AddSwaggerWithJwt();
builder.Services.AddValidation();
builder.Services.AddDbContext<TodoDbContext>(options =>
    options.UseSqlite("Data Source=todos.db"));
builder.Services.AddTodoServices();
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddCustomProblemDetails();
builder.Services.AddCustomRateLimiting();

var app = builder.Build();

// Middleware
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
        // Custom styling
        options.InjectStylesheet("/swagger/custom.css");
        options.InjectJavascript("/swagger/custom.js");
        // Dark theme
        options.ConfigObject.AdditionalItems.Add("theme", "dark");
    });
}

// Correlation ID for all request
app.UseCorrelationId();

// Structured request/response logging only for API v2
app.UseWhen(ctx => ctx.Request.Path.StartsWithSegments("/api/v2"), v2App =>
{
    v2App.UseSerilogRequestLogging(options =>
    {
        options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.000}ms | CorrelationId: {CorrelationId} | UserId: {UserId}";

        options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
        {
            // Request headers
            diagnosticContext.Set("RequestHeaders", 
                httpContext.Request.Headers.ToDictionary(h => h.Key, h => h.Value.ToString()), 
                destructureObjects: true);

            // Get UserId with safed method
            var userId = httpContext.User?.Claims
                .FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier || c.Type == "sub")?.Value;

            if (!string.IsNullOrEmpty(userId))
                diagnosticContext.Set("UserId", userId);
            else
                diagnosticContext.Set("UserId", "anonymous");
        };
    });
});

app.UseGlobalExceptionHandler();
app.UseHttpErrorResponses();
app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();
app.MapApiEndpoints();

app.Run();
