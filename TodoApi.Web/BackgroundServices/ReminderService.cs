using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TodoApi.Web.Features.Todos;

namespace TodoApi.Web.BackgroundServices;

public class ReminderService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ReminderService> _logger;
    // private readonly TimeSpan _interval = TimeSpan.FromMinutes(5); // Check each 5 minute
    private readonly TimeSpan _interval = TimeSpan.FromSeconds(10); // Check each 10 seconds

    public ReminderService(IServiceProvider serviceProvider, ILogger<ReminderService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("ReminderService started. Checking interval: {Interval}", _interval);

        using var timer = new PeriodicTimer(_interval);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CheckOverdueTodosAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while checking overdue todos");
            }

            await timer.WaitForNextTickAsync(stoppingToken);
        }

        _logger.LogInformation("ReminderService is stopping.");
    }

    private async Task CheckOverdueTodosAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TodoDbContext>();

        var now = DateTime.UtcNow;

        var overdueTodos = await dbContext.Todos
            .Include(t => t.User)
            .Where(t => !t.IsCompleted 
                     && !t.IsDeleted 
                     && t.DueDate.HasValue 
                     && t.DueDate.Value < now)
            .ToListAsync(cancellationToken);

        if (overdueTodos.Count == 0)
        {
            _logger.LogDebug("No overdue todos found at {Time}", now);
            return;
        }

        _logger.LogInformation("Found {Count} overdue todo(s) at {Time}", overdueTodos.Count, now);

        foreach (var todo in overdueTodos)
        {
            _logger.LogWarning("REMINDER - OVERDUE TODO: " +
                "Id={TodoId}, User={Username}, Title='{Title}', DueDate={DueDate:yyyy-MM-dd HH:mm} UTC",
                todo.Id,
                todo.User?.Username ?? "Unknown",
                todo.Title,
                todo.DueDate);
        }
    }
}
