namespace TodoApi.Web.Features.Todos;

public interface ITodoRepository
{
    Task<List<Todo>> GetAllAsync();
    Task<Todo?> GetByIdAsync(int id);
    Task AddAsync(Todo todo);
    void Update(Todo todo);
    void Delete(Todo todo);
    Task<PagedResult<Todo>> GetPagedAsync(TodoQueryParams query);
    Task<bool> ExistsWithTitleAsync(string title, CancellationToken ct = default);
}
