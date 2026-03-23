namespace TodoApi.Web.Extensions;

using Microsoft.Extensions.DependencyInjection;
using TodoApi.Web.Features.Todos;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddTodoServices(this IServiceCollection services)
    {
        services.AddScoped<TodoService>();
        return services;
    }
}
