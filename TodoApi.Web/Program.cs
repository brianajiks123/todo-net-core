using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using TodoApi.Web;
using TodoApi.Web.Extensions;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ────────────────────────────────────────
// Configuration
// ────────────────────────────────────────
var configuration = builder.Configuration;

// ────────────────────────────────────────
// Services
// ────────────────────────────────────────
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Todo API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Masukkan JWT dengan format: Bearer {token}",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "Bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddValidation();
builder.Services.AddDbContext<TodoDbContext>(options =>
    options.UseSqlite("Data Source=todos.db"));

builder.Services.AddTodoServices();

// JWT Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var jwtSettings = configuration.GetSection("Jwt");
        var jwtSecret = jwtSettings["Secret"]
            ?? throw new InvalidOperationException("JWT Secret tidak ditemukan. Set via user-secrets atau environment variable.");

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings["Issuer"] ?? "todo-api",
            ValidAudience = jwtSettings["Audience"] ?? "todo-api",
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret))
        };
    });

builder.Services.AddAuthorization();

// ProblemDetails Customization
builder.Services.AddProblemDetails(options =>
{
    options.CustomizeProblemDetails = context =>
    {
        context.ProblemDetails.Extensions["traceId"] = context.HttpContext.TraceIdentifier;
        context.ProblemDetails.Extensions["timestamp"] = DateTime.UtcNow.ToString("o");
        context.ProblemDetails.Instance = $"{context.HttpContext.Request.Method} {context.HttpContext.Request.Path}{context.HttpContext.Request.QueryString}";

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

// Global Exception Handling
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        var exceptionHandlerPathFeature = context.Features.Get<IExceptionHandlerPathFeature>();
        var exception = exceptionHandlerPathFeature?.Error;

        if (exception != null)
        {
            Console.Error.WriteLine($"Unhandled exception: {exception}");

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
                Exception = exception
            });
        }
    });
});

app.UseAuthentication();
app.UseAuthorization();

// ────────────────────────────────────────
// Endpoints
// ────────────────────────────────────────
var api = app.MapGroup("/api");
api.MapAuthEndpoints();
api.MapTodoEndpoints();

app.Run();
