using ERAMonitor.Core.DTOs.Common;
using ERAMonitor.Core.DTOs.Hosts;
using ERAMonitor.Core.Enums;

namespace ERAMonitor.Core.Interfaces.Services;

public interface IHostService
{
    Task<PagedResponse<HostListItemDto>> GetPagedAsync(
        Guid organizationId,
        PagedRequest request,
        StatusType? status = null,
        Guid? customerId = null,
        Guid? locationId = null,
        OsType? osType = null,
        string[]? tags = null);
    
    Task<HostDetailDto> GetByIdAsync(Guid id, Guid organizationId);
    Task<HostDetailDto> CreateAsync(Guid organizationId, CreateHostRequest request);
    Task<HostDetailDto> UpdateAsync(Guid id, Guid organizationId, UpdateHostRequest request);
    Task DeleteAsync(Guid id, Guid organizationId);
    
    Task<string> RegenerateApiKeyAsync(Guid id, Guid organizationId);
    Task<HostDetailDto> SetMaintenanceAsync(Guid id, Guid organizationId, SetMaintenanceRequest request);
    Task<bool> ToggleMonitoringAsync(Guid id, Guid organizationId);
    
    // Metrics
    Task<HostMetricsDto> GetMetricsAsync(Guid id, Guid organizationId, DateTime from, DateTime to, string interval = "5m");
    
    // Statistics
    Task<Dictionary<StatusType, int>> GetStatusCountsAsync(Guid organizationId);
    Task<List<HostListItemDto>> GetRecentlyDownAsync(Guid organizationId, int limit = 10);
    Task<List<HostListItemDto>> GetHighResourceUsageAsync(Guid organizationId, int limit = 10);
}
