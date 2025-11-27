using ERAMonitor.Core.Enums;

namespace ERAMonitor.Core.DTOs;

public class ServiceDto
{
    public Guid Id { get; set; }
    public Guid HostId { get; set; }
    public string ServiceType { get; set; } = string.Empty;
    public string ServiceName { get; set; } = string.Empty;
    public string? DisplayName { get; set; }
    public string? Description { get; set; }
    public string CurrentStatus { get; set; } = string.Empty;
    public DateTime? LastStatusChange { get; set; }
    public bool MonitoringEnabled { get; set; }
    public int RestartCount { get; set; }
}

public class UpdateServiceRequest
{
    public bool MonitoringEnabled { get; set; }
    public string? DisplayName { get; set; }
    public string? Description { get; set; }
}
