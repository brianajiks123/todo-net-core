namespace TodoApi.Web.Features.Todos;

public class Todo
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public bool IsCompleted { get; set; }
    public DateTime CreatedAt { get; set; }
    public int CreatedBy { get; set; }
    public int UpdatedBy { get; set; }
    public DateTime UpdatedAt { get; set; }
    public bool IsDeleted { get; set; } = false;
    public int UserId { get; set; }
    public User User { get; set; } = null!;
    public DateTime? DueDate { get; set; }

    public Todo() { }

    public Todo(string title)
    {
        Title = title;
        CreatedAt = DateTime.UtcNow;
    }
}
