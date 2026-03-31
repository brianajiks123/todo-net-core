using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using TodoApi.Web.Features.Auth.Repositories;
using TodoApi.Web.Features.Todos;
using static BCrypt.Net.BCrypt;

namespace TodoApi.Web.Features.Auth;

public class AuthService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IConfiguration _configuration;

    public AuthService(IUnitOfWork unitOfWork, IConfiguration configuration)
    {
        _unitOfWork = unitOfWork;
        _configuration = configuration;
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto)
    {
        var passwordHash = HashPassword(dto.Password);

        var user = new User
        {
            Username = dto.Username.Trim(),
            PasswordHash = passwordHash
        };

        await _unitOfWork.Users.AddAsync(user);
        await _unitOfWork.SaveChangesAsync();

        return await GenerateTokensAsync(user);
    }

    public async Task<AuthResponseDto?> LoginAsync(LoginDto dto)
    {
        var user = await _unitOfWork.Users.GetByUsernameAsync(dto.Username);
        if (user == null || !Verify(dto.Password, user.PasswordHash))
            return null;

        return await GenerateTokensAsync(user);
    }

    public async Task<AuthResponseDto?> RefreshTokenAsync(string refreshTokenStr)
    {
        var refresh = await _unitOfWork.RefreshTokens.GetByTokenAsync(refreshTokenStr);
        if (refresh?.User == null)
            return null;

        refresh.IsRevoked = true;
        _unitOfWork.RefreshTokens.Update(refresh);

        return await GenerateTokensAsync(refresh.User);
    }

    private async Task<AuthResponseDto> GenerateTokensAsync(User user)
    {
        var jwtSection = _configuration.GetSection("Jwt");
        var secret = jwtSection["Secret"] 
            ?? throw new InvalidOperationException("JWT Secret not found. Set it with: dotnet user-secrets set Jwt:Secret \"your-very-long-secret-min-32-chars\"");

        var issuer = jwtSection["Issuer"] ?? "todo-api";
        var audience = jwtSection["Audience"] ?? "todo-api";
        var expHours = int.Parse(jwtSection["ExpirationHours"] ?? "1");
        var refreshDays = int.Parse(jwtSection["RefreshExpirationDays"] ?? "7");

        // Generate Access Token (JWT)
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(secret);

        var claims = new[]
        {
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddHours(expHours),
            Issuer = issuer,
            Audience = audience,
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var accessToken = tokenHandler.WriteToken(tokenHandler.CreateToken(tokenDescriptor));

        // Generate Refresh Token
        var refreshTokenStr = GenerateRefreshTokenString();
        var refreshEntity = new RefreshToken
        {
            Token = refreshTokenStr,
            UserId = user.Id,
            ExpiresAt = DateTime.UtcNow.AddDays(refreshDays),
            IsRevoked = false
        };

        await _unitOfWork.RefreshTokens.AddAsync(refreshEntity);
        await _unitOfWork.SaveChangesAsync();

        return new AuthResponseDto(accessToken, refreshTokenStr);
    }

    private static string GenerateRefreshTokenString()
    {
        var randomNumber = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }
}
