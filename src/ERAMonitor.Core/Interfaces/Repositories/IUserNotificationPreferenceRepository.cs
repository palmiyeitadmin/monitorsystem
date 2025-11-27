using ERAMonitor.Core.Entities;

namespace ERAMonitor.Core.Interfaces.Repositories;

public interface IUserNotificationPreferenceRepository
{
    Task<UserNotificationPreference?> GetByUserIdAsync(Guid userId);
    Task<UserNotificationPreference> CreateAsync(UserNotificationPreference preference);
    Task UpdateAsync(UserNotificationPreference preference);
    Task DeleteAsync(Guid userId);
    Task<UserNotificationPreference> GetOrCreateAsync(Guid userId);
}
