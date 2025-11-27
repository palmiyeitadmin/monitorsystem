using ERAMonitor.Core.Entities;
using ERAMonitor.Core.DTOs.Hosts;

namespace ERAMonitor.Core.Interfaces.Repositories;

public interface IHostMetricRepository
{
    Task<HostMetric> AddAsync(HostMetric metric);
    Task AddRangeAsync(IEnumerable<HostMetric> metrics);
    
    Task<List<HostMetric>> GetByHostAsync(Guid hostId, DateTime from, DateTime to);
    Task<List<HostMetric>> GetLatestByHostAsync(Guid hostId, int count = 100);
    
    Task<HostMetricsDto> GetMetricsHistoryAsync(Guid hostId, DateTime from, DateTime to, string interval = "5m");
    
    // Aggregated data for charts
    Task<List<MetricDataPoint>> GetCpuHistoryAsync(Guid hostId, DateTime from, DateTime to, string interval = "5m");
    Task<List<MetricDataPoint>> GetRamHistoryAsync(Guid hostId, DateTime from, DateTime to, string interval = "5m");
    
    // Cleanup old metrics
    Task<int> DeleteOlderThanAsync(int retentionDays);
    
    Task SaveChangesAsync();
}
