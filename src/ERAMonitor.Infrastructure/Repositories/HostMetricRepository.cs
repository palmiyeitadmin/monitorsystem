using Microsoft.EntityFrameworkCore;
using ERAMonitor.Core.DTOs.Hosts;
using ERAMonitor.Core.Entities;
using ERAMonitor.Core.Interfaces.Repositories;
using ERAMonitor.Infrastructure.Data;

namespace ERAMonitor.Infrastructure.Repositories;

public class HostMetricRepository : IHostMetricRepository
{
    private readonly ApplicationDbContext _context;

    public HostMetricRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<HostMetric> AddAsync(HostMetric metric)
    {
        await _context.HostMetrics.AddAsync(metric);
        return metric;
    }

    public async Task AddRangeAsync(IEnumerable<HostMetric> metrics)
    {
        await _context.HostMetrics.AddRangeAsync(metrics);
    }

    public async Task<List<HostMetric>> GetByHostAsync(Guid hostId, DateTime from, DateTime to)
    {
        return await _context.HostMetrics
            .Where(m => m.HostId == hostId && m.RecordedAt >= from && m.RecordedAt <= to)
            .OrderBy(m => m.RecordedAt)
            .ToListAsync();
    }

    public async Task<List<HostMetric>> GetLatestByHostAsync(Guid hostId, int count = 100)
    {
        return await _context.HostMetrics
            .Where(m => m.HostId == hostId)
            .OrderByDescending(m => m.RecordedAt)
            .Take(count)
            .OrderBy(m => m.RecordedAt)
            .ToListAsync();
    }

    public async Task<HostMetricsDto> GetMetricsHistoryAsync(Guid hostId, DateTime from, DateTime to, string interval = "5m")
    {
        // In a real production system with TimescaleDB or InfluxDB, we would use database aggregation
        // For standard SQL, we'll fetch and aggregate in memory or use basic grouping
        // This is a simplified implementation
        
        var metrics = await GetByHostAsync(hostId, from, to);
        
        // Downsample if too many points
        // ... implementation of downsampling logic would go here
        
        var dto = new HostMetricsDto
        {
            HostId = hostId,
            CpuHistory = metrics
                .Where(m => m.CpuPercent.HasValue)
                .Select(m => new MetricDataPoint 
                { 
                    Timestamp = m.RecordedAt, 
                    Value = m.CpuPercent!.Value 
                }).ToList(),
            RamHistory = metrics
                .Where(m => m.RamPercent.HasValue)
                .Select(m => new MetricDataPoint 
                { 
                    Timestamp = m.RecordedAt, 
                    Value = m.RamPercent!.Value 
                }).ToList(),
            NetworkHistory = metrics
                .Where(m => m.NetworkInBytes.HasValue || m.NetworkOutBytes.HasValue)
                .Select(m => new NetworkMetricDataPoint
                {
                    Timestamp = m.RecordedAt,
                    InBytes = m.NetworkInBytes ?? 0,
                    OutBytes = m.NetworkOutBytes ?? 0
                }).ToList()
        };
        
        return dto;
    }

    public async Task<List<MetricDataPoint>> GetCpuHistoryAsync(Guid hostId, DateTime from, DateTime to, string interval = "5m")
    {
        return await _context.HostMetrics
            .Where(m => m.HostId == hostId && m.RecordedAt >= from && m.RecordedAt <= to && m.CpuPercent.HasValue)
            .OrderBy(m => m.RecordedAt)
            .Select(m => new MetricDataPoint
            {
                Timestamp = m.RecordedAt,
                Value = m.CpuPercent!.Value
            })
            .ToListAsync();
    }

    public async Task<List<MetricDataPoint>> GetRamHistoryAsync(Guid hostId, DateTime from, DateTime to, string interval = "5m")
    {
        return await _context.HostMetrics
            .Where(m => m.HostId == hostId && m.RecordedAt >= from && m.RecordedAt <= to && m.RamPercent.HasValue)
            .OrderBy(m => m.RecordedAt)
            .Select(m => new MetricDataPoint
            {
                Timestamp = m.RecordedAt,
                Value = m.RamPercent!.Value
            })
            .ToListAsync();
    }

    public async Task<int> DeleteOlderThanAsync(int retentionDays)
    {
        var threshold = DateTime.UtcNow.AddDays(-retentionDays);
        
        // Batch delete for performance
        return await _context.HostMetrics
            .Where(m => m.RecordedAt < threshold)
            .ExecuteDeleteAsync();
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
