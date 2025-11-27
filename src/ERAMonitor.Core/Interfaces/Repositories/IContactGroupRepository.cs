using ERAMonitor.Core.Entities;

namespace ERAMonitor.Core.Interfaces.Repositories;

public interface IContactGroupRepository
{
    Task<ContactGroup?> GetByIdAsync(Guid id, Guid organizationId);
    Task<List<ContactGroup>> GetAllAsync(Guid organizationId, bool activeOnly = true);
    Task<ContactGroup> CreateAsync(ContactGroup group);
    Task UpdateAsync(ContactGroup group);
    Task DeleteAsync(Guid id, Guid organizationId);
    Task<bool> ExistsAsync(Guid id, Guid organizationId);
    Task AddMemberAsync(ContactGroupMember member);
    Task RemoveMemberAsync(Guid memberId);
    Task<List<ContactGroupMember>> GetMembersAsync(Guid groupId);
}
