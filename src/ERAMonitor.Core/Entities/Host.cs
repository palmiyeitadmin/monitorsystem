using ERAMonitor.Core.Enums;

namespace ERAMonitor.Core.Entities;

public class Host : BaseEntityWithOrganization
{
    public Guid? LocationId { get; set; }
    public Guid? CustomerId { get; set; }
    
    // Basic Info
    public string Name { get; set; } = string.Empty;
    public string? Hostname { get; set; }
    public string? Description { get; set; }
    public OsType OsType { get; set; }
    public string? OsVersion { get; set; }
    public HostCategory Category { get; set; } = HostCategory.VirtualMachine;
    public string[] Tags { get; set; } = Array.Empty<string>();
    
    // Network Info
    public string? PrimaryIp { get; set; }
    public string? PublicIp { get; set; }
    
    // Agent Configuration
    public string ApiKey { get; set; } = GenerateApiKey();
    public string? AgentVersion { get; set; }
    public DateTime? AgentInstalledAt { get; set; }
    public int CheckIntervalSeconds { get; set; } = 60;
    
    // Current Status (updated by agent heartbeat)
    public StatusType CurrentStatus { get; set; } = StatusType.Unknown;
    public DateTime? LastSeenAt { get; set; }
    public string? LastHeartbeat { get; set; } // Full JSON for debugging
    public DateTime? StatusChangedAt { get; set; }
    public StatusType? PreviousStatus { get; set; }
    
    // Current Metrics (denormalized for quick dashboard access)
    public long? UptimeSeconds { get; set; }
    public decimal? CpuPercent { get; set; }
    public decimal? RamPercent { get; set; }
    public long? RamUsedMb { get; set; }
    public long? RamTotalMb { get; set; }
    public int? ProcessCount { get; set; }
    
    // Thresholds for alerts
    public int CpuWarningThreshold { get; set; } = 80;
    public int CpuCriticalThreshold { get; set; } = 95;
    public int RamWarningThreshold { get; set; } = 80;
    public int RamCriticalThreshold { get; set; } = 95;
    public int DiskWarningThreshold { get; set; } = 80;
    public int DiskCriticalThreshold { get; set; } = 95;
    
    // Monitoring Settings
    public bool MonitoringEnabled { get; set; } = true;
    public bool AlertOnDown { get; set; } = true;
    public int AlertDelaySeconds { get; set; } = 60; // Wait before alerting
    public bool AlertOnHighCpu { get; set; } = true;
    public bool AlertOnHighRam { get; set; } = true;
    public bool AlertOnHighDisk { get; set; } = true;
    
    // Maintenance Mode
    public bool MaintenanceMode { get; set; } = false;
    public DateTime? MaintenanceStartAt { get; set; }
    public DateTime? MaintenanceEndAt { get; set; }
    public string? MaintenanceReason { get; set; }
    
    // Notes
    public string? Notes { get; set; }
    
    // Soft Delete
    public bool IsActive { get; set; } = true;
    
    // Navigation Properties
    public virtual Location? Location { get; set; }
    public virtual Customer? Customer { get; set; }
    public virtual ICollection<HostDisk> Disks { get; set; } = new List<HostDisk>();
    public virtual ICollection<HostMetric> Metrics { get; set; } = new List<HostMetric>();
    public virtual ICollection<Service> Services { get; set; } = new List<Service>();
    public virtual ICollection<Check> Checks { get; set; } = new List<Check>();
    
    // Helper Methods
    public static string GenerateApiKey()
    {
        return Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString("N");
    }
    
    public bool IsInMaintenance()
    {
        if (!MaintenanceMode) return false;
        
        var now = DateTime.UtcNow;
        
        if (MaintenanceStartAt.HasValue && now < MaintenanceStartAt.Value)
            return false;
            
        if (MaintenanceEndAt.HasValue && now > MaintenanceEndAt.Value)
            return false;
            
        return true;
    }
    
    public bool ShouldAlert()
    {
        return MonitoringEnabled && !IsInMaintenance() && AlertOnDown;
    }
    
    public string GetStatusDisplay()
    {
        if (!MonitoringEnabled) return "Disabled";
        if (IsInMaintenance()) return "Maintenance";
        return CurrentStatus.ToString();
    }
}
