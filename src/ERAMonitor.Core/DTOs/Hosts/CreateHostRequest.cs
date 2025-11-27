using System.ComponentModel.DataAnnotations;
using ERAMonitor.Core.Enums;

namespace ERAMonitor.Core.DTOs.Hosts;

public class CreateHostRequest
{
    [Required(ErrorMessage = "Name is required")]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(255)]
    public string? Hostname { get; set; }
    
    public string? Description { get; set; }
    
    [Required(ErrorMessage = "OS type is required")]
    public OsType OsType { get; set; }
    
    [MaxLength(100)]
    public string? OsVersion { get; set; }
    
    public HostCategory Category { get; set; } = HostCategory.VirtualMachine;
    
    public string[]? Tags { get; set; }
    
    // Network
    [MaxLength(50)]
    public string? PrimaryIp { get; set; }
    
    [MaxLength(50)]
    public string? PublicIp { get; set; }
    
    // Assignment
    public Guid? CustomerId { get; set; }
    public Guid? LocationId { get; set; }
    
    // Monitoring Settings
    [Range(10, 3600)]
    public int CheckIntervalSeconds { get; set; } = 60;
    
    public bool MonitoringEnabled { get; set; } = true;
    public bool AlertOnDown { get; set; } = true;
    
    [Range(0, 3600)]
    public int AlertDelaySeconds { get; set; } = 60;
    
    public bool AlertOnHighCpu { get; set; } = true;
    public bool AlertOnHighRam { get; set; } = true;
    public bool AlertOnHighDisk { get; set; } = true;
    
    // Thresholds
    [Range(1, 100)]
    public int CpuWarningThreshold { get; set; } = 80;
    
    [Range(1, 100)]
    public int CpuCriticalThreshold { get; set; } = 95;
    
    [Range(1, 100)]
    public int RamWarningThreshold { get; set; } = 80;
    
    [Range(1, 100)]
    public int RamCriticalThreshold { get; set; } = 95;
    
    [Range(1, 100)]
    public int DiskWarningThreshold { get; set; } = 80;
    
    [Range(1, 100)]
    public int DiskCriticalThreshold { get; set; } = 95;
    
    public string? Notes { get; set; }
}
