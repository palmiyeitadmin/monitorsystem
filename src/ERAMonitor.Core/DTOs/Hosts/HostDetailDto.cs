using ERAMonitor.Core.Enums;
using ERAMonitor.Core.DTOs.Services;
using System.Text.Json.Serialization;

namespace ERAMonitor.Core.DTOs.Hosts;

public class HostDetailDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Hostname { get; set; }
    public string? Description { get; set; }
    public OsType OsType { get; set; }
    public string? OsVersion { get; set; }
    public HostCategory Category { get; set; }
    public string[] Tags { get; set; } = Array.Empty<string>();
    
    // Network
    public string? PrimaryIp { get; set; }
    public string? PublicIp { get; set; }
    
    // Agent
    public string ApiKey { get; set; } = string.Empty;
    public string? AgentVersion { get; set; }
    public DateTime? AgentInstalledAt { get; set; }
    public int CheckIntervalSeconds { get; set; }
    
    // Status
    public StatusType CurrentStatus { get; set; }
    public string StatusDisplay { get; set; } = string.Empty;
    public DateTime? LastSeenAt { get; set; }
    public DateTime? StatusChangedAt { get; set; }
    
    // Current Metrics
    public HostCurrentMetricsDto Metrics { get; set; } = new();
    
    // Disks
    public List<HostDiskDto> Disks { get; set; } = new();

    // Services
    [JsonPropertyName("services")]
    public List<ServiceDto> Services { get; set; } = new();
    
    // Thresholds
    public HostThresholdsDto Thresholds { get; set; } = new();
    
    // Monitoring Settings
    public HostMonitoringSettingsDto MonitoringSettings { get; set; } = new();
    
    // Maintenance
    public HostMaintenanceDto Maintenance { get; set; } = new();
    
    // Related
    public CustomerSummaryDto? Customer { get; set; }
    public LocationSummaryDto? Location { get; set; }
    
    // Statistics
    public HostStatisticsDto Statistics { get; set; } = new();
    
    public string? Notes { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class HostCurrentMetricsDto
{
    public decimal? CpuPercent { get; set; }
    public decimal? RamPercent { get; set; }
    public long? RamUsedMb { get; set; }
    public long? RamTotalMb { get; set; }
    public long? UptimeSeconds { get; set; }
    public int? ProcessCount { get; set; }
    
    public string UptimeDisplay => FormatUptime(UptimeSeconds);
    
    private static string FormatUptime(long? seconds)
    {
        if (!seconds.HasValue) return "Unknown";
        
        var ts = TimeSpan.FromSeconds(seconds.Value);
        if (ts.TotalDays >= 1)
            return $"{(int)ts.TotalDays}d {ts.Hours}h {ts.Minutes}m";
        if (ts.TotalHours >= 1)
            return $"{(int)ts.TotalHours}h {ts.Minutes}m";
        return $"{ts.Minutes}m {ts.Seconds}s";
    }
}

public class HostDiskDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? MountPoint { get; set; }
    public string? FileSystem { get; set; }
    public decimal TotalGb { get; set; }
    public decimal UsedGb { get; set; }
    public decimal FreeGb { get; set; }
    public decimal UsedPercent { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class HostThresholdsDto
{
    public int CpuWarning { get; set; } = 80;
    public int CpuCritical { get; set; } = 95;
    public int RamWarning { get; set; } = 80;
    public int RamCritical { get; set; } = 95;
    public int DiskWarning { get; set; } = 80;
    public int DiskCritical { get; set; } = 95;
}

public class HostMonitoringSettingsDto
{
    public bool MonitoringEnabled { get; set; }
    public bool AlertOnDown { get; set; }
    public int AlertDelaySeconds { get; set; }
    public bool AlertOnHighCpu { get; set; }
    public bool AlertOnHighRam { get; set; }
    public bool AlertOnHighDisk { get; set; }
}

public class HostMaintenanceDto
{
    public bool InMaintenance { get; set; }
    public DateTime? StartAt { get; set; }
    public DateTime? EndAt { get; set; }
    public string? Reason { get; set; }
}

public class HostStatisticsDto
{
    public int ServiceCount { get; set; }
    public int ServicesUp { get; set; }
    public int ServicesDown { get; set; }
    public int CheckCount { get; set; }
    public int OpenIncidentCount { get; set; }
    public decimal Uptime7d { get; set; }
    public decimal Uptime30d { get; set; }
}
