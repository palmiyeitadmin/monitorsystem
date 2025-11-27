using System.Security.Claims;
using ERAMonitor.Core.DTOs;
using ERAMonitor.Core.Entities;
using ERAMonitor.Core.Enums;
using ERAMonitor.Core.Interfaces;
using ERAMonitor.Core.Interfaces.Services;
using ERAMonitor.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ERAMonitor.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class UserPreferencesController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IPasswordService _passwordService;
    private readonly IAuditLogService _auditLogService;

    public UserPreferencesController(ApplicationDbContext context, IPasswordService passwordService, IAuditLogService auditLogService)
    {
        _context = context;
        _passwordService = passwordService;
        _auditLogService = auditLogService;
    }

    [HttpGet("profile")]
    public async Task<ActionResult<UserProfileDto>> GetProfile()
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var user = await _context.Users.FindAsync(userId);

        if (user == null) return NotFound();

        return new UserProfileDto
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            Role = user.Role.ToString(),
            PhoneNumber = user.PhoneNumber,
            Theme = user.Theme
        };
    }

    [HttpPut("profile")]
    public async Task<ActionResult<UserProfileDto>> UpdateProfile(UpdateProfileRequest request)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var orgId = Guid.Parse(User.FindFirstValue("OrganizationId")!);
        var user = await _context.Users.FindAsync(userId);

        if (user == null) return NotFound();

        var oldValues = new { user.FullName, user.PhoneNumber, user.Theme };

        user.FullName = request.FullName;
        if (!string.IsNullOrEmpty(request.Email)) user.Email = request.Email;
        user.PhoneNumber = request.PhoneNumber;
        user.Theme = request.Theme;

        await _context.SaveChangesAsync();

        await _auditLogService.LogAsync(orgId, userId, "UpdateProfile", "User", (Guid?)user.Id, user.FullName, oldValues, request, HttpContext.Connection.RemoteIpAddress?.ToString(), Request.Headers["User-Agent"]);

        return Ok(new UserProfileDto
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            Role = user.Role.ToString(),
            PhoneNumber = user.PhoneNumber,
            Theme = user.Theme
        });
    }

    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword(ChangePasswordRequest request)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var orgId = Guid.Parse(User.FindFirstValue("OrganizationId")!);
        var user = await _context.Users.FindAsync(userId);

        if (user == null) return NotFound();

        if (string.IsNullOrEmpty(user.PasswordHash) || !_passwordService.VerifyPassword(request.CurrentPassword, user.PasswordHash))
        {
            return BadRequest("Invalid current password");
        }

        user.PasswordHash = _passwordService.HashPassword(request.NewPassword);
        await _context.SaveChangesAsync();

        await _auditLogService.LogAsync(orgId, userId, "ChangePassword", "User", (Guid?)user.Id, user.FullName, null, null, HttpContext.Connection.RemoteIpAddress?.ToString(), Request.Headers["User-Agent"]);

        return Ok("Password changed successfully");
    }
}
