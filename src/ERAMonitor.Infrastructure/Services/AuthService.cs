using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ERAMonitor.Core.Configuration;
using ERAMonitor.Core.DTOs.Auth;
using ERAMonitor.Core.DTOs.Users;
using ERAMonitor.Core.Entities;
using ERAMonitor.Core.Exceptions;
using ERAMonitor.Core.Interfaces.Repositories;
using ERAMonitor.Core.Interfaces.Services;
using ERAMonitor.Core.Enums;
using OtpNet;
using System.Security.Claims;

namespace ERAMonitor.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;
    private readonly IEmailService _emailService;
    private readonly IAuditService _auditService;
    private readonly JwtSettings _jwtSettings;
    private readonly ILogger<AuthService> _logger;
    
    public AuthService(
        IUnitOfWork unitOfWork,
        IPasswordHasher passwordHasher,
        ITokenService tokenService,
        IEmailService emailService,
        IAuditService auditService,
        IOptions<JwtSettings> jwtSettings,
        ILogger<AuthService> logger)
    {
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
        _emailService = emailService;
        _auditService = auditService;
        _jwtSettings = jwtSettings.Value;
        _logger = logger;
    }
    
    public async Task<LoginResponse> LoginAsync(LoginRequest request, string ipAddress, string userAgent)
    {
        var user = await _unitOfWork.Users.GetByEmailAsync(request.Email.ToLowerInvariant());
        
        if (user == null)
        {
            // Don't reveal user existence
            throw new UnauthorizedException("Invalid email or password");
        }
        
        if (!user.IsActive)
        {
            throw new UnauthorizedException("Account is deactivated");
        }
        
        if (user.IsLocked)
        {
            throw new UnauthorizedException($"Account is locked until {user.LockedUntil}");
        }
        
        if (!_passwordHasher.Verify(request.Password, user.PasswordHash!))
        {
            user.FailedLoginAttempts++;
            if (user.FailedLoginAttempts >= 5)
            {
                user.LockedUntil = DateTime.UtcNow.AddMinutes(15);
                user.FailedLoginAttempts = 0;
            }
            
            _unitOfWork.Users.Update(user);
            await _unitOfWork.SaveChangesAsync();
            
            await _auditService.LogLoginAsync(user.Id, ipAddress, false, "Invalid password");
            
            throw new UnauthorizedException("Invalid email or password");
        }
        
        // Reset failed attempts
        user.FailedLoginAttempts = 0;
        user.LockedUntil = null;
        user.LastLoginAt = DateTime.UtcNow;
        user.LastLoginIp = ipAddress;
        
        // 2FA Check
        if (user.TwoFactorEnabled)
        {
            if (string.IsNullOrEmpty(request.TwoFactorCode))
            {
                return new LoginResponse { RequiresTwoFactor = true };
            }
            
            if (!VerifyTwoFactorCode(user.TwoFactorSecret!, request.TwoFactorCode))
            {
                // Check backup codes
                if (user.TwoFactorBackupCodes != null && user.TwoFactorBackupCodes.Contains(request.TwoFactorCode))
                {
                    // Remove used backup code
                    user.TwoFactorBackupCodes = user.TwoFactorBackupCodes.Where(c => c != request.TwoFactorCode).ToArray();
                }
                else
                {
                    await _auditService.LogLoginAsync(user.Id, ipAddress, false, "Invalid 2FA code");
                    throw new UnauthorizedException("Invalid 2FA code");
                }
            }
        }
        
        _unitOfWork.Users.Update(user);
        
        // Generate Tokens
        var accessToken = _tokenService.GenerateAccessToken(user);
        var refreshToken = _tokenService.GenerateRefreshToken();
        var refreshTokenHash = _tokenService.HashToken(refreshToken);
        
        // Create Session
        var (deviceName, deviceType, browser, os) = ParseUserAgent(userAgent);
        
        var session = new UserSession
        {
            UserId = user.Id,
            TokenHash = refreshTokenHash,
            IpAddress = ipAddress,
            UserAgent = userAgent,
            DeviceName = deviceName,
            DeviceType = deviceType,
            Browser = browser,
            OperatingSystem = os,
            ExpiresAt = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays)
        };
        
        await _unitOfWork.UserSessions.AddAsync(session);
        await _unitOfWork.SaveChangesAsync();
        
        await _auditService.LogLoginAsync(user.Id, ipAddress, true);
        
        return new LoginResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            AccessTokenExpires = DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes),
            RefreshTokenExpires = session.ExpiresAt,
            User = MapToUserDto(user)
        };
    }
    
    public async Task<RefreshTokenResponse> RefreshTokenAsync(string refreshToken, string ipAddress)
    {
        var tokenHash = _tokenService.HashToken(refreshToken);
        var session = await _unitOfWork.UserSessions.GetByRefreshTokenHashAsync(tokenHash);
        
        if (session == null || !session.IsValid)
        {
            throw new UnauthorizedException("Invalid or expired refresh token");
        }
        
        var user = await _unitOfWork.Users.GetByIdAsync(session.UserId);
        if (user == null || !user.IsActive)
        {
            throw new UnauthorizedException("User not found or deactivated");
        }
        
        // Generate new tokens
        var newAccessToken = _tokenService.GenerateAccessToken(user);
        var newRefreshToken = _tokenService.GenerateRefreshToken();
        var newRefreshTokenHash = _tokenService.HashToken(newRefreshToken);
        
        // Update session with new refresh token (token rotation)
        session.TokenHash = newRefreshTokenHash;
        session.LastActiveAt = DateTime.UtcNow;
        session.IpAddress = ipAddress;
        
        _unitOfWork.UserSessions.Update(session);
        await _unitOfWork.SaveChangesAsync();
        
        return new RefreshTokenResponse
        {
            AccessToken = newAccessToken,
            RefreshToken = newRefreshToken,
            AccessTokenExpires = DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes),
            RefreshTokenExpires = session.ExpiresAt
        };
    }
    
    public async Task LogoutAsync(Guid userId, string refreshToken)
    {
        var tokenHash = _tokenService.HashToken(refreshToken);
        var session = await _unitOfWork.UserSessions.GetByRefreshTokenHashAsync(tokenHash);
        
        if (session != null && session.UserId == userId)
        {
            session.IsRevoked = true;
            session.RevokedAt = DateTime.UtcNow;
            session.RevokedReason = "User logout";
            
            _unitOfWork.UserSessions.Update(session);
            await _unitOfWork.SaveChangesAsync();
        }
        
        await _auditService.LogLogoutAsync(userId);
    }
    
    public async Task LogoutAllAsync(Guid userId)
    {
        await _unitOfWork.UserSessions.RevokeAllUserSessionsAsync(userId, "User requested logout from all devices");
        await _unitOfWork.SaveChangesAsync();
        await _auditService.LogAsync("LogoutAll", "User", userId, null);
    }
    
    public async Task<User> RegisterAsync(CreateUserRequest request, Guid organizationId)
    {
        // Check if email exists
        if (await _unitOfWork.Users.EmailExistsAsync(request.Email.ToLowerInvariant()))
        {
            throw new BusinessException("Email already exists", "EMAIL_EXISTS");
        }
        
        var user = new User
        {
            OrganizationId = organizationId,
            Email = request.Email.ToLowerInvariant(),
            PasswordHash = _passwordHasher.Hash(request.Password),
            FullName = request.FullName,
            PhoneNumber = request.Phone,
            JobTitle = request.JobTitle,
            Role = request.Role,
            Timezone = request.Timezone,
            EmailVerificationToken = GenerateRandomToken(),
            EmailVerificationTokenExpires = DateTime.UtcNow.AddHours(24)
        };
        
        await _unitOfWork.Users.AddAsync(user);
        
        // Assign customers if provided
        if (request.AssignedCustomerIds?.Any() == true)
        {
            foreach (var customerId in request.AssignedCustomerIds)
            {
                var assignment = new UserCustomerAssignment
                {
                    UserId = user.Id,
                    CustomerId = customerId
                };
                user.CustomerAssignments.Add(assignment);
            }
        }
        
        await _unitOfWork.SaveChangesAsync();
        
        // Send welcome email
        if (request.SendWelcomeEmail)
        {
            try
            {
                await _emailService.SendWelcomeEmailAsync(user.Email, user.FullName, request.Password);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send welcome email to {Email}", user.Email);
            }
        }
        
        await _auditService.LogCreateAsync(user);
        
        return user;
    }
    
    public async Task ForgotPasswordAsync(string email)
    {
        var user = await _unitOfWork.Users.GetByEmailAsync(email.ToLowerInvariant());
        
        // Don't reveal if user exists or not
        if (user == null || !user.IsActive)
        {
            _logger.LogWarning("Password reset requested for non-existent/inactive email: {Email}", email);
            return;
        }
        
        // Generate reset token
        user.PasswordResetToken = GenerateRandomToken();
        user.PasswordResetTokenExpires = DateTime.UtcNow.AddHours(1);
        
        _unitOfWork.Users.Update(user);
        await _unitOfWork.SaveChangesAsync();
        
        // Send reset email
        var resetLink = $"https://monitor.eracloud.com.tr/reset-password?token={user.PasswordResetToken}&email={user.Email}";
        await _emailService.SendPasswordResetEmailAsync(user.Email, user.FullName, resetLink);
        
        await _auditService.LogAsync("PasswordResetRequested", "User", user.Id, user.Email);
        
        _logger.LogInformation("Password reset email sent to: {Email}", email);
    }
    
    public async Task ResetPasswordAsync(ResetPasswordRequest request)
    {
        var user = await _unitOfWork.Users.GetByPasswordResetTokenAsync(request.Token);
        
        if (user == null)
        {
            throw new BusinessException("Invalid or expired reset token", "INVALID_TOKEN");
        }
        
        if (user.Email.ToLowerInvariant() != request.Email.ToLowerInvariant())
        {
            throw new BusinessException("Invalid reset request", "INVALID_REQUEST");
        }
        
        if (user.PasswordResetTokenExpires < DateTime.UtcNow)
        {
            throw new BusinessException("Reset token has expired", "TOKEN_EXPIRED");
        }
        
        // Update password
        user.PasswordHash = _passwordHasher.Hash(request.NewPassword);
        user.PasswordResetToken = null;
        user.PasswordResetTokenExpires = null;
        user.FailedLoginAttempts = 0;
        user.LockedUntil = null;
        
        _unitOfWork.Users.Update(user);
        
        // Revoke all sessions for security
        await _unitOfWork.UserSessions.RevokeAllUserSessionsAsync(user.Id, "Password reset");
        
        await _unitOfWork.SaveChangesAsync();
        
        await _auditService.LogAsync("PasswordReset", "User", user.Id, user.Email);
        
        _logger.LogInformation("Password reset completed for: {Email}", user.Email);
    }
    
    public async Task ChangePasswordAsync(Guid userId, ChangePasswordRequest request)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        
        if (user == null)
        {
            throw new NotFoundException("User not found");
        }
        
        // Verify current password
        if (!_passwordHasher.Verify(request.CurrentPassword, user.PasswordHash!))
        {
            throw new BusinessException("Current password is incorrect", "INVALID_PASSWORD");
        }
        
        // Check if new password is same as old
        if (_passwordHasher.Verify(request.NewPassword, user.PasswordHash!))
        {
            throw new BusinessException("New password must be different from current password", "SAME_PASSWORD");
        }
        
        user.PasswordHash = _passwordHasher.Hash(request.NewPassword);
        _unitOfWork.Users.Update(user);
        await _unitOfWork.SaveChangesAsync();
        
        await _auditService.LogAsync("PasswordChanged", "User", userId, user.Email);
        
        _logger.LogInformation("Password changed for user: {Email}", user.Email);
    }
    
    public async Task<bool> VerifyEmailAsync(string token)
    {
        var user = await _unitOfWork.Users.GetByEmailVerificationTokenAsync(token);
        
        if (user == null)
        {
            return false;
        }
        
        if (user.EmailVerificationTokenExpires < DateTime.UtcNow)
        {
            return false;
        }
        
        user.EmailVerified = true;
        user.EmailVerificationToken = null;
        user.EmailVerificationTokenExpires = null;
        
        _unitOfWork.Users.Update(user);
        await _unitOfWork.SaveChangesAsync();
        
        await _auditService.LogAsync("EmailVerified", "User", user.Id, user.Email);
        
        return true;
    }
    
    public async Task ResendVerificationEmailAsync(string email)
    {
        var user = await _unitOfWork.Users.GetByEmailAsync(email.ToLowerInvariant());
        
        if (user == null || user.EmailVerified)
        {
            return;
        }
        
        user.EmailVerificationToken = GenerateRandomToken();
        user.EmailVerificationTokenExpires = DateTime.UtcNow.AddHours(24);
        
        _unitOfWork.Users.Update(user);
        await _unitOfWork.SaveChangesAsync();
        
        var verificationLink = $"https://monitor.eracloud.com.tr/verify-email?token={user.EmailVerificationToken}";
        await _emailService.SendEmailVerificationAsync(user.Email, user.FullName, verificationLink);
    }
    
    public async Task<TwoFactorSetupDto> GenerateTwoFactorSecretAsync(Guid userId)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        
        if (user == null)
        {
            throw new NotFoundException("User not found");
        }
        
        // Generate secret
        var key = KeyGeneration.GenerateRandomKey(20);
        var secret = Base32Encoding.ToString(key);
        
        // Generate QR code URL
        var issuer = "ERAMonitor";
        var qrCodeUrl = $"otpauth://totp/{issuer}:{user.Email}?secret={secret}&issuer={issuer}&algorithm=SHA1&digits=6&period=30";
        
        return new TwoFactorSetupDto
        {
            Secret = secret,
            QrCodeUrl = qrCodeUrl,
            ManualEntryKey = secret
        };
    }
    
    public async Task EnableTwoFactorAsync(Guid userId, string code)
    {
        var setupDto = await GenerateTwoFactorSecretAsync(userId);
        
        if (!VerifyTwoFactorCode(setupDto.Secret, code))
        {
            throw new BusinessException("Invalid verification code", "INVALID_2FA_CODE");
        }
        
        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        user!.TwoFactorEnabled = true;
        user.TwoFactorSecret = setupDto.Secret;
        user.TwoFactorBackupCodes = GenerateBackupCodes();
        
        _unitOfWork.Users.Update(user);
        await _unitOfWork.SaveChangesAsync();
        
        await _auditService.LogAsync("TwoFactorEnabled", "User", userId, user.Email);
    }
    
    public async Task DisableTwoFactorAsync(Guid userId, string password, string code)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        
        if (user == null)
        {
            throw new NotFoundException("User not found");
        }
        
        if (!_passwordHasher.Verify(password, user.PasswordHash!))
        {
            throw new BusinessException("Invalid password", "INVALID_PASSWORD");
        }
        
        if (!VerifyTwoFactorCode(user.TwoFactorSecret!, code))
        {
            throw new BusinessException("Invalid verification code", "INVALID_2FA_CODE");
        }
        
        user.TwoFactorEnabled = false;
        user.TwoFactorSecret = null;
        user.TwoFactorBackupCodes = null;
        
        _unitOfWork.Users.Update(user);
        await _unitOfWork.SaveChangesAsync();
        
        await _auditService.LogAsync("TwoFactorDisabled", "User", userId, user.Email);
    }
    
    public async Task<List<SessionDto>> GetUserSessionsAsync(Guid userId, string? currentSessionToken)
    {
        var sessions = await _unitOfWork.UserSessions.GetByUserIdAsync(userId);
        var currentTokenHash = currentSessionToken != null ? _tokenService.HashToken(currentSessionToken) : null;
        
        return sessions.Select(s => new SessionDto
        {
            Id = s.Id,
            DeviceName = s.DeviceName,
            DeviceType = s.DeviceType,
            Browser = s.Browser,
            OperatingSystem = s.OperatingSystem,
            IpAddress = s.IpAddress,
            Location = s.Location,
            CreatedAt = s.CreatedAt,
            LastActiveAt = s.LastActiveAt,
            ExpiresAt = s.ExpiresAt,
            IsCurrent = currentTokenHash != null && s.TokenHash == currentTokenHash
        }).ToList();
    }
    
    public async Task RevokeSessionAsync(Guid userId, Guid sessionId)
    {
        var session = await _unitOfWork.UserSessions.GetByIdAsync(sessionId);
        
        if (session == null || session.UserId != userId)
        {
            throw new NotFoundException("Session not found");
        }
        
        session.IsRevoked = true;
        session.RevokedAt = DateTime.UtcNow;
        session.RevokedReason = "Revoked by user";
        
        _unitOfWork.UserSessions.Update(session);
        await _unitOfWork.SaveChangesAsync();
        
        await _auditService.LogAsync("SessionRevoked", "UserSession", sessionId, null, userId: userId);
    }
    
    // Private helper methods
    
    private static string GenerateRandomToken()
    {
        var randomBytes = new byte[32];
        using var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToHexString(randomBytes).ToLower();
    }
    
    private static string[] GenerateBackupCodes()
    {
        var codes = new string[10];
        for (int i = 0; i < 10; i++)
        {
            codes[i] = Guid.NewGuid().ToString("N")[..8].ToUpper();
        }
        return codes;
    }
    
    private static bool VerifyTwoFactorCode(string secret, string code)
    {
        var totp = new Totp(Base32Encoding.ToBytes(secret));
        return totp.VerifyTotp(code, out _, new VerificationWindow(previous: 1, future: 1));
    }
    
    private static (string DeviceName, string DeviceType, string Browser, string OperatingSystem) ParseUserAgent(string userAgent)
    {
        // Simple user agent parsing - can be enhanced with UAParser library
        var deviceType = "Desktop";
        var browser = "Unknown";
        var os = "Unknown";
        var deviceName = "Unknown Device";
        
        if (userAgent.Contains("Mobile"))
            deviceType = "Mobile";
        else if (userAgent.Contains("Tablet"))
            deviceType = "Tablet";
        
        if (userAgent.Contains("Chrome"))
            browser = "Chrome";
        else if (userAgent.Contains("Firefox"))
            browser = "Firefox";
        else if (userAgent.Contains("Safari"))
            browser = "Safari";
        else if (userAgent.Contains("Edge"))
            browser = "Edge";
        
        if (userAgent.Contains("Windows"))
            os = "Windows";
        else if (userAgent.Contains("Mac"))
            os = "macOS";
        else if (userAgent.Contains("Linux"))
            os = "Linux";
        else if (userAgent.Contains("Android"))
            os = "Android";
        else if (userAgent.Contains("iOS") || userAgent.Contains("iPhone"))
            os = "iOS";
        
        deviceName = $"{browser} on {os}";
        
        return (deviceName, deviceType, browser, os);
    }
    
    private static UserDto MapToUserDto(User user)
    {
        return new UserDto
        {
            Id = user.Id,
            Email = user.Email,
            FullName = user.FullName,
            AvatarUrl = user.AvatarUrl,
            Role = user.Role.ToString(),
            OrganizationId = user.OrganizationId,
            Timezone = user.Timezone,
            TwoFactorEnabled = user.TwoFactorEnabled,
            LastLoginAt = user.LastLoginAt
        };
    }
}
