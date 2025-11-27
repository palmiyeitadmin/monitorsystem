using ERAMonitor.Core.DTOs.Common;
using ERAMonitor.Core.DTOs.Users;
using ERAMonitor.Core.Entities;

namespace ERAMonitor.Core.Interfaces.Repositories;

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByEmailAsync(string email, bool includeInactive);
    Task<User?> GetByPasswordResetTokenAsync(string token);
    Task<User?> GetByEmailVerificationTokenAsync(string token);
    Task<bool> EmailExistsAsync(string email);
    Task<PagedResponse<UserListItemDto>> GetPagedAsync(Guid organizationId, PagedRequest request, string? role = null, bool? isActive = null);
    Task<List<User>> GetByOrganizationAsync(Guid organizationId);
    Task<List<User>> GetByRoleAsync(Guid organizationId, string role);
    Task<List<User>> GetCustomerUsersAsync(Guid customerId);
}
