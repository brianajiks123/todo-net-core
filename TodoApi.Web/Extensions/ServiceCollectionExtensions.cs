using Microsoft.Extensions.DependencyInjection;
using TodoApi.Web.Features.Todos;
using FluentValidation;
using TodoApi.Web.Features.Todos.Mapping;
using TodoApi.Web.Features.Auth;
using TodoApi.Web.Features.Auth.Repositories;

namespace TodoApi.Web.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddTodoServices(this IServiceCollection services)
    {
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<TodoService>();
        services.AddScoped<AuthService>();
        
        services.AddValidatorsFromAssemblyContaining<CreateTodoDtoValidator>();

        services.AddAutoMapper(cfg => {}, typeof(TodoProfile));

        return services;
    }
}
