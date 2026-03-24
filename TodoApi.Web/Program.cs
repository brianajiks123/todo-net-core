using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TodoApi.Web;
using System.Diagnostics;
using TodoApi.Web.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// ────────────────────────────────────────
// Configuration
// ────────────────────────────────────────
var configuration = builder.Configuration;

var jwtSettings = configuration.GetSection("Jwt");
var jwtSecret = jwtSettings["Secret"] 
    ?? throw new InvalidOperationException("JWT Secret tidak ditemukan. Set via 'dotnet user-secrets' atau Environment Variable JWT__Secret");
var jwtIssuer = jwtSettings["Issuer"] ?? "todo-api";
var jwtAudience = jwtSettings["Audience"] ?? "todo-api";
var jwtExpirationHours = int.Parse(jwtSettings["ExpirationHours"] ?? "24");

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

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret))
        };
    });

builder.Services.AddAuthorization();

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
app.UseAuthentication();
app.UseAuthorization();

// ────────────────────────────────────────
// Auth Endpoint (Login)
// ────────────────────────────────────────
app.MapGroup("/api/auth")
   .MapPost("/login", (LoginRequest request) =>
   {
       if (request.Username == "admin" && request.Password == "password")
       {
           var token = GenerateJwtToken(request.Username, jwtSecret, jwtIssuer, jwtAudience, jwtExpirationHours);
           return Results.Ok(new { token });
       }
       return Results.Unauthorized();
   });

string GenerateJwtToken(string username, string secret, string issuer, string audience, int expirationHours)
{
    var tokenHandler = new JwtSecurityTokenHandler();
    var key = Encoding.UTF8.GetBytes(secret);

    var tokenDescriptor = new SecurityTokenDescriptor
    {
        Subject = new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, username) }),
        Expires = DateTime.UtcNow.AddHours(expirationHours),
        Issuer = issuer,
        Audience = audience,
        SigningCredentials = new SigningCredentials(
            new SymmetricSecurityKey(key),
            SecurityAlgorithms.HmacSha256Signature)
    };

    var token = tokenHandler.CreateToken(tokenDescriptor);
    return tokenHandler.WriteToken(token);
}

app.MapGroup("/api")
   .MapTodoEndpoints();

app.Run();

public record LoginRequest(string Username, string Password);
