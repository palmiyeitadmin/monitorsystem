using ERAMonitor.Core.DTOs.Common;
using ERAMonitor.Core.DTOs.Notifications;
using ERAMonitor.Core.Entities;
using ERAMonitor.Core.Enums;

namespace ERAMonitor.Core.Interfaces.Repositories;

public interface INotificationLogRepository
{
    Task<NotificationLog?> GetByIdAsync(Guid id);
    Task<PagedResponse<NotificationLog>> GetPagedAsync(
        Guid organizationId,
        PagedRequest request,
        NotificationEventType? eventType = null,
        NotificationStatus? status = null,
        Guid? channelId = null,
        DateTime? dateFrom = null,
        DateTime? dateTo = null);
    Task<List<NotificationLog>> GetPendingAsync(int limit = 100);
    Task<List<NotificationLog>> GetFailedForRetryAsync(int limit = 100);
    Task<NotificationLog> CreateAsync(NotificationLog log);
    Task UpdateAsync(NotificationLog log);
    Task<NotificationStatsDto> GetStatsAsync(Guid organizationId, DateTime? from = null, DateTime? to = null);
    Task<int> CountByStatusAsync(Guid organizationId, NotificationStatus status, DateTime? from = null);
    Task DeleteOldLogsAsync(DateTime olderThan);
}
