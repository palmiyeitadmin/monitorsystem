using ERAMonitor.Core.Enums;

namespace ERAMonitor.Core.DTOs.Services;

public class ServiceDetailDto
{
    public Guid Id { get; set; }
    public Guid HostId { get; set; }
    public string HostName { get; set; } = string.Empty;
    
    public ServiceType ServiceType { get; set; }
    public string ServiceName { get; set; } = string.Empty;
    public string? DisplayName { get; set; }
    public string? Description { get; set; }
    
    // Status
    public StatusType CurrentStatus { get; set; }
    public DateTime? LastStatusChange { get; set; }
    public StatusType? PreviousStatus { get; set; }
    
    // Config (type-specific)
    public Dictionary<string, object>? Config { get; set; }
    
    // Monitoring
    public bool MonitoringEnabled { get; set; }
    public bool AlertOnStop { get; set; }
    
    // Statistics
    public int RestartCount { get; set; }
    public DateTime? LastRestartAt { get; set; }
    public DateTime? LastHealthyAt { get; set; }
    
    // Recent Status History
    public List<ServiceStatusHistoryDto> RecentHistory { get; set; } = new();
    
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class ServiceStatusHistoryDto
{
    public StatusType Status { get; set; }
    public string? Message { get; set; }
    public DateTime RecordedAt { get; set; }
}
