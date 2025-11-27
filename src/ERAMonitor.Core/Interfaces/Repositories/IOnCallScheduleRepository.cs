using ERAMonitor.Core.Entities;

namespace ERAMonitor.Core.Interfaces.Repositories;

public interface IOnCallScheduleRepository
{
    Task<OnCallSchedule?> GetByIdAsync(Guid id, Guid organizationId);
    Task<List<OnCallSchedule>> GetAllAsync(Guid organizationId, bool activeOnly = true);
    Task<OnCallSchedule> CreateAsync(OnCallSchedule schedule);
    Task UpdateAsync(OnCallSchedule schedule);
    Task DeleteAsync(Guid id, Guid organizationId);
    Task<bool> ExistsAsync(Guid id, Guid organizationId);
    Task<User?> GetCurrentOnCallUserAsync(Guid scheduleId);
    Task<List<OnCallSchedule>> GetSchedulesNeedingRotationAsync();
    Task AddMemberAsync(OnCallScheduleMember member);
    Task RemoveMemberAsync(Guid memberId);
    Task<List<OnCallScheduleMember>> GetMembersAsync(Guid scheduleId);
    Task AddOverrideAsync(OnCallOverride onCallOverride);
    Task RemoveOverrideAsync(Guid overrideId);
    Task<List<OnCallOverride>> GetActiveOverridesAsync(Guid scheduleId);
}
