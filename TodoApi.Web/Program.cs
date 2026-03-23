using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using TodoApi.Web;
using System.Diagnostics;

var builder = WebApplication.CreateBuilder(args);

// ────────────────────────────────────────
// Services
// ────────────────────────────────────────
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<TodoService>();  // in-memory storage
builder.Services.AddValidation();

// Enable ProblemDetails + Customization Global
builder.Services.AddProblemDetails(options =>
{
    options.CustomizeProblemDetails = context =>
    {
        // Add metadata to all ProblemDetails response
        context.ProblemDetails.Extensions["traceId"] = context.HttpContext.TraceIdentifier;
        context.ProblemDetails.Extensions["timestamp"] = DateTime.UtcNow.ToString("o"); // ISO 8601
        context.ProblemDetails.Instance = $"{context.HttpContext.Request.Method} {context.HttpContext.Request.Path}{context.HttpContext.Request.QueryString}";

        // Development Mode: add exception type; remove it when production mode
        if (context.HttpContext.RequestServices.GetService<IWebHostEnvironment>()?.IsDevelopment() ?? false)
        {
            if (context.Exception is not null)
            {
                context.ProblemDetails.Extensions["exceptionType"] = context.Exception.GetType().Name;
            }
        }
    };
});

var app = builder.Build();

// ────────────────────────────────────────
// Middleware pipeline
// ────────────────────────────────────────
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Global Exception Handling → Convert to ProblemDetails
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        var exceptionHandlerPathFeature = context.Features.Get<IExceptionHandlerPathFeature>();
        var exception = exceptionHandlerPathFeature?.Error;

        if (exception != null)
        {
            // Log exception in here if has logger
            Console.Error.WriteLine($"Unhandled exception: {exception}");

            // Create ProblemDetails manual for unhandled error
            var problemDetailsService = context.RequestServices.GetRequiredService<IProblemDetailsService>();
            await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
            {
                HttpContext = context,
                ProblemDetails = new ProblemDetails
                {
                    Status = StatusCodes.Status500InternalServerError,
                    Title = "Terjadi kesalahan internal server",
                    Detail = app.Environment.IsDevelopment() ? exception.Message : "Terjadi kesalahan. Silakan coba lagi nanti.",
                    Type = "https://httpstatuses.com/500"
                },
                Exception = exception // for customization callback
            });
        }
    });
});

// app.UseHttpsRedirection();   // off when development mode
// app.UseAuthorization();

app.MapGroup("/api")
   .MapTodoEndpoints();

app.Run();
