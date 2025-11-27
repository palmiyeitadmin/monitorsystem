using ERAMonitor.Core.Enums;

namespace ERAMonitor.Core.DTOs.Services;

public class ServiceDto
{
    public Guid Id { get; set; }
    public Guid HostId { get; set; }
    public string HostName { get; set; } = string.Empty;
    public ServiceType ServiceType { get; set; }
    public string ServiceName { get; set; } = string.Empty;
    public string? DisplayName { get; set; }
    public StatusType CurrentStatus { get; set; }
    public DateTime? LastStatusChange { get; set; }
    public bool MonitoringEnabled { get; set; }
    public DateTime CreatedAt { get; set; }
}
