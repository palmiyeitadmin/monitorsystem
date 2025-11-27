using ERAMonitor.Core.Enums;

namespace ERAMonitor.Core.DTOs.Services;

public class ServiceListItemDto
{
    public Guid Id { get; set; }
    public Guid HostId { get; set; }
    public string HostName { get; set; } = string.Empty;
    public OsType HostOsType { get; set; }
    
    public ServiceType ServiceType { get; set; }
    public string ServiceTypeDisplay => GetServiceTypeDisplay();
    public string ServiceName { get; set; } = string.Empty;
    public string? DisplayName { get; set; }
    
    public StatusType CurrentStatus { get; set; }
    public DateTime? LastStatusChange { get; set; }
    public bool MonitoringEnabled { get; set; }
    
    public Guid? CustomerId { get; set; }
    public string? CustomerName { get; set; }
    
    private string GetServiceTypeDisplay()
    {
        return ServiceType switch
        {
            ServiceType.IIS_Site => "IIS Site",
            ServiceType.IIS_AppPool => "IIS App Pool",
            ServiceType.WindowsService => "Windows Service",
            ServiceType.SystemdUnit => "Systemd Unit",
            ServiceType.DockerContainer => "Docker Container",
            ServiceType.Process => "Process",
            _ => ServiceType.ToString()
        };
    }
}
