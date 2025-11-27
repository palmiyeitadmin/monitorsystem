using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ERAMonitor.Core.DTOs.Common;
using ERAMonitor.Core.DTOs.EventLog;
using ERAMonitor.Core.Interfaces.Repositories;

namespace ERAMonitor.API.Controllers;

[ApiController]
[Route("api/hosts/{hostId}/[controller]")]
[Authorize]
public class EventLogsController : ControllerBase
{
    private readonly IEventLogRepository _eventLogRepository;
    private readonly ILogger<EventLogsController> _logger;

    public EventLogsController(
        IEventLogRepository eventLogRepository,
        ILogger<EventLogsController> logger)
    {
        _eventLogRepository = eventLogRepository;
        _logger = logger;
    }

    /// <summary>
    /// Get event logs for a host with filtering and pagination
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<PagedResponse<EventLogListItemDto>>> GetEventLogs(
        Guid hostId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        [FromQuery] string? category = null,
        [FromQuery] string? level = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        [FromQuery] string? search = null)
    {
        try
        {
            var request = new PagedRequest
            {
                Page = page,
                PageSize = Math.Min(pageSize, 100), // Max 100 per page
                Search = search
            };

            var result = await _eventLogRepository.GetPagedByHostAsync(
                hostId,
                request,
                category,
                level,
                fromDate,
                toDate);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving event logs for host {HostId}", hostId);
            return StatusCode(500, "Error retrieving event logs");
        }
    }

    /// <summary>
    /// Get available categories for a host
    /// </summary>
    [HttpGet("categories")]
    public async Task<ActionResult<List<string>>> GetCategories(Guid hostId)
    {
        try
        {
            var categories = await _eventLogRepository.GetCategoriesByHostAsync(hostId);
            return Ok(categories);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving event log categories for host {HostId}", hostId);
            return StatusCode(500, "Error retrieving categories");
        }
    }
}
