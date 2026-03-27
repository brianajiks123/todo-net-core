using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using System.Text.Json;
using TodoApi.Web.Features.Auth;
using TodoApi.Web.Features.Auth.Repositories;
using TodoApi.Web.Features.Todos;
using TodoApi.Web.Features.Todos.Mapping;
using FluentValidation;

namespace TodoApi.Web.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddTodoServices(this IServiceCollection services)
    {
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<TodoService>();
        services.AddScoped<AuthService>();
        services.AddValidatorsFromAssemblyContaining<CreateTodoDtoValidator>();
        services.AddAutoMapper(cfg => { }, typeof(TodoProfile));
        services.AddHttpContextAccessor();
        return services;
    }

    public static IServiceCollection AddSwaggerWithJwt(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();

        services.AddSwaggerGen(options =>
        {
            // v1
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Todo API",
                Version = "v1",
                Description = "Version 1 - API Todo",
                Contact = new OpenApiContact
                {
                    Name = "Brian Aji",
                    Email = "your.email@example.com"
                }
            });

            // v2
            options.SwaggerDoc("v2", new OpenApiInfo
            {
                Title = "Todo API",
                Version = "v2",
                Description = "Version 2 - With Rate Limiting"
            });

            // JWT Security
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                In = ParameterLocation.Header,
                Description = "Input JWT with format: Bearer {token}",
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                BearerFormat = "JWT",
                Scheme = "Bearer"
            });

            options.AddSecurityRequirement(new OpenApiSecurityRequirement
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

        return services;
    }

    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                var jwtSettings = configuration.GetSection("Jwt");
                var jwtSecret = jwtSettings["Secret"]
                    ?? throw new InvalidOperationException("JWT Secret is not found. Set via user-secrets or environment variable.");

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

                options.Events = new JwtBearerEvents
                {
                    OnChallenge = async context =>
                    {
                        context.HandleResponse();
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        context.Response.ContentType = "application/json";
                        var message = string.IsNullOrEmpty(context.ErrorDescription)
                            ? "Access is rejected. Token is not valid or not found."
                            : context.ErrorDescription;
                        await context.Response.WriteAsync(JsonSerializer.Serialize(ApiResponse.Fail(message)));
                    },
                    OnForbidden = async context =>
                    {
                        context.Response.StatusCode = StatusCodes.Status403Forbidden;
                        context.Response.ContentType = "application/json";
                        await context.Response.WriteAsync(JsonSerializer.Serialize(
                            ApiResponse.Fail("You are not allowed to access the resource.")));
                    }
                };
            });

        services.AddAuthorization();
        return services;
    }

    public static IServiceCollection AddCustomProblemDetails(this IServiceCollection services)
    {
        services.AddProblemDetails(options =>
        {
            options.CustomizeProblemDetails = context =>
            {
                context.ProblemDetails.Extensions["traceId"] = context.HttpContext.TraceIdentifier;
                context.ProblemDetails.Extensions["timestamp"] = DateTime.UtcNow.ToString("o");
                context.ProblemDetails.Instance = $"{context.HttpContext.Request.Method} {context.HttpContext.Request.Path}{context.HttpContext.Request.QueryString}";

                if (context.HttpContext.RequestServices.GetService<IWebHostEnvironment>()?.IsDevelopment() ?? false)
                {
                    if (context.Exception is not null)
                        context.ProblemDetails.Extensions["exceptionType"] = context.Exception.GetType().Name;
                }
            };
        });
        return services;
    }
}
