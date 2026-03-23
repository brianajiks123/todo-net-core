namespace TodoApi.Web.Features.Todos;

public class TodoService
{
    private readonly List<Todo> _todos = new()
    {
        new Todo(1, "Buy milk"),
        new Todo(2, "Call mom", true)
    };

    private int _nextId = 3;

    public List<Todo> GetAll() => _todos;

    public Todo? GetById(int id) => _todos.Find(t => t.Id == id);

    public Todo Create(string title)
    {
        var todo = new Todo(_nextId++, title);
        _todos.Add(todo);
        return todo;
    }

    public bool Update(int id, string? title, bool? isCompleted)
    {
        var todo = GetById(id);
        if (todo is null) return false;

        if (title is not null) title = title.Trim();

        if (string.IsNullOrWhiteSpace(title) && isCompleted is null)
            return false;

        var updated = todo with
        {
            Title = title ?? todo.Title,
            IsCompleted = isCompleted ?? todo.IsCompleted
        };

        var index = _todos.IndexOf(todo);
        _todos[index] = updated;
        return true;
    }

    public bool Delete(int id)
    {
        var todo = GetById(id);
        if (todo is null) return false;
        return _todos.Remove(todo);
    }
}
