using System.ComponentModel.DataAnnotations;

namespace TodoApi.Web.Features.Auth;

public record RegisterDto(
    [Required] [StringLength(50, MinimumLength = 3)] string Username,
    [Required] [StringLength(100, MinimumLength = 6)] string Password
);

public record LoginDto(
    [Required] string Username,
    [Required] string Password
);

public record AuthResponseDto(string AccessToken, string RefreshToken);

public record RefreshRequestDto(string RefreshToken);
