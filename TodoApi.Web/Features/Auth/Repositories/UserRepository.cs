using Microsoft.EntityFrameworkCore;

namespace TodoApi.Web.Features.Auth.Repositories;

public class UserRepository : IUserRepository
{
    private readonly TodoDbContext _context;
    
    public UserRepository(TodoDbContext context) => _context = context;

    public async Task<User?> GetByUsernameAsync(string username, CancellationToken ct = default)
        => await _context.Users.FirstOrDefaultAsync(u => u.Username.ToLower() == username.ToLower(), ct);

    public async Task<bool> ExistsByUsernameAsync(string username, CancellationToken ct = default)
        => await _context.Users.AnyAsync(u => u.Username.ToLower() == username.ToLower(), ct);

    public async Task AddAsync(User user) => await _context.Users.AddAsync(user);
    public async Task<User?> GetByIdAsync(int id) => await _context.Users.FindAsync(id);
}
