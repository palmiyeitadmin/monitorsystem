using ERAMonitor.Core.DTOs.Common;
using ERAMonitor.Core.DTOs.EventLog;
using ERAMonitor.Core.Entities;

namespace ERAMonitor.Core.Interfaces.Repositories;

public interface IEventLogRepository : IRepository<EventLog>
{
    Task<PagedResponse<EventLogListItemDto>> GetPagedByHostAsync(
        Guid hostId,
        PagedRequest request,
        string? category = null,
        string? level = null,
        DateTime? fromDate = null,
        DateTime? toDate = null);
    
    Task<List<string>> GetCategoriesByHostAsync(Guid hostId);
    
    Task<int> DeleteOlderThanAsync(int retentionDays);
}
