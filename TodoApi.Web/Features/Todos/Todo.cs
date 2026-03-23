namespace TodoApi.Web.Features.Todos;

public class Todo
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public bool IsCompleted { get; set; }
    public DateTime CreatedAt { get; set; }

    public Todo() { }

    public Todo(string title)
    {
        Title = title;
        CreatedAt = DateTime.UtcNow;
    }
}
