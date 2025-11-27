using System.Text.Json;
using ERAMonitor.Core.DTOs;
using ERAMonitor.Core.Entities;
using ERAMonitor.Core.Enums;
using ERAMonitor.Core.Interfaces;
using ERAMonitor.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ERAMonitor.Infrastructure.Services;

public class IncidentService : IIncidentService
{
    private readonly ApplicationDbContext _context;
    private readonly INotificationService _notificationService;

    public IncidentService(ApplicationDbContext context, INotificationService notificationService)
    {
        _context = context;
        _notificationService = notificationService;
    }

    public async Task<Incident> CreateAutoIncident(object resource, string title, IncidentSeverity severity)
    {
        // Identify resource type and ID
        string sourceType = resource.GetType().Name;
        Guid sourceId = Guid.Empty;
        Guid orgId = Guid.Empty;
        Guid? customerId = null;
        string resourceName = string.Empty;
        string currentStatus = string.Empty;

        if (resource is Host host)
        {
            sourceId = host.Id;
            orgId = host.OrganizationId;
            customerId = host.CustomerId;
            resourceName = host.Name;
            currentStatus = host.CurrentStatus.ToString();
        }
        else if (resource is Check check)
        {
            sourceId = check.Id;
            orgId = check.OrganizationId;
            customerId = check.CustomerId;
            resourceName = check.Name;
            currentStatus = check.CurrentStatus.ToString();
        }
        else
        {
            throw new ArgumentException("Unsupported resource type");
        }

        // Check for existing open incident
        var existingIncident = await _context.Incidents
            .FirstOrDefaultAsync(i => i.SourceType == sourceType && i.SourceId == sourceId && i.Status != IncidentStatus.Resolved && i.Status != IncidentStatus.Closed);

        if (existingIncident != null)
        {
            await AddTimelineEntry(existingIncident.Id, "ResourceStatusUpdate", $"Status changed to {currentStatus}");
            return existingIncident;
        }

        var incident = new Incident
        {
            OrganizationId = orgId,
            CustomerId = customerId,
            Title = title,
            Description = $"Automatically created due to {sourceType} status change",
            Status = IncidentStatus.New,
            Severity = severity,
            Priority = MapSeverityToPriority(severity),
            SourceType = sourceType,
            SourceId = sourceId,
            AffectedResources = JsonSerializer.Serialize(new[] {
                new { 
                    Type = sourceType, 
                    Id = sourceId, 
                    Name = resourceName,
                    Status = currentStatus
                }
            })
        };

        _context.Incidents.Add(incident);
        await _context.SaveChangesAsync();

        await AddTimelineEntry(incident.Id, "Created", $"Incident automatically created for {resourceName}");
        
        // Notify
        await _notificationService.SendIncidentCreatedNotification(incident);

        return incident;
    }

    public async Task AcknowledgeIncident(Guid incidentId, Guid userId)
    {
        var incident = await _context.Incidents.FindAsync(incidentId);
        if (incident == null) return;

        var user = await _context.Users.FindAsync(userId);

        incident.Status = IncidentStatus.Acknowledged;
        incident.AcknowledgedById = userId;
        incident.AcknowledgedAt = DateTime.UtcNow;

        // Check SLA
        if (incident.ResponseSlaMinutes > 0 && incident.AcknowledgedAt.HasValue)
        {
            var responseTime = (incident.AcknowledgedAt.Value - incident.CreatedAt).TotalMinutes;
            incident.ResponseSlaMet = responseTime <= incident.ResponseSlaMinutes;
        }

        await AddTimelineEntry(incidentId, "Acknowledged", $"Acknowledged by {user?.FullName}", userId);
        await _context.SaveChangesAsync();
    }

    public async Task ResolveIncident(Guid incidentId, Guid userId, ResolveIncidentRequest request)
    {
        var incident = await _context.Incidents.FindAsync(incidentId);
        if (incident == null) return;

        var user = await _context.Users.FindAsync(userId);

        incident.Status = IncidentStatus.Resolved;
        incident.ResolvedById = userId;
        incident.ResolvedAt = DateTime.UtcNow;
        incident.RootCauseCategory = request.RootCauseCategory;
        incident.RootCauseDescription = request.RootCauseDescription;
        incident.ResolutionSteps = request.ResolutionSteps;
        incident.PreventiveActions = request.PreventiveActions;

        // Check SLA
        if (incident.ResolutionSlaMinutes > 0 && incident.ResolvedAt.HasValue)
        {
            var resolutionTime = (incident.ResolvedAt.Value - incident.CreatedAt).TotalMinutes;
            incident.ResolutionSlaMet = resolutionTime <= incident.ResolutionSlaMinutes;
        }

        await AddTimelineEntry(incidentId, "Resolved", $"Resolved by {user?.FullName}: {request.ResolutionSteps}", userId);
        await _context.SaveChangesAsync();
        
        // Notify
        await _notificationService.SendIncidentResolvedNotification(incident);
    }

