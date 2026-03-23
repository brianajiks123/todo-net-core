using Microsoft.Extensions.DependencyInjection;
using TodoApi.Web.Features.Todos;
using FluentValidation;
using TodoApi.Web.Features.Todos.Mapping;

namespace TodoApi.Web.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddTodoServices(this IServiceCollection services)
    {
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<TodoService>();
        
        services.AddValidatorsFromAssemblyContaining<CreateTodoDtoValidator>();

        services.AddAutoMapper(cfg => {}, typeof(TodoProfile));

        return services;
    }
}
