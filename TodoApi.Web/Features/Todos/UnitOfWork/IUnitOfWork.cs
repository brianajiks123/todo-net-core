namespace TodoApi.Web.Features.Todos;

public interface IUnitOfWork : IDisposable
{
    ITodoRepository Todos { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
