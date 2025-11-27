using ERAMonitor.Core.DTOs.Common;
using ERAMonitor.Core.DTOs.Services;
using ERAMonitor.Core.Enums;

namespace ERAMonitor.Core.Interfaces.Services;

public interface IServiceMonitorService
{
    Task<PagedResponse<ServiceListItemDto>> GetPagedAsync(
        Guid organizationId,
        PagedRequest request,
        Guid? hostId = null,
        Guid? customerId = null,
        ServiceType? serviceType = null,
        StatusType? status = null);
    
    Task<ServiceDetailDto> GetByIdAsync(Guid id);
    Task<ServiceDetailDto> UpdateAsync(Guid id, UpdateServiceRequest request);
    Task<bool> ToggleMonitoringAsync(Guid id);
    
    Task<List<ServiceStatusHistoryDto>> GetStatusHistoryAsync(Guid id, DateTime from, DateTime to);
    
    Task<Dictionary<StatusType, int>> GetStatusCountsAsync(Guid organizationId);
    Task<Dictionary<ServiceType, int>> GetCountsByTypeAsync(Guid organizationId);
}
