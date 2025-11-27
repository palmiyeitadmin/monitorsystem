using ERAMonitor.Core.DTOs.Auth;
using ERAMonitor.Core.DTOs.Users;
using ERAMonitor.Core.Entities;

namespace ERAMonitor.Core.Interfaces.Services;

public interface IAuthService
{
    Task<LoginResponse> LoginAsync(LoginRequest request, string ipAddress, string userAgent);
    Task<RefreshTokenResponse> RefreshTokenAsync(string refreshToken, string ipAddress);
    Task LogoutAsync(Guid userId, string refreshToken);
    Task LogoutAllAsync(Guid userId);
    Task<User> RegisterAsync(CreateUserRequest request, Guid organizationId);
    Task ForgotPasswordAsync(string email);
    Task ResetPasswordAsync(ResetPasswordRequest request);
    Task ChangePasswordAsync(Guid userId, ChangePasswordRequest request);
    Task<bool> VerifyEmailAsync(string token);
    Task ResendVerificationEmailAsync(string email);
    Task<TwoFactorSetupDto> GenerateTwoFactorSecretAsync(Guid userId);
    Task EnableTwoFactorAsync(Guid userId, string code);
    Task DisableTwoFactorAsync(Guid userId, string password, string code);
    Task<List<SessionDto>> GetUserSessionsAsync(Guid userId, string? currentSessionToken);
    Task RevokeSessionAsync(Guid userId, Guid sessionId);
}
