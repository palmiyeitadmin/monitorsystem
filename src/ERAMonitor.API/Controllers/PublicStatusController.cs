using ERAMonitor.Core.DTOs.StatusPages;
using ERAMonitor.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ERAMonitor.API.Controllers;

[ApiController]
[Route("api/public/status")]
public class PublicStatusController : ControllerBase
{
    private readonly IStatusPageService _statusPageService;

    public PublicStatusController(IStatusPageService statusPageService)
    {
        _statusPageService = statusPageService;
    }

    [HttpGet("{slug}")]
    public async Task<ActionResult<StatusPageDetailDto>> GetBySlug(string slug)
    {
        var result = await _statusPageService.GetBySlugAsync(slug);
        if (result == null || !result.IsPublic || !result.IsActive) return NotFound();
        
        return Ok(result);
    }

    [HttpPost("{slug}/subscribe")]
    public async Task<ActionResult<StatusPageSubscriberDto>> Subscribe(string slug, CreateStatusPageSubscriberDto dto)
    {
        var statusPage = await _statusPageService.GetBySlugAsync(slug);
        if (statusPage == null || !statusPage.IsPublic || !statusPage.IsActive) return NotFound();

        var result = await _statusPageService.AddSubscriberAsync(statusPage.Id, dto);
        return CreatedAtAction(nameof(GetBySlug), new { slug = slug }, result);
    }

    [HttpGet("verify/{token}")]
    public async Task<ActionResult> VerifySubscription(string token, [FromQuery] Guid subscriberId)
    {
        var result = await _statusPageService.VerifySubscriberAsync(subscriberId, token);
        if (!result) return BadRequest("Invalid verification token or subscriber ID.");
        
        return Ok("Subscription verified successfully.");
    }

    // Unsubscribe endpoint?
    // Usually unsubscribe link has a token or ID.
    // For now, skipping public unsubscribe endpoint as it requires secure token handling not fully defined.
}
