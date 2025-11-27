using System.Security.Claims;
using ERAMonitor.Core.DTOs;
using ERAMonitor.Core.Entities;
using ERAMonitor.Core.Enums;
using ERAMonitor.Core.Interfaces;
using ERAMonitor.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ERAMonitor.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class SettingsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IAuditLogService _auditLogService;

    public SettingsController(ApplicationDbContext context, IAuditLogService auditLogService)
    {
        _context = context;
        _auditLogService = auditLogService;
    }

    [HttpGet]
    public async Task<ActionResult<SystemSettingDto>> GetSettings()
    {
        var orgId = Guid.Parse(User.FindFirstValue("OrganizationId")!);
        var role = User.FindFirstValue(ClaimTypes.Role);

        if (role != nameof(UserRole.SuperAdmin))
        {
            return Forbid();
        }

        var settings = await _context.SystemSettings
            .FirstOrDefaultAsync(s => s.OrganizationId == orgId);

        if (settings == null)
        {
            // Create default settings if not exists
            settings = new SystemSetting { OrganizationId = orgId };
            _context.SystemSettings.Add(settings);
            await _context.SaveChangesAsync();
        }

        return new SystemSettingDto
        {
            Id = settings.Id,
            SmtpHost = settings.SmtpHost,
            SmtpPort = settings.SmtpPort,
            SmtpUsername = settings.SmtpUsername,
            SmtpFromEmail = settings.SmtpFromEmail,
            SmtpFromName = settings.SmtpFromName,
            SmtpUseSsl = settings.SmtpUseSsl,
            HasSmtpPassword = !string.IsNullOrEmpty(settings.SmtpPasswordEncrypted),
            HasTelegramBotToken = !string.IsNullOrEmpty(settings.TelegramBotTokenEncrypted),
            DefaultCheckIntervalSeconds = settings.DefaultCheckIntervalSeconds,
            DefaultAlertDelaySeconds = settings.DefaultAlertDelaySeconds,
            RetentionDaysMetrics = settings.RetentionDaysMetrics,
            RetentionDaysLogs = settings.RetentionDaysLogs
        };
    }

    [HttpPut]
    public async Task<ActionResult<SystemSettingDto>> UpdateSettings(UpdateSystemSettingRequest request)
    {
        var orgId = Guid.Parse(User.FindFirstValue("OrganizationId")!);
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var role = User.FindFirstValue(ClaimTypes.Role);

        if (role != nameof(UserRole.SuperAdmin))
        {
            return Forbid();
        }

        var settings = await _context.SystemSettings
            .FirstOrDefaultAsync(s => s.OrganizationId == orgId);

        if (settings == null)
        {
            settings = new SystemSetting { OrganizationId = orgId };
            _context.SystemSettings.Add(settings);
        }

        // Keep old values for audit
        var oldValues = new { settings.SmtpHost, settings.DefaultCheckIntervalSeconds };

        settings.SmtpHost = request.SmtpHost;
        settings.SmtpPort = request.SmtpPort;
        settings.SmtpUsername = request.SmtpUsername;
        if (!string.IsNullOrEmpty(request.SmtpPassword))
        {
            settings.SmtpPasswordEncrypted = request.SmtpPassword; // Should encrypt here
        }
        settings.SmtpFromEmail = request.SmtpFromEmail;
        settings.SmtpFromName = request.SmtpFromName;
        settings.SmtpUseSsl = request.SmtpUseSsl;
        if (!string.IsNullOrEmpty(request.TelegramBotToken))
        {
            settings.TelegramBotTokenEncrypted = request.TelegramBotToken; // Should encrypt here
        }
        settings.DefaultCheckIntervalSeconds = request.DefaultCheckIntervalSeconds;
        settings.DefaultAlertDelaySeconds = request.DefaultAlertDelaySeconds;
        settings.RetentionDaysMetrics = request.RetentionDaysMetrics;
        settings.RetentionDaysLogs = request.RetentionDaysLogs;

        await _context.SaveChangesAsync();

        await _auditLogService.LogAsync(orgId, userId, "UpdateSettings", "SystemSetting", (Guid?)settings.Id, "System Settings", oldValues, request, HttpContext.Connection.RemoteIpAddress?.ToString(), Request.Headers["User-Agent"]);

        return Ok(new SystemSettingDto
        {
            Id = settings.Id,
            SmtpHost = settings.SmtpHost,
            SmtpPort = settings.SmtpPort,
            SmtpUsername = settings.SmtpUsername,
            SmtpFromEmail = settings.SmtpFromEmail,
            SmtpFromName = settings.SmtpFromName,
            SmtpUseSsl = settings.SmtpUseSsl,
            HasSmtpPassword = !string.IsNullOrEmpty(settings.SmtpPasswordEncrypted),
            HasTelegramBotToken = !string.IsNullOrEmpty(settings.TelegramBotTokenEncrypted),
            DefaultCheckIntervalSeconds = settings.DefaultCheckIntervalSeconds,
            DefaultAlertDelaySeconds = settings.DefaultAlertDelaySeconds,
            RetentionDaysMetrics = settings.RetentionDaysMetrics,
            RetentionDaysLogs = settings.RetentionDaysLogs
        });
    }
}
