namespace TodoApi.Web.Features.Todos;

public class UnitOfWork : IUnitOfWork
{
    private readonly TodoDbContext _context;
    private readonly Lazy<ITodoRepository> _todoRepository;

    public UnitOfWork(TodoDbContext context)
    {
        _context = context;
        _todoRepository = new Lazy<ITodoRepository>(() => new TodoRepository(context));
    }

    public ITodoRepository Todos => _todoRepository.Value;

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
