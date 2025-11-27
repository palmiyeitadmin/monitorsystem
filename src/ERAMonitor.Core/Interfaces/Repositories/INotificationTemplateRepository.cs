using ERAMonitor.Core.DTOs.Common;
using ERAMonitor.Core.Entities;
using ERAMonitor.Core.Enums;

namespace ERAMonitor.Core.Interfaces.Repositories;

public interface INotificationTemplateRepository
{
    Task<NotificationTemplate?> GetByIdAsync(Guid id, Guid organizationId);
    Task<List<NotificationTemplate>> GetAllAsync(Guid organizationId);
    Task<PagedResponse<NotificationTemplate>> GetPagedAsync(
        Guid organizationId, 
        PagedRequest request, 
        NotificationEventType? eventType = null,
        NotificationChannelType? channelType = null);
    Task<NotificationTemplate?> GetDefaultTemplateAsync(
        Guid organizationId,
        NotificationEventType eventType,
        NotificationChannelType channelType);
    Task<NotificationTemplate> CreateAsync(NotificationTemplate template);
    Task UpdateAsync(NotificationTemplate template);
    Task DeleteAsync(Guid id, Guid organizationId);
    Task<bool> ExistsAsync(Guid id, Guid organizationId);
}
