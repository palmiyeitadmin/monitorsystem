using ERAMonitor.Core.Enums;

namespace ERAMonitor.Core.Entities;

public class StatusPageComponent : BaseEntity
{
    public Guid StatusPageId { get; set; }
    
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    
    // What this component represents
    public StatusPageComponentType Type { get; set; }
    public Guid? HostId { get; set; }
    public Guid? CheckId { get; set; }
    public Guid? ServiceId { get; set; }
    
    // Grouping
    public Guid? GroupId { get; set; }
    public int SortOrder { get; set; } = 0;
    
    // Display
    public bool ShowUptime { get; set; } = true;
    public bool ShowResponseTime { get; set; } = false;
    
    // Status
    public StatusPageComponentStatus Status { get; set; } = StatusPageComponentStatus.Operational;
    
    // Override status (for manual control)
    public bool OverrideStatus { get; set; } = false;
    public StatusPageComponentStatus? ManualStatus { get; set; }
    public string? ManualStatusMessage { get; set; }
    
    public int Order { get; set; } = 0;
    
    // Navigation
    public virtual StatusPage StatusPage { get; set; } = null!;
    public virtual Host? Host { get; set; }
    public virtual Check? Check { get; set; }
    public virtual Service? Service { get; set; }
    public virtual StatusPageComponentGroup? Group { get; set; }
}
