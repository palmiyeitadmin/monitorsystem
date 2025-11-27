using System.Security.Claims;
using ERAMonitor.Core.DTOs.Common;
using ERAMonitor.Core.DTOs.StatusPages;
using ERAMonitor.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERAMonitor.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class StatusPagesController : ControllerBase
{
    private readonly IStatusPageService _statusPageService;

    public StatusPagesController(IStatusPageService statusPageService)
    {
        _statusPageService = statusPageService;
    }

    // Status Page CRUD
    [HttpGet]
    public async Task<ActionResult<PagedResponse<StatusPageDto>>> GetPaged([FromQuery] PagedRequest request)
    {
        var orgId = Guid.Parse(User.FindFirst("OrganizationId")?.Value ?? throw new UnauthorizedAccessException());
        var result = await _statusPageService.GetPagedAsync(orgId, request);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<StatusPageDetailDto>> GetDetail(Guid id)
    {
        var orgId = Guid.Parse(User.FindFirst("OrganizationId")?.Value ?? throw new UnauthorizedAccessException());
        var result = await _statusPageService.GetDetailAsync(id, orgId);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<StatusPageDetailDto>> Create(CreateStatusPageDto dto)
    {
        var orgId = Guid.Parse(User.FindFirst("OrganizationId")?.Value ?? throw new UnauthorizedAccessException());
        var result = await _statusPageService.CreateAsync(orgId, dto);
        return CreatedAtAction(nameof(GetDetail), new { id = result.Id }, result);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<StatusPageDetailDto>> Update(Guid id, UpdateStatusPageDto dto)
    {
        var orgId = Guid.Parse(User.FindFirst("OrganizationId")?.Value ?? throw new UnauthorizedAccessException());
        var result = await _statusPageService.UpdateAsync(id, orgId, dto);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(Guid id)
    {
        var orgId = Guid.Parse(User.FindFirst("OrganizationId")?.Value ?? throw new UnauthorizedAccessException());
        var result = await _statusPageService.DeleteAsync(id, orgId);
        if (!result) return NotFound();
        return NoContent();
    }

    // Component Management
    [HttpPost("{id}/components")]
    public async Task<ActionResult<StatusPageComponentDto>> AddComponent(Guid id, CreateStatusPageComponentDto dto)
    {
        var orgId = Guid.Parse(User.FindFirst("OrganizationId")?.Value ?? throw new UnauthorizedAccessException());
        try
        {
            var result = await _statusPageService.AddComponentAsync(id, orgId, dto);
            return Ok(result);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpPut("components/{componentId}")]
    public async Task<ActionResult<StatusPageComponentDto>> UpdateComponent(Guid componentId, UpdateStatusPageComponentDto dto)
    {
        var orgId = Guid.Parse(User.FindFirst("OrganizationId")?.Value ?? throw new UnauthorizedAccessException());
        var result = await _statusPageService.UpdateComponentAsync(componentId, orgId, dto);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpDelete("components/{componentId}")]
    public async Task<ActionResult> DeleteComponent(Guid componentId)
    {
        var orgId = Guid.Parse(User.FindFirst("OrganizationId")?.Value ?? throw new UnauthorizedAccessException());
        var result = await _statusPageService.DeleteComponentAsync(componentId, orgId);
        if (!result) return NotFound();
        return NoContent();
    }

    [HttpPut("components/{componentId}/status")]
    public async Task<ActionResult> UpdateComponentStatus(Guid componentId, UpdateComponentStatusDto dto)
    {
        var orgId = Guid.Parse(User.FindFirst("OrganizationId")?.Value ?? throw new UnauthorizedAccessException());
        await _statusPageService.UpdateComponentStatusAsync(componentId, orgId, dto);
        return NoContent();
    }

    // Group Management
    [HttpPost("{id}/groups")]
    public async Task<ActionResult<StatusPageComponentGroupDto>> AddGroup(Guid id, CreateStatusPageComponentGroupDto dto)
    {
        var orgId = Guid.Parse(User.FindFirst("OrganizationId")?.Value ?? throw new UnauthorizedAccessException());
        try
        {
            var result = await _statusPageService.AddGroupAsync(id, orgId, dto);
            return Ok(result);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpPut("groups/{groupId}")]
    public async Task<ActionResult<StatusPageComponentGroupDto>> UpdateGroup(Guid groupId, UpdateStatusPageComponentGroupDto dto)
    {
        var orgId = Guid.Parse(User.FindFirst("OrganizationId")?.Value ?? throw new UnauthorizedAccessException());
        var result = await _statusPageService.UpdateGroupAsync(groupId, orgId, dto);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpDelete("groups/{groupId}")]
    public async Task<ActionResult> DeleteGroup(Guid groupId)
    {
        var orgId = Guid.Parse(User.FindFirst("OrganizationId")?.Value ?? throw new UnauthorizedAccessException());
        var result = await _statusPageService.DeleteGroupAsync(groupId, orgId);
        if (!result) return NotFound();
        return NoContent();
    }

    // Subscriber Management
    [HttpGet("{id}/subscribers")]
    public async Task<ActionResult<PagedResponse<StatusPageSubscriberDto>>> GetSubscribers(Guid id, [FromQuery] PagedRequest request)
    {
        var orgId = Guid.Parse(User.FindFirst("OrganizationId")?.Value ?? throw new UnauthorizedAccessException());
        try
        {
            var result = await _statusPageService.GetSubscribersAsync(id, orgId, request);
            return Ok(result);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpDelete("{id}/subscribers/{subscriberId}")]
    public async Task<ActionResult> RemoveSubscriber(Guid id, Guid subscriberId)
    {
        var orgId = Guid.Parse(User.FindFirst("OrganizationId")?.Value ?? throw new UnauthorizedAccessException());
        var result = await _statusPageService.RemoveSubscriberAsync(subscriberId, id, orgId);
        if (!result) return NotFound();
        return NoContent();
    }
}
