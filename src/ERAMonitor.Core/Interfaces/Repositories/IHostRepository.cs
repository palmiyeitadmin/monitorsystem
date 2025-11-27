using ERAMonitor.Core.Entities;
using ERAMonitor.Core.DTOs.Common;
using ERAMonitor.Core.DTOs.Hosts;
using ERAMonitor.Core.Enums;

namespace ERAMonitor.Core.Interfaces.Repositories;

public interface IHostRepository : IRepository<Host>
{
    Task<Host?> GetByApiKeyAsync(string apiKey);
    Task<bool> ApiKeyExistsAsync(string apiKey, Guid? excludeId = null);
    
    Task<PagedResponse<HostListItemDto>> GetPagedAsync(
        Guid organizationId,
        PagedRequest request,
        StatusType? status = null,
        Guid? customerId = null,
        Guid? locationId = null,
        OsType? osType = null,
        string[]? tags = null,
        bool? monitoringEnabled = null,
        bool? isActive = null);
    
    Task<HostDetailDto?> GetDetailAsync(Guid id, Guid organizationId);
    
    Task<List<Host>> GetByCustomerAsync(Guid customerId);
    Task<List<Host>> GetByLocationAsync(Guid locationId);
    
    Task<List<Host>> GetHostsNotSeenSinceAsync(DateTime threshold);
    Task<List<Host>> GetHostsInMaintenanceEndingAsync(DateTime before);
    
    Task<int> GetCountByStatusAsync(Guid organizationId, StatusType status);
    Task<Dictionary<StatusType, int>> GetStatusCountsAsync(Guid organizationId);
    
    // For dashboard
    Task<List<HostListItemDto>> GetRecentlyDownAsync(Guid organizationId, int limit = 10);
    Task<List<HostListItemDto>> GetHighResourceUsageAsync(Guid organizationId, int cpuThreshold = 90, int ramThreshold = 90, int limit = 10);
}
