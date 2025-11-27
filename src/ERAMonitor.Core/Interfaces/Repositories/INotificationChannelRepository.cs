using ERAMonitor.Core.DTOs.Common;
using ERAMonitor.Core.DTOs.Notifications;
using ERAMonitor.Core.Entities;
using ERAMonitor.Core.Enums;

namespace ERAMonitor.Core.Interfaces.Repositories;

public interface INotificationChannelRepository
{
    Task<NotificationChannel?> GetByIdAsync(Guid id, Guid organizationId);
    Task<List<NotificationChannel>> GetAllAsync(Guid organizationId);
    Task<PagedResponse<NotificationChannel>> GetPagedAsync(Guid organizationId, PagedRequest request, NotificationChannelType? type = null);
    Task<NotificationChannel?> GetDefaultChannelAsync(Guid organizationId, NotificationChannelType? type = null);
    Task<NotificationChannel> CreateAsync(NotificationChannel channel);
    Task UpdateAsync(NotificationChannel channel);
    Task DeleteAsync(Guid id, Guid organizationId);
    Task<bool> ExistsAsync(Guid id, Guid organizationId);
    Task<int> GetNotificationsSent24hAsync(Guid channelId);
    Task<int> GetRuleCountAsync(Guid channelId);
}
