namespace ERAMonitor.Core.DTOs.Agent;

public class HeartbeatRequest
{
    public DateTime Timestamp { get; set; }
    public SystemInfoDto System { get; set; } = new();
    public List<DiskInfoDto> Disks { get; set; } = new();
    public List<ServiceInfoDto> Services { get; set; } = new();
    public NetworkInfoDto? Network { get; set; }
    public List<EventLogInfoDto>? EventLogs { get; set; }
    public string AgentVersion { get; set; } = string.Empty;
}

public class SystemInfoDto
{
    public string Hostname { get; set; } = string.Empty;
    public string OsType { get; set; } = string.Empty; // Windows, Linux
    public string? OsVersion { get; set; }
    public decimal CpuPercent { get; set; }
    public decimal RamPercent { get; set; }
    public long RamUsedMb { get; set; }
    public long RamTotalMb { get; set; }
    public long UptimeSeconds { get; set; }
    public int ProcessCount { get; set; }
}

public class DiskInfoDto
{
    public string Name { get; set; } = string.Empty; // C:, /dev/sda1
    public string? MountPoint { get; set; } // C:\, /
    public string? FileSystem { get; set; } // NTFS, ext4
    public decimal TotalGb { get; set; }
    public decimal UsedGb { get; set; }
    public decimal UsedPercent { get; set; }
}

public class ServiceInfoDto
{
    public string Name { get; set; } = string.Empty; // w3svc, nginx.service
    public string? DisplayName { get; set; }
    public string Type { get; set; } = string.Empty; // IIS_Site, IIS_AppPool, WindowsService, SystemdUnit, DockerContainer
    public string Status { get; set; } = string.Empty; // Running, Stopped, Starting, etc.
    public Dictionary<string, object>? Config { get; set; }
}

public class NetworkInfoDto
{
    public string? PrimaryIp { get; set; }
    public string? PublicIp { get; set; }
    public long? InBytes { get; set; }
    public long? OutBytes { get; set; }
}
