using ERAMonitor.Core.Entities;
using ERAMonitor.Core.DTOs.Common;
using ERAMonitor.Core.DTOs.Services;
using ERAMonitor.Core.Enums;

namespace ERAMonitor.Core.Interfaces.Repositories;

public interface IServiceRepository : IRepository<Service>
{
    Task<Service?> GetByHostAndNameAsync(Guid hostId, ServiceType type, string serviceName);
    
    Task<PagedResponse<ServiceListItemDto>> GetPagedAsync(
        Guid organizationId,
        PagedRequest request,
        Guid? hostId = null,
        Guid? customerId = null,
        ServiceType? serviceType = null,
        StatusType? status = null,
        bool? monitoringEnabled = null);
    
    Task<ServiceDetailDto?> GetDetailAsync(Guid id);
    
    Task<List<Service>> GetByHostAsync(Guid hostId);
    Task<List<Service>> GetByHostAsync(Guid hostId, ServiceType type);
    
    Task<int> GetCountByStatusAsync(Guid organizationId, StatusType status);
    Task<Dictionary<StatusType, int>> GetStatusCountsAsync(Guid organizationId);
    Task<Dictionary<ServiceType, int>> GetCountsByTypeAsync(Guid organizationId);
    
    Task UpsertServicesAsync(Guid hostId, List<Service> services);
}
