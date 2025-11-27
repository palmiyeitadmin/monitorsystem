using ERAMonitor.Core.Enums;

namespace ERAMonitor.Core.DTOs.Hosts;

public class HostListItemDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Hostname { get; set; }
    public OsType OsType { get; set; }
    public HostCategory Category { get; set; }
    public string[] Tags { get; set; } = Array.Empty<string>();
    
    // Status
    public StatusType CurrentStatus { get; set; }
    public string StatusDisplay { get; set; } = string.Empty;
    public DateTime? LastSeenAt { get; set; }
    public bool MonitoringEnabled { get; set; }
    public bool MaintenanceMode { get; set; }
    
    // Metrics
    public decimal? CpuPercent { get; set; }
    public decimal? RamPercent { get; set; }
    public decimal? DiskMaxPercent { get; set; } // Highest disk usage
    public long? UptimeSeconds { get; set; }
    
    // Related
    public CustomerSummaryDto? Customer { get; set; }
    public LocationSummaryDto? Location { get; set; }
    
    // Counts
    public int ServiceCount { get; set; }
    public int ServicesUp { get; set; }
    public int ServicesDown { get; set; }
    public int OpenIncidentCount { get; set; }
    
    public DateTime CreatedAt { get; set; }
}

public class CustomerSummaryDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? LogoUrl { get; set; }
}

public class LocationSummaryDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? City { get; set; }
    public string? Country { get; set; }
}
