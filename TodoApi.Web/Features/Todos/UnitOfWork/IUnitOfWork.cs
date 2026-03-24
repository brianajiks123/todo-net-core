using TodoApi.Web.Features.Auth.Repositories;

namespace TodoApi.Web.Features.Todos;

public interface IUnitOfWork : IDisposable
{
    ITodoRepository Todos { get; }
    IUserRepository Users { get; }
    IRefreshTokenRepository RefreshTokens { get; }
    
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
