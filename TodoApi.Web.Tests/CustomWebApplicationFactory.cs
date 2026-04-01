using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TodoApi.Web.Features.Todos;

namespace TodoApi.Web.Tests;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly InMemoryDatabaseRoot _dbRoot = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "Jwt:Secret", "SuperSecretKeyforJWTAtLeast32CharactersLong0987654321!" },
                { "Jwt:Issuer", "todo-api" },
                { "Jwt:Audience", "todo-api" },
                { "Jwt:ExpirationHours", "1" },
                { "Jwt:RefreshExpirationDays", "7" }
            });
        });

        builder.ConfigureServices(services =>
        {
            var toRemove = services
                .Where(d =>
                    d.ServiceType == typeof(DbContextOptions<TodoDbContext>) ||
                    d.ServiceType == typeof(DbContextOptions) ||
                    d.ServiceType == typeof(TodoDbContext) ||
                    d.ServiceType == typeof(IDbContextOptionsConfiguration<TodoDbContext>))
                .ToList();

            foreach (var d in toRemove)
                services.Remove(d);

            services.AddDbContext<TodoDbContext>(options =>
                options.UseInMemoryDatabase("TodoTestDb", _dbRoot));

            services.PostConfigureAll<Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerOptions>(options =>
            {
                var key = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(
                    System.Text.Encoding.UTF8.GetBytes("SuperSecretKeyforJWTAtLeast32CharactersLong0987654321!"));
                options.TokenValidationParameters.IssuerSigningKey = key;
                options.TokenValidationParameters.ValidIssuer = "todo-api";
                options.TokenValidationParameters.ValidAudience = "todo-api";
            });
        });
    }
}
