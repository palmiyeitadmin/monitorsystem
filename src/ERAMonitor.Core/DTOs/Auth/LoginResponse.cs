using ERAMonitor.Core.DTOs.Users;

namespace ERAMonitor.Core.DTOs.Auth;

public class LoginResponse
{
    public string? AccessToken { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime? AccessTokenExpires { get; set; }
    public DateTime? RefreshTokenExpires { get; set; }
    public UserDto? User { get; set; }
    public bool RequiresTwoFactor { get; set; }
}
