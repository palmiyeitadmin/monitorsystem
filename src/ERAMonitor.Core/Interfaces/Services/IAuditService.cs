using ERAMonitor.Core.Entities;

namespace ERAMonitor.Core.Interfaces.Services;

public interface IAuditService
{
    Task LogAsync(string action, string entityType, Guid? entityId, string? entityName, object? oldValues = null, object? newValues = null, Guid? userId = null, Guid? organizationId = null);
    Task LogCreateAsync<T>(T entity, Guid? userId = null) where T : BaseEntity;
    Task LogUpdateAsync<T>(T entity, object? oldValues, Guid? userId = null) where T : BaseEntity;
    Task LogDeleteAsync<T>(T entity, Guid? userId = null) where T : BaseEntity;
    Task LogLoginAsync(Guid userId, string ipAddress, bool success, string? failReason = null);
    Task LogLogoutAsync(Guid userId);
}
