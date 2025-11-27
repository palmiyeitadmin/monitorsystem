using ERAMonitor.Core.DTOs;
using ERAMonitor.Core.Entities;
using ERAMonitor.Core.Enums;

namespace ERAMonitor.Core.Interfaces;

public interface IIncidentService
{
    Task<Incident> CreateAutoIncident(object resource, string title, IncidentSeverity severity);
    Task AcknowledgeIncident(Guid incidentId, Guid userId);
    Task ResolveIncident(Guid incidentId, Guid userId, ResolveIncidentRequest request);
    Task AutoResolveIncident(Check check);
    Task AddTimelineEntry(Guid incidentId, string eventType, string content, Guid? userId = null);
    
    // Phase 3 Methods
    Task<Incident> CreateAutoIncidentAsync(string resourceType, Guid resourceId, string resourceName, string title, string description, IncidentSeverity severity, Guid? customerId, Guid organizationId);
    Task AutoResolveIncidentsAsync(string resourceType, Guid resourceId, string resolutionNote);
}
