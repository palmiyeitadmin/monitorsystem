using System.Security.Claims;
using ERAMonitor.Core.DTOs;
using ERAMonitor.Core.Entities;
using ERAMonitor.Core.Enums;
using ERAMonitor.Core.Interfaces;
using ERAMonitor.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ERAMonitor.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class IncidentsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IIncidentService _incidentService;

    public IncidentsController(ApplicationDbContext context, IIncidentService incidentService)
    {
        _context = context;
        _incidentService = incidentService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<IncidentDto>>> GetIncidents(
        [FromQuery] Guid? customerId,
        [FromQuery] IncidentStatus? status,
        [FromQuery] IncidentSeverity? severity,
        [FromQuery] Guid? assignedToId)
    {
        var query = _context.Incidents.AsQueryable();

        if (customerId.HasValue)
            query = query.Where(i => i.CustomerId == customerId);

        if (status.HasValue)
            query = query.Where(i => i.Status == status);

        if (severity.HasValue)
            query = query.Where(i => i.Severity == severity);

        if (assignedToId.HasValue)
            query = query.Where(i => i.AssignedToId == assignedToId);

        var incidents = await query
            .Include(i => i.AssignedTo)
            .Select(i => new IncidentDto
            {
                Id = i.Id,
                Title = i.Title,
                Description = i.Description,
                Status = i.Status.ToString(),
                Severity = i.Severity.ToString(),
                Priority = i.Priority.ToString(),
                SourceType = i.SourceType,
                SourceId = i.SourceId,
                CreatedAt = i.CreatedAt,
                AcknowledgedAt = i.AcknowledgedAt,
                ResolvedAt = i.ResolvedAt,
                AssignedToId = i.AssignedToId,
                AssignedToName = i.AssignedTo != null ? i.AssignedTo.FullName : null,
                ResponseSlaMet = i.ResponseSlaMet,
                ResolutionSlaMet = i.ResolutionSlaMet,
                CustomerId = i.CustomerId
            })
            .ToListAsync();

        return Ok(incidents);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<IncidentDto>> GetIncident(Guid id)
    {
        var i = await _context.Incidents
            .Include(x => x.AssignedTo)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (i == null)
        {
            return NotFound();
        }

        return new IncidentDto
        {
            Id = i.Id,
            Title = i.Title,
            Description = i.Description,
            Status = i.Status.ToString(),
            Severity = i.Severity.ToString(),
            Priority = i.Priority.ToString(),
            SourceType = i.SourceType,
            SourceId = i.SourceId,
            CreatedAt = i.CreatedAt,
            AcknowledgedAt = i.AcknowledgedAt,
            ResolvedAt = i.ResolvedAt,
            AssignedToId = i.AssignedToId,
            AssignedToName = i.AssignedTo != null ? i.AssignedTo.FullName : null,
            ResponseSlaMet = i.ResponseSlaMet,
            ResolutionSlaMet = i.ResolutionSlaMet,
            CustomerId = i.CustomerId
        };
    }

    [HttpPost]
    public async Task<ActionResult<IncidentDto>> CreateIncident(CreateIncidentRequest request)
    {
        var orgId = Guid.Parse(User.FindFirstValue("OrganizationId")!);

        var incident = new Incident
        {
            OrganizationId = orgId,
            CustomerId = request.CustomerId,
            Title = request.Title,
            Description = request.Description,
            Status = IncidentStatus.New,
            Severity = request.Severity,
            Priority = request.Priority,
            SourceType = "Manual",
            AssignedToId = request.AssignedToId
        };

        _context.Incidents.Add(incident);
        await _context.SaveChangesAsync();

        await _incidentService.AddTimelineEntry(incident.Id, "Created", "Incident manually created", Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!));

        return CreatedAtAction(nameof(GetIncident), new { id = incident.Id }, new IncidentDto
        {
            Id = incident.Id,
            Title = incident.Title,
            Status = incident.Status.ToString()
        });
    }

    [HttpPost("{id}/acknowledge")]
    public async Task<IActionResult> Acknowledge(Guid id)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        await _incidentService.AcknowledgeIncident(id, userId);
        return NoContent();
    }

    [HttpPost("{id}/resolve")]
    public async Task<IActionResult> Resolve(Guid id, ResolveIncidentRequest request)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        await _incidentService.ResolveIncident(id, userId, request);
        return NoContent();
    }

    [HttpGet("{id}/timeline")]
    public async Task<ActionResult<IEnumerable<IncidentTimelineDto>>> GetTimeline(Guid id)
    {
        var timeline = await _context.IncidentTimelines
            .Where(t => t.IncidentId == id)
            .OrderBy(t => t.CreatedAt)
            .Select(t => new IncidentTimelineDto
            {
                Id = t.Id,
                EventType = t.EventType,
                Content = t.Content,
                CreatedAt = t.CreatedAt,
                CreatedByName = t.User != null ? t.User.FullName : "System"
            })
            .ToListAsync();

        return Ok(timeline);
    }
}
