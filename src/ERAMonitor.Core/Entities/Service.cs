using ERAMonitor.Core.Enums;

namespace ERAMonitor.Core.Entities;

public class Service : BaseEntity
{
    public Guid HostId { get; set; }
    
    // Service Identification
    public ServiceType ServiceType { get; set; }
    public string ServiceName { get; set; } = string.Empty; // Internal name: w3svc, nginx.service
    public string? DisplayName { get; set; } // Friendly name
    public string? Description { get; set; }
    
    // Current Status
    public StatusType CurrentStatus { get; set; } = StatusType.Unknown;
    public DateTime? LastStatusChange { get; set; }
    public StatusType? PreviousStatus { get; set; }
    
    // Service-specific configuration (JSON)
    public string? Config { get; set; }
    // For IIS_Site: {bindings, physicalPath, appPoolName}
    // For IIS_AppPool: {managedRuntimeVersion, startMode, processModel}
    // For WindowsService: {startType, serviceAccount, executablePath}
    // For SystemdUnit: {unitFile, activeState, subState, mainPID}
    // For DockerContainer: {containerId, image, ports, networks}
    
    // Monitoring
    public bool MonitoringEnabled { get; set; } = true;
    public bool AlertOnStop { get; set; } = true;
    
    // Statistics
    public int RestartCount { get; set; } = 0;
    public DateTime? LastRestartAt { get; set; }
    public DateTime? LastHealthyAt { get; set; }
    
    // Navigation
    public virtual Host Host { get; set; } = null!;
    public virtual ICollection<ServiceStatusHistory> StatusHistory { get; set; } = new List<ServiceStatusHistory>();
}