    public async Task AutoResolveIncident(Check check)
    {
        var incident = await _context.Incidents
            .FirstOrDefaultAsync(i => i.SourceType == nameof(Check) && i.SourceId == check.Id && i.Status != IncidentStatus.Resolved && i.Status != IncidentStatus.Closed);

        if (incident != null)
        {
            incident.Status = IncidentStatus.Resolved;
            incident.ResolvedAt = DateTime.UtcNow;
            incident.ResolutionSteps = "Automatically resolved as check is back UP";
            
            await AddTimelineEntry(incident.Id, "AutoResolved", "Automatically resolved as check is back UP");
            await _context.SaveChangesAsync();
            
            // Notify
            await _notificationService.SendIncidentResolvedNotification(incident);
        }
    }

    // Phase 3 Methods
    public async Task<Incident> CreateAutoIncidentAsync(string resourceType, Guid resourceId, string resourceName, string title, string description, IncidentSeverity severity, Guid? customerId, Guid organizationId)
    {
        // Check for existing open incident
        var existingIncident = await _context.Incidents
            .FirstOrDefaultAsync(i => i.SourceType == resourceType && i.SourceId == resourceId && i.Status != IncidentStatus.Resolved && i.Status != IncidentStatus.Closed);

        if (existingIncident != null)
        {
            await AddTimelineEntry(existingIncident.Id, "ResourceStatusUpdate", $"Status update: {description}");
            return existingIncident;
        }

        var incident = new Incident
        {
            OrganizationId = organizationId,
            CustomerId = customerId,
            Title = title,
            Description = description,
            Status = IncidentStatus.New,
            Severity = severity,
            Priority = MapSeverityToPriority(severity),
            SourceType = resourceType,
            SourceId = resourceId,
            AffectedResources = JsonSerializer.Serialize(new[] {
                new { 
                    Type = resourceType, 
                    Id = resourceId, 
                    Name = resourceName
                }
            })
        };

        _context.Incidents.Add(incident);
        await _context.SaveChangesAsync();

        await AddTimelineEntry(incident.Id, "Created", $"Incident automatically created: {title}");
        
        // Notify
        await _notificationService.SendIncidentCreatedNotification(incident);

        return incident;
    }

    public async Task AutoResolveIncidentsAsync(string resourceType, Guid resourceId, string resolutionNote)
    {
        var incidents = await _context.Incidents
            .Where(i => i.SourceType == resourceType && i.SourceId == resourceId && i.Status != IncidentStatus.Resolved && i.Status != IncidentStatus.Closed)
            .ToListAsync();

        foreach (var incident in incidents)
        {
            incident.Status = IncidentStatus.Resolved;
            incident.ResolvedAt = DateTime.UtcNow;
            incident.ResolutionSteps = resolutionNote;
            
            await AddTimelineEntry(incident.Id, "AutoResolved", resolutionNote);
            
            // Notify
            await _notificationService.SendIncidentResolvedNotification(incident);
        }
        
        if (incidents.Any())
        {
            await _context.SaveChangesAsync();
        }
    }

    public async Task AddTimelineEntry(Guid incidentId, string eventType, string content, Guid? userId = null)
    {
        var entry = new IncidentTimeline
        {
            IncidentId = incidentId,
            EventType = eventType,
            Content = content,
            UserId = userId,
            CreatedAt = DateTime.UtcNow
        };

        _context.IncidentTimelines.Add(entry);
        await _context.SaveChangesAsync();
    }

    private IncidentPriority MapSeverityToPriority(IncidentSeverity severity)
    {
        return severity switch
        {
            IncidentSeverity.Critical => IncidentPriority.Urgent,
            IncidentSeverity.High => IncidentPriority.High,
            IncidentSeverity.Medium => IncidentPriority.Medium,
            IncidentSeverity.Low => IncidentPriority.Low,
            _ => IncidentPriority.Medium
        };
    }
}
