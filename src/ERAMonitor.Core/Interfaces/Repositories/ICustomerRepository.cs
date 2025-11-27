using ERAMonitor.Core.DTOs.Common;
using ERAMonitor.Core.DTOs.Customers;
using ERAMonitor.Core.Entities;

namespace ERAMonitor.Core.Interfaces.Repositories;

public interface ICustomerRepository : IRepository<Customer>
{
    Task<Customer?> GetBySlugAsync(Guid organizationId, string slug);
    Task<Customer?> GetByApiKeyAsync(string apiKey);
    Task<bool> SlugExistsAsync(Guid organizationId, string slug, Guid? excludeId = null);
    Task<PagedResponse<CustomerListItemDto>> GetPagedAsync(
        Guid organizationId, 
        PagedRequest request, 
        Guid? assignedAdminId = null, 
        bool? isActive = null);
    Task<List<Customer>> GetByAssignedAdminAsync(Guid adminId);
    Task<CustomerDetailDto?> GetDetailAsync(Guid id);
}
