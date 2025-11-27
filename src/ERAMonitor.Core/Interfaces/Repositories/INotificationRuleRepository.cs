using ERAMonitor.Core.DTOs.Common;
using ERAMonitor.Core.Entities;
using ERAMonitor.Core.Enums;

namespace ERAMonitor.Core.Interfaces.Repositories;

public interface INotificationRuleRepository
{
    Task<NotificationRule?> GetByIdAsync(Guid id, Guid organizationId);
    Task<List<NotificationRule>> GetAllAsync(Guid organizationId);
    Task<PagedResponse<NotificationRule>> GetPagedAsync(
        Guid organizationId, 
        PagedRequest request,
        NotificationEventType? eventType = null,
        bool? isEnabled = null);
    Task<List<NotificationRule>> GetMatchingRulesAsync(
        Guid organizationId,
        NotificationEventType eventType,
        Guid? customerId = null,
        Guid? hostId = null,
        Guid? checkId = null,
        List<string>? tags = null);
    Task<NotificationRule> CreateAsync(NotificationRule rule);
    Task UpdateAsync(NotificationRule rule);
    Task DeleteAsync(Guid id, Guid organizationId);
    Task<bool> ExistsAsync(Guid id, Guid organizationId);
    Task ReorderAsync(Guid organizationId, List<Guid> ruleIds);
}
