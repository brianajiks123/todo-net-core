using Microsoft.EntityFrameworkCore;

namespace TodoApi.Web.Features.Auth.Repositories;

public class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly TodoDbContext _context;
    
    public RefreshTokenRepository(TodoDbContext context) => _context = context;

    public async Task AddAsync(RefreshToken refreshToken) => await _context.RefreshTokens.AddAsync(refreshToken);
    public async Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken ct = default)
        => await _context.RefreshTokens
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.Token == token && !rt.IsRevoked && rt.ExpiresAt > DateTime.UtcNow, ct);

    public void Update(RefreshToken refreshToken) => _context.RefreshTokens.Update(refreshToken);
}
