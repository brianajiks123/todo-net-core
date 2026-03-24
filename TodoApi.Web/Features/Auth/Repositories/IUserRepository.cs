namespace TodoApi.Web.Features.Auth.Repositories;

public interface IUserRepository
{
    Task<User?> GetByUsernameAsync(string username, CancellationToken ct = default);
    Task<bool> ExistsByUsernameAsync(string username, CancellationToken ct = default);
    Task<User?> GetByIdAsync(int id);
    
    Task AddAsync(User user);
}
