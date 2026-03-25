using Microsoft.EntityFrameworkCore;
using TodoApi.Web.Features.Auth;

namespace TodoApi.Web.Features.Todos;

public class TodoDbContext : DbContext
{
    public DbSet<Todo> Todos => Set<Todo>();
    public DbSet<User> Users => Set<User>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    public TodoDbContext(DbContextOptions<TodoDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Todo Configuration
        modelBuilder.Entity<Todo>(entity =>
        {
            entity.HasKey(t => t.Id);
            entity.Property(t => t.Title).IsRequired().HasMaxLength(200);
            entity.Property(t => t.CreatedBy).IsRequired();
            entity.Property(t => t.CreatedAt).IsRequired();
            entity.Property(t => t.UpdatedBy).IsRequired();
            entity.Property(t => t.UpdatedAt).IsRequired();
            entity.Property(t => t.UserId).IsRequired();

            entity.Property(t => t.IsDeleted)
                .IsRequired()
                .HasDefaultValue(false);

            entity.HasQueryFilter(t => !t.IsDeleted);

            entity.HasOne(t => t.User)
                .WithMany(u => u.Todos)
                .HasForeignKey(t => t.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // User Configuration
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(u => u.Id);
            entity.Property(u => u.Username).IsRequired().HasMaxLength(50);
            entity.HasIndex(u => u.Username).IsUnique();
            entity.Property(u => u.PasswordHash).IsRequired();
            entity.Property(u => u.CreatedAt).IsRequired();
        });

        // RefreshToken Configuration
        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.HasKey(rt => rt.Id);
            entity.Property(rt => rt.Token).IsRequired().HasMaxLength(256);
            entity.HasIndex(rt => rt.Token).IsUnique();
            entity.Property(rt => rt.ExpiresAt).IsRequired();
            entity.Property(rt => rt.IsRevoked).IsRequired();
            entity.Property(rt => rt.CreatedAt).IsRequired();

            entity.HasOne(rt => rt.User)
                  .WithMany()
                  .HasForeignKey(rt => rt.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
