namespace TodoApi.Web.Features.Todos;

public record Todo(
    int Id,
    string Title,
    bool IsCompleted = false,
    DateTime CreatedAt = default
)

{
    public Todo() : this(0, "", false, DateTime.UtcNow) { }
}
