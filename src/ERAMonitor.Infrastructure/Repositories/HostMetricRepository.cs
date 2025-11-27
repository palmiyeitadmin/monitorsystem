using Microsoft.EntityFrameworkCore;
using ERAMonitor.Core.DTOs.Agent;
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
        
        // Calculate Max Disk Usage per point
        var diskHistory = new List<MetricDataPoint>();
        foreach (var m in metrics)
        {
            if (string.IsNullOrEmpty(m.DiskInfo)) continue;
            
            try 
            {
                var disks = System.Text.Json.JsonSerializer.Deserialize<List<DiskInfoDto>>(m.DiskInfo);
                if (disks != null && disks.Any())
                {
                    var maxUsage = disks.Max(d => d.UsedPercent);
                    diskHistory.Add(new MetricDataPoint 
                    { 
                        Timestamp = m.RecordedAt, 
                        Value = maxUsage 
                    });
                }
            }
            catch { /* ignore parsing errors */ }
        }

        // Calculate Network Traffic Rate (MB/s)
        var networkHistory = new List<MetricDataPoint>();
        HostMetric? previous = null;
        
        foreach (var current in metrics.Where(m => m.NetworkInBytes.HasValue || m.NetworkOutBytes.HasValue))
        {
            if (previous != null)
            {
                var timeDiff = (current.RecordedAt - previous.RecordedAt).TotalSeconds;
                if (timeDiff > 0)
                {
                    var bytesDiff = (current.NetworkInBytes ?? 0) + (current.NetworkOutBytes ?? 0) - 
                                    ((previous.NetworkInBytes ?? 0) + (previous.NetworkOutBytes ?? 0));
                    
                    // Handle counter reset or negative diff
                    if (bytesDiff >= 0)
                    {
                        var mbs = (decimal)(bytesDiff / timeDiff / 1024.0 / 1024.0);
                        networkHistory.Add(new MetricDataPoint
                        {
                            Timestamp = current.RecordedAt,
                            Value = Math.Round(mbs, 2)
                        });
                    }
                }
            }
            previous = current;
        }

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
            DiskHistory = diskHistory,
            NetworkHistory = networkHistory
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
