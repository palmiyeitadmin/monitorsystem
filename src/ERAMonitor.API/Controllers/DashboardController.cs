using System.Security.Claims;
using ERAMonitor.Core.DTOs.Common;
using ERAMonitor.Core.DTOs.Dashboard;
using ERAMonitor.Core.Interfaces;
using ERAMonitor.Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ERAMonitor.API.Extensions;

namespace ERAMonitor.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;
    private readonly IHostService _hostService;

    public DashboardController(
        IDashboardService dashboardService,
        IHostService hostService)
    {
        _dashboardService = dashboardService;
        _hostService = hostService;
    }

    [HttpGet("status-overview")]
    public async Task<ActionResult<object>> GetStatusOverview()
    {
        var organizationId = User.GetOrganizationId();
        
        // Get host counts
        var hostCounts = await _hostService.GetStatusCountsAsync(organizationId);
        var totalHosts = hostCounts.Values.Sum();
        var hostsUp = hostCounts.GetValueOrDefault(Core.Enums.StatusType.Up, 0);
        var hostsDown = hostCounts.GetValueOrDefault(Core.Enums.StatusType.Down, 0);
        var hostsWarning = hostCounts.GetValueOrDefault(Core.Enums.StatusType.Warning, 0) +
                          hostCounts.GetValueOrDefault(Core.Enums.StatusType.Degraded, 0);

        var overview = new
        {
            totalHosts,
            hostsUp,
            hostsDown,
            hostsWarning,
            totalServices = 0,
            servicesUp = 0,
            servicesDown = 0,
            totalChecks = 0,
            checksPassing = 0,
            checksFailing = 0,
            openIncidents = 0,
            criticalIncidents = 0,
            overallHealth = totalHosts > 0 ? (hostsUp * 100 / totalHosts) : 100
        };

        return Ok(overview);
    }

    [HttpGet]
    public async Task<ActionResult<PagedResponse<DashboardDto>>> GetPaged([FromQuery] PagedRequest request)
    {
        var orgId = Guid.Parse(User.FindFirst("OrganizationId")?.Value ?? throw new UnauthorizedAccessException());
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? throw new UnauthorizedAccessException());
        
        var result = await _dashboardService.GetPagedAsync(orgId, userId, request);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<DashboardDetailDto>> GetDetail(Guid id)
    {
        var orgId = Guid.Parse(User.FindFirst("OrganizationId")?.Value ?? throw new UnauthorizedAccessException());
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? throw new UnauthorizedAccessException());

        var result = await _dashboardService.GetDetailAsync(id, orgId, userId);
        if (result == null) return NotFound();
        
        return Ok(result);
    }

    [HttpGet("slug/{slug}")]
    public async Task<ActionResult<DashboardDetailDto>> GetBySlug(string slug)
    {
        var orgId = Guid.Parse(User.FindFirst("OrganizationId")?.Value ?? throw new UnauthorizedAccessException());
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? throw new UnauthorizedAccessException());

        var result = await _dashboardService.GetBySlugAsync(slug, orgId, userId);
        if (result == null) return NotFound();

        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<DashboardDetailDto>> Create(CreateDashboardDto dto)
    {
        var orgId = Guid.Parse(User.FindFirst("OrganizationId")?.Value ?? throw new UnauthorizedAccessException());
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? throw new UnauthorizedAccessException());

        var result = await _dashboardService.CreateAsync(orgId, userId, dto);
        return CreatedAtAction(nameof(GetDetail), new { id = result.Id }, result);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<DashboardDetailDto>> Update(Guid id, UpdateDashboardDto dto)
    {
        var orgId = Guid.Parse(User.FindFirst("OrganizationId")?.Value ?? throw new UnauthorizedAccessException());
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? throw new UnauthorizedAccessException());

        var result = await _dashboardService.UpdateAsync(id, orgId, userId, dto);
        if (result == null) return NotFound();

        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(Guid id)
    {
        var orgId = Guid.Parse(User.FindFirst("OrganizationId")?.Value ?? throw new UnauthorizedAccessException());
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? throw new UnauthorizedAccessException());

        var result = await _dashboardService.DeleteAsync(id, orgId, userId);
        if (!result) return NotFound();

        return NoContent();
    }

    // Widget Endpoints

    [HttpPost("{id}/widgets")]
    public async Task<ActionResult<DashboardWidgetDto>> AddWidget(Guid id, CreateWidgetDto dto)
    {
        var orgId = Guid.Parse(User.FindFirst("OrganizationId")?.Value ?? throw new UnauthorizedAccessException());
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? throw new UnauthorizedAccessException());

        try
        {
            var result = await _dashboardService.AddWidgetAsync(id, orgId, userId, dto);
            return Ok(result);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    [HttpPut("widgets/{widgetId}")]
    public async Task<ActionResult<DashboardWidgetDto>> UpdateWidget(Guid widgetId, UpdateWidgetDto dto)
    {
        var orgId = Guid.Parse(User.FindFirst("OrganizationId")?.Value ?? throw new UnauthorizedAccessException());
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? throw new UnauthorizedAccessException());

        var result = await _dashboardService.UpdateWidgetAsync(widgetId, orgId, userId, dto);
        if (result == null) return NotFound();

        return Ok(result);
    }

    [HttpDelete("widgets/{widgetId}")]
    public async Task<ActionResult> DeleteWidget(Guid widgetId)
    {
        var orgId = Guid.Parse(User.FindFirst("OrganizationId")?.Value ?? throw new UnauthorizedAccessException());
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? throw new UnauthorizedAccessException());

        var result = await _dashboardService.DeleteWidgetAsync(widgetId, orgId, userId);
        if (!result) return NotFound();

        return NoContent();
    }

    [HttpPut("{id}/widgets/positions")]
    public async Task<ActionResult> UpdateWidgetPositions(Guid id, List<Guid> widgetIds)
    {
        var orgId = Guid.Parse(User.FindFirst("OrganizationId")?.Value ?? throw new UnauthorizedAccessException());
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? throw new UnauthorizedAccessException());

        await _dashboardService.UpdateWidgetPositionsAsync(id, orgId, userId, widgetIds);
        return NoContent();
    }
}
