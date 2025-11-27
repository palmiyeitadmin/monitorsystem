using System.Text.Json;
using ERAMonitor.Core.Entities;
using ERAMonitor.Core.Interfaces;
using ERAMonitor.Infrastructure.Data;

namespace ERAMonitor.Infrastructure.Services;

public class AuditLogService : IAuditLogService
{
    private readonly ApplicationDbContext _context;

    public AuditLogService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task LogAsync(Guid organizationId, Guid? userId, string action, string entityType, string entityId, string? entityName, object? oldValues, object? newValues, string? ipAddress, string? userAgent)
    {
        Guid? parsedEntityId = null;
        if (Guid.TryParse(entityId, out var id))
        {
            parsedEntityId = id;
        }

        await LogAsync(organizationId, userId, action, entityType, parsedEntityId, entityName, oldValues, newValues, ipAddress, userAgent);
    }

    public async Task LogAsync(Guid organizationId, Guid? userId, string action, string entityType, Guid? entityId, string? entityName, object? oldValues, object? newValues, string? ipAddress, string? userAgent)
    {
        var log = new AuditLog
        {
            OrganizationId = organizationId,
            UserId = userId,
            Action = action,
            EntityType = entityType,
            EntityId = entityId,
            EntityName = entityName,
            OldValues = oldValues != null ? JsonSerializer.Serialize(oldValues) : null,
            NewValues = newValues != null ? JsonSerializer.Serialize(newValues) : null,
            IpAddress = ipAddress,
            UserAgent = userAgent,
            CreatedAt = DateTime.UtcNow
        };

        _context.AuditLogs.Add(log);
        await _context.SaveChangesAsync();
    }
}
