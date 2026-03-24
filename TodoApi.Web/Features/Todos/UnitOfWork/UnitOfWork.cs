using TodoApi.Web.Features.Auth.Repositories;

namespace TodoApi.Web.Features.Todos;

public class UnitOfWork : IUnitOfWork
{
    private readonly TodoDbContext _context;
    private readonly Lazy<ITodoRepository> _todoRepository;
    private readonly Lazy<IUserRepository> _userRepository;
    private readonly Lazy<IRefreshTokenRepository> _refreshTokenRepository;

    public ITodoRepository Todos => _todoRepository.Value;
    public IUserRepository Users => _userRepository.Value;
    public IRefreshTokenRepository RefreshTokens => _refreshTokenRepository.Value;

    public UnitOfWork(TodoDbContext context)
    {
        _context = context;

        _todoRepository = new Lazy<ITodoRepository>(() => new TodoRepository(context));
        _userRepository = new Lazy<IUserRepository>(() => new UserRepository(context));
        _refreshTokenRepository = new Lazy<IRefreshTokenRepository>(() => new RefreshTokenRepository(context));
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
