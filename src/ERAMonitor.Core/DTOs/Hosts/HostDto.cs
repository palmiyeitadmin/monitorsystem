using ERAMonitor.Core.Enums;

namespace ERAMonitor.Core.DTOs.Hosts;

public class HostDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Hostname { get; set; }
    public OsType OsType { get; set; }
    public string? OsVersion { get; set; }
    public HostCategory Category { get; set; }
    public StatusType CurrentStatus { get; set; }
    public DateTime? LastSeenAt { get; set; }
    public decimal? CpuPercent { get; set; }
    public decimal? RamPercent { get; set; }
    public bool MonitoringEnabled { get; set; }
    public bool MaintenanceMode { get; set; }
    
    // Related
    public Guid? CustomerId { get; set; }
    public string? CustomerName { get; set; }
    public Guid? LocationId { get; set; }
    public string? LocationName { get; set; }
    
    // Counts
    public int ServiceCount { get; set; }
    public int ServicesHealthy { get; set; }
    public int ServicesUnhealthy { get; set; }
    
    public DateTime CreatedAt { get; set; }
}
