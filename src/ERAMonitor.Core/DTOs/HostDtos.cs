using System.ComponentModel.DataAnnotations;
using ERAMonitor.Core.Enums;

namespace ERAMonitor.Core.DTOs;

public class HostDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Hostname { get; set; }
    public string? Description { get; set; }
    public string OsType { get; set; } = string.Empty;
    public string? OsVersion { get; set; }
    public string Category { get; set; } = string.Empty;
    public string[] Tags { get; set; } = Array.Empty<string>();
    public string? AgentVersion { get; set; }
    public int CheckIntervalSeconds { get; set; }
    public string CurrentStatus { get; set; } = string.Empty;
    public DateTime? LastSeenAt { get; set; }
    public long? UptimeSeconds { get; set; }
    public decimal? CpuPercent { get; set; }
    public decimal? RamPercent { get; set; }
    public long? RamUsedMb { get; set; }
    public long? RamTotalMb { get; set; }
    public bool MonitoringEnabled { get; set; }
    public bool IsActive { get; set; }
    public Guid? LocationId { get; set; }
    public Guid? CustomerId { get; set; }
}

public class CreateHostRequest
{
    [Required]
    public string Name { get; set; } = string.Empty;
    
    public string? Hostname { get; set; }
    public string? Description { get; set; }
    
    [Required]
    public OsType OsType { get; set; }
    
    public HostCategory Category { get; set; } = HostCategory.VirtualMachine;
    public string[] Tags { get; set; } = Array.Empty<string>();
    public int CheckIntervalSeconds { get; set; } = 60;
    public bool MonitoringEnabled { get; set; } = true;
    
    public Guid? LocationId { get; set; }
    public Guid? CustomerId { get; set; }
}

public class UpdateHostRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Hostname { get; set; }
    public string? Description { get; set; }
    public HostCategory Category { get; set; }
    public string[] Tags { get; set; } = Array.Empty<string>();
    public int CheckIntervalSeconds { get; set; }
    public bool MonitoringEnabled { get; set; }
    public bool AlertOnDown { get; set; }
    public int AlertDelaySeconds { get; set; }
    public Guid? LocationId { get; set; }
    public Guid? CustomerId { get; set; }
}

public class HostApiKeyResponse
{
    public Guid HostId { get; set; }
    public string ApiKey { get; set; } = string.Empty;
}
