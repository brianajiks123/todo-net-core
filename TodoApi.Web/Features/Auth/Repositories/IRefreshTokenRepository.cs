namespace TodoApi.Web.Features.Auth.Repositories;

public interface IRefreshTokenRepository
{
    Task AddAsync(RefreshToken refreshToken);

    Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken ct = default);
    
    void Update(RefreshToken refreshToken);
}
