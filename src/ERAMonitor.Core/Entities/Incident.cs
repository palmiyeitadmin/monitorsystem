using ERAMonitor.Core.Enums;

namespace ERAMonitor.Core.Entities;

public class Incident : BaseEntity
{
    public Guid OrganizationId { get; set; }
    public Organization Organization { get; set; } = null!;
    
    public Guid? CustomerId { get; set; }
    public Customer? Customer { get; set; }
    
    public int IncidentNumber { get; set; } // Serial
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    
    public IncidentStatus Status { get; set; } = IncidentStatus.New;
    public IncidentSeverity Severity { get; set; } = IncidentSeverity.Medium;
    public IncidentPriority Priority { get; set; } = IncidentPriority.Medium;
    public string? Impact { get; set; }
    
    public string? SourceType { get; set; }
    public Guid? SourceId { get; set; }
    
    public Guid? AssignedToId { get; set; }
    public User? AssignedTo { get; set; }
    
    public Guid? AcknowledgedById { get; set; }
    public User? AcknowledgedBy { get; set; }
    
    public Guid? ResolvedById { get; set; }
    public User? ResolvedBy { get; set; }
    
    public DateTime? AcknowledgedAt { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public DateTime? ClosedAt { get; set; }
    
    public int ResponseSlaMinutes { get; set; } = 15;
    public int ResolutionSlaMinutes { get; set; } = 240;
    public bool? ResponseSlaMet { get; set; }
    public bool? ResolutionSlaMet { get; set; }
    
    public string? RootCauseCategory { get; set; }
    public string? RootCauseDescription { get; set; }
    public string? ResolutionSteps { get; set; }
    public string? PreventiveActions { get; set; }
    
    public string? AffectedResources { get; set; } // JSONB
}
