using System.ComponentModel.DataAnnotations;

namespace ERAMonitor.Core.DTOs.Auth;

public class RefreshTokenRequest
{
    [Required(ErrorMessage = "Refresh token is required")]
    public string RefreshToken { get; set; } = string.Empty;

    public string? AccessToken { get; set; }
}
