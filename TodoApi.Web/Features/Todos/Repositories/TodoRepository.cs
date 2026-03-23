using Microsoft.EntityFrameworkCore;

namespace TodoApi.Web.Features.Todos;

public class TodoRepository : ITodoRepository
{
    private readonly TodoDbContext _context;

    public TodoRepository(TodoDbContext context)
    {
        _context = context;
    }

    public async Task<List<Todo>> GetAllAsync() 
        => await _context.Todos.ToListAsync();

    public async Task<Todo?> GetByIdAsync(int id) 
        => await _context.Todos.FindAsync(id);

    public async Task AddAsync(Todo todo) 
        => await _context.Todos.AddAsync(todo);

    public void Update(Todo todo) 
        => _context.Todos.Update(todo);

    public void Delete(Todo todo) 
        => _context.Todos.Remove(todo);

    public async Task<PagedResult<Todo>> GetPagedAsync(TodoQueryParams query)
    {
        var todos = _context.Todos.AsQueryable();

        // Filtering
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
                _ => todos.OrderByDescending(t => t.CreatedAt)
            };
        }
        else
        {
            todos = todos.OrderByDescending(t => t.CreatedAt);
        }

        var totalItems = await todos.CountAsync();
        var page = query.Page ?? 1;
        var size = query.Size ?? 10;
        
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
