using ERAMonitor.Core.Entities;

namespace ERAMonitor.Core.Interfaces;

public interface IAuditLogService
{
    Task LogAsync(Guid organizationId, Guid? userId, string action, string entityType, string entityId, string? entityName, object? oldValues, object? newValues, string? ipAddress, string? userAgent);
    Task LogAsync(Guid organizationId, Guid? userId, string action, string entityType, Guid? entityId, string? entityName, object? oldValues, object? newValues, string? ipAddress, string? userAgent);
}
