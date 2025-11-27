using System.ComponentModel.DataAnnotations;
using ERAMonitor.Core.Enums;

namespace ERAMonitor.Core.DTOs;

public class IncidentDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public string SourceType { get; set; } = string.Empty;
    public Guid? SourceId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? AcknowledgedAt { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public Guid? AssignedToId { get; set; }
    public string? AssignedToName { get; set; }
    public bool? ResponseSlaMet { get; set; }
    public bool? ResolutionSlaMet { get; set; }
    public Guid? CustomerId { get; set; }
}

public class CreateIncidentRequest
{
    [Required]
    public string Title { get; set; } = string.Empty;
    
    [Required]
    public string Description { get; set; } = string.Empty;
    
    [Required]
    public IncidentSeverity Severity { get; set; }
    
    public IncidentPriority Priority { get; set; }
    public Guid? CustomerId { get; set; }
    public Guid? AssignedToId { get; set; }
}

public class UpdateIncidentRequest
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public IncidentSeverity? Severity { get; set; }
    public IncidentPriority? Priority { get; set; }
    public Guid? AssignedToId { get; set; }
}

public class ResolveIncidentRequest
{
    [Required]
    public string ResolutionSteps { get; set; } = string.Empty;
    
    public string? RootCauseCategory { get; set; }
    public string? RootCauseDescription { get; set; }
    public string? PreventiveActions { get; set; }
}

public class IncidentTimelineDto
{
    public Guid Id { get; set; }
    public string EventType { get; set; } = string.Empty;
    public string? Content { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? CreatedByName { get; set; }
}

