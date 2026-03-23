using Microsoft.EntityFrameworkCore;

namespace TodoApi.Web.Features.Todos;

public class TodoDbContext : DbContext
{
    public DbSet<Todo> Todos => Set<Todo>();

    public TodoDbContext(DbContextOptions<TodoDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Todo>(entity =>
        {
            entity.HasKey(t => t.Id);
            entity.Property(t => t.Title).IsRequired().HasMaxLength(200);
            entity.Property(t => t.CreatedAt).IsRequired();
        });

        // Seed first data
        modelBuilder.Entity<Todo>().HasData(
            new Todo 
            { 
                Id = 1, 
                Title = "Buy milk", 
                IsCompleted = false, 
                CreatedAt = new DateTime(2025, 3, 20, 0, 0, 0, DateTimeKind.Utc)
            },
            new Todo 
            { 
                Id = 2, 
                Title = "Call mom", 
                IsCompleted = true, 
                CreatedAt = new DateTime(2025, 3, 21, 0, 0, 0, DateTimeKind.Utc)
            }
        );
    }
}
