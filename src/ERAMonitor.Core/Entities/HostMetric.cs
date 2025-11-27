namespace ERAMonitor.Core.Entities;

public class HostMetric
{
    public long Id { get; set; } // BIGSERIAL for time series
    public Guid HostId { get; set; }
    
    // CPU
    public decimal? CpuPercent { get; set; }
    
    // Memory
    public decimal? RamPercent { get; set; }
    public long? RamUsedMb { get; set; }
    public long? RamTotalMb { get; set; }
    
    // Disk (JSON array for multiple disks)
    public string? DiskInfo { get; set; } // [{name, totalGb, usedGb, usedPercent}]
    
    // Network
    public long? NetworkInBytes { get; set; }
    public long? NetworkOutBytes { get; set; }
    
    // System
    public long? UptimeSeconds { get; set; }
    public int? ProcessCount { get; set; }
    
    // Timestamp
    public DateTime RecordedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation
    public virtual Host Host { get; set; } = null!;
}
