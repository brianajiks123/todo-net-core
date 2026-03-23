using Microsoft.EntityFrameworkCore;

namespace TodoApi.Web.Features.Todos;

public class TodoService
{
    private readonly TodoDbContext _context;

    public TodoService(TodoDbContext context)
    {
        _context = context;
    }

    public async Task<List<Todo>> GetAll() => await _context.Todos.ToListAsync();

    public async Task<Todo?> GetById(int id) => await _context.Todos.FindAsync(id);

    public async Task<Todo> Create(string title)
    {
        var todo = new Todo(title);
        _context.Todos.Add(todo);
        await _context.SaveChangesAsync();
        return todo;
    }

    public async Task<bool> Update(int id, string? title, bool? isCompleted)
    {
        var todo = await GetById(id);
        if (todo is null) return false;

        if (title is not null) todo.Title = title.Trim();
        if (isCompleted.HasValue) todo.IsCompleted = isCompleted.Value;

        if (string.IsNullOrWhiteSpace(todo.Title))
            return false;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> Delete(int id)
    {
        var todo = await GetById(id);
        if (todo is null) return false;

        _context.Todos.Remove(todo);
        await _context.SaveChangesAsync();
        return true;
    }

    // Pagination
    public async Task<PagedResult<Todo>> GetPaged(TodoQueryParams query)
    {
        var todos = _context.Todos.AsQueryable();

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
                "title" => direction == "desc" ? todos.OrderByDescending(t => t.Title) : todos.OrderBy(t => t.Title),
                "createdat" => direction == "desc" ? todos.OrderByDescending(t => t.CreatedAt) : todos.OrderBy(t => t.CreatedAt),
                "iscompleted" => direction == "desc" ? todos.OrderByDescending(t => t.IsCompleted) : todos.OrderBy(t => t.IsCompleted),
                _ => todos.OrderByDescending(t => t.CreatedAt)
            };
        }
        else
        {
            todos = todos.OrderByDescending(t => t.CreatedAt);
        }

        var totalItems = await todos.CountAsync();

        var page = query.Page ?? 1;     // default 1 if null
        var size = query.Size ?? 10;    // default 10 if null

        page = Math.Max(1, page);
        size = Math.Clamp(size, 1, 50);

        var skip = (page - 1) * size;

        var items = await todos.Skip(skip).Take(size).ToListAsync();

        return new PagedResult<Todo>
        {
            Items = items,
            Page = page,
            Size = size,
            TotalItems = totalItems,
            TotalPages = (int)Math.Ceiling(totalItems / (double)size)
        };
    }
}
