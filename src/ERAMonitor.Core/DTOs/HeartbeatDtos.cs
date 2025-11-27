using System.Text.Json.Serialization;
using ERAMonitor.Core.Enums;

namespace ERAMonitor.Core.DTOs;

public class HeartbeatRequest
{
    public DateTime Timestamp { get; set; }
    public SystemInfo System { get; set; } = new();
    public List<DiskInfo> Disks { get; set; } = new();
    public List<ServiceInfo> Services { get; set; } = new();
    public NetworkInfo Network { get; set; } = new();
    public string AgentVersion { get; set; } = string.Empty;
}

public class SystemInfo
{
    public string Hostname { get; set; } = string.Empty;
    
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public OsType OsType { get; set; }
    
    public string OsVersion { get; set; } = string.Empty;
    public decimal CpuPercent { get; set; }
    public decimal RamPercent { get; set; }
    public long RamUsedMB { get; set; }
    public long RamTotalMB { get; set; }
    public long UptimeSeconds { get; set; }
}

public class DiskInfo
{
    public string Name { get; set; } = string.Empty;
    public decimal TotalGB { get; set; }
    public decimal UsedGB { get; set; }
    public decimal UsedPercent { get; set; }
    public string? MountPoint { get; set; }
}

public class ServiceInfo
{
    public string Name { get; set; } = string.Empty;
    public string? DisplayName { get; set; }
    
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public ServiceType Type { get; set; }
    
    public string Status { get; set; } = string.Empty;
    public object? Config { get; set; }
}

public class NetworkInfo
{
    public string? PrimaryIP { get; set; }
    public string? PublicIP { get; set; }
}

public class HeartbeatResponse
{
    public bool Success { get; set; }
    public Guid HostId { get; set; }
    public int NextCheckIn { get; set; }
    public List<string> Commands { get; set; } = new();
}
