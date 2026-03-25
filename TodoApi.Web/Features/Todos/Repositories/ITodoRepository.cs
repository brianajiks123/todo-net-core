namespace TodoApi.Web.Features.Todos;

public interface ITodoRepository
{
    Task<List<Todo>> GetAllByUserIdAsync(int userId);
    Task<Todo?> GetByIdAsync(int id, int userId);
    Task AddAsync(Todo todo);
    void Update(Todo todo);
    void Delete(Todo todo);
    Task<PagedResult<Todo>> GetPagedAsync(TodoQueryParams query, int userId);
    Task<bool> ExistsWithTitleAsync(string title, int userId, CancellationToken ct = default);
}
