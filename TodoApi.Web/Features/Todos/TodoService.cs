namespace TodoApi.Web.Features.Todos;

public class TodoService
{
    private readonly List<Todo> _todos = new()
    {
        new Todo(1, "Buy milk"),
        new Todo(2, "Call mom", true),
        new Todo(3, "Meeting with team", false, DateTime.UtcNow.AddDays(-2)),
        new Todo(4, "Finish report", true),
        new Todo(5, "Buy groceries", false),
        new Todo(6, "Call dad", false),
    };

    private int _nextId = 7;

    public PagedResult<Todo> GetPaged(TodoQueryParams query)
    {
        var todos = _todos.AsQueryable();

        // Filtering: search by title (case-insensitive)
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var searchLower = query.Search.Trim().ToLowerInvariant();
            todos = todos.Where(t => t.Title.ToLowerInvariant().Contains(searchLower));
        }

        // Sorting
        if (!string.IsNullOrWhiteSpace(query.Sort))
        {
            var parts = query.Sort.Split(':');
            var field = parts[0].ToLowerInvariant();
            var direction = parts.Length > 1 && parts[1].ToLowerInvariant() == "desc" ? "desc" : "asc";

            todos = field switch
            {
                "title" => direction == "desc"
                    ? todos.OrderByDescending(t => t.Title)
                    : todos.OrderBy(t => t.Title),
                "createdat" => direction == "desc"
                    ? todos.OrderByDescending(t => t.CreatedAt)
                    : todos.OrderBy(t => t.CreatedAt),
                "iscompleted" => direction == "desc"
                    ? todos.OrderByDescending(t => t.IsCompleted)
                    : todos.OrderBy(t => t.IsCompleted),
                _ => todos.OrderByDescending(t => t.CreatedAt) // default
            };
        }
        else
        {
            // Default sort: newest first
            todos = todos.OrderByDescending(t => t.CreatedAt);
        }

        var totalItems = todos.Count();

        // Pagination
        var page = query.Page ?? 1;     // default 1 if null
        var size = query.Size ?? 10;    // default 10 if null

        page = Math.Max(1, page);
        size = Math.Clamp(size, 1, 50);

        var skip = (page - 1) * size;
        var items = todos.Skip(skip).Take(size).ToList();

        return new PagedResult<Todo>
        {
            Items = items,
            Page = page,
            Size = size,
            TotalItems = totalItems,
            TotalPages = (int)Math.Ceiling(totalItems / (double)size)
        };
    }

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
