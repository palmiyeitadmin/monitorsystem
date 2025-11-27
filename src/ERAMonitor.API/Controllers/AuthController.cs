using System.Security.Claims;
using ERAMonitor.Core.DTOs;
using ERAMonitor.Core.DTOs.Auth;
using ERAMonitor.Core.DTOs.Users;
using ERAMonitor.Core.Entities;
using ERAMonitor.Core.Enums;
using ERAMonitor.Core.Interfaces.Services;
using ERAMonitor.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ERAMonitor.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ITokenService _tokenService;
    private readonly IPasswordService _passwordService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(ApplicationDbContext context, ITokenService tokenService, IPasswordService passwordService, ILogger<AuthController> logger)
    {
        _context = context;
        _tokenService = tokenService;
        _passwordService = passwordService;
        _logger = logger;
    }

    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login(LoginRequest request)
    {
        var user = await _context.Users
            .Include(u => u.Organization)
            .FirstOrDefaultAsync(u => u.Email == request.Email);

        var inputHash = _passwordService.HashPassword(request.Password);
        if (user == null || !_passwordService.VerifyPassword(request.Password, user.PasswordHash!))
        {
            return Unauthorized("Invalid email or password");
        }

        if (!user.IsActive)
        {
            return Unauthorized("User account is inactive");
        }

        var accessToken = _tokenService.GenerateAccessToken(user);
        var refreshToken = _tokenService.GenerateRefreshToken();

        // Save refresh token
        var userSession = new UserSession
        {
            UserId = user.Id,
            TokenHash = refreshToken, // In production, hash this!
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
            DeviceInfo = Request.Headers["User-Agent"].ToString(),
            LastActiveAt = DateTime.UtcNow
        };

        _context.UserSessions.Add(userSession);
        await _context.SaveChangesAsync();

        return new LoginResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            User = new UserDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                Role = user.Role.ToString(),
                AvatarUrl = user.AvatarUrl,
                OrganizationId = user.OrganizationId
            }
        };
    }

    [HttpPost("register")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<ActionResult<UserDto>> Register(RegisterRequest request)
    {
        if (await _context.Users.AnyAsync(u => u.Email == request.Email))
        {
            return BadRequest("Email already exists");
        }

        var currentUserRole = User.FindFirstValue(ClaimTypes.Role);
        var currentOrgId = Guid.Parse(User.FindFirstValue("OrganizationId")!);

        // If creating a new organization (SuperAdmin only)
        Guid orgId = currentOrgId;
        if (!string.IsNullOrEmpty(request.OrganizationName))
        {
            if (currentUserRole != nameof(UserRole.SuperAdmin))
            {
                return Forbid("Only SuperAdmin can create new organizations");
            }

            var org = new Organization
            {
                Name = request.OrganizationName,
                Slug = request.OrganizationName.ToLower().Replace(" ", "-") // Simple slug generation
            };
            _context.Organizations.Add(org);
            await _context.SaveChangesAsync();
            orgId = org.Id;
        }

        var user = new User
        {
            Email = request.Email,
            FullName = request.FullName,
            PasswordHash = _passwordService.HashPassword(request.Password),
            OrganizationId = orgId,
            Role = UserRole.Viewer, // Default role
            IsActive = true
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(Me), new UserDto
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            Role = user.Role.ToString(),
            OrganizationId = user.OrganizationId
        });
    }

    [HttpPost("refresh-token")]
    public async Task<ActionResult<LoginResponse>> RefreshToken(RefreshTokenRequest request)
    {
        var principal = _tokenService.GetPrincipalFromExpiredToken(request.AccessToken);
        var userId = Guid.Parse(principal.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var user = await _context.Users.FindAsync(userId);
        if (user == null) return Unauthorized();

        var session = await _context.UserSessions
            .FirstOrDefaultAsync(s => s.UserId == userId && s.TokenHash == request.RefreshToken);

        if (session == null || session.ExpiresAt < DateTime.UtcNow)
        {
            return Unauthorized("Invalid or expired refresh token");
        }

        var newAccessToken = _tokenService.GenerateAccessToken(user);
        var newRefreshToken = _tokenService.GenerateRefreshToken();

        // Rotate refresh token
        session.TokenHash = newRefreshToken;
        session.ExpiresAt = DateTime.UtcNow.AddDays(7);
        session.LastActiveAt = DateTime.UtcNow;
        
        await _context.SaveChangesAsync();

        return new LoginResponse
        {
            AccessToken = newAccessToken,
            RefreshToken = newRefreshToken,
            User = new UserDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                Role = user.Role.ToString(),
                AvatarUrl = user.AvatarUrl,
                OrganizationId = user.OrganizationId
            }
        };
    }

    [Authorize]
    [HttpPost("revoke-token")]
    public async Task<IActionResult> RevokeToken([FromBody] string refreshToken)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var session = await _context.UserSessions
            .FirstOrDefaultAsync(s => s.UserId == userId && s.TokenHash == refreshToken);

        if (session != null)
        {
            _context.UserSessions.Remove(session);
            await _context.SaveChangesAsync();
        }

        return NoContent();
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<UserDto>> Me()
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var user = await _context.Users.FindAsync(userId);

        if (user == null) return NotFound();

        return new UserDto
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            Role = user.Role.ToString(),
            AvatarUrl = user.AvatarUrl,
            OrganizationId = user.OrganizationId
        };
    }
}
