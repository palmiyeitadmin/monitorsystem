using System.Security.Claims;
using ERAMonitor.Core.DTOs.Common;
using ERAMonitor.Core.DTOs.Dashboard;
using ERAMonitor.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERAMonitor.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
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
