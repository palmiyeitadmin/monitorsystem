using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ERAMonitor.Core.DTOs.Common;
using ERAMonitor.Core.DTOs.Hosts;
using ERAMonitor.Core.Enums;
using ERAMonitor.Core.Interfaces.Services;
using ERAMonitor.API.Extensions;

namespace ERAMonitor.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class HostsController : ControllerBase
{
    private readonly IHostService _hostService;
    private readonly ILogger<HostsController> _logger;
    
    public HostsController(IHostService hostService, ILogger<HostsController> logger)
    {
        _hostService = hostService;
        _logger = logger;
    }
    
    /// <summary>
    /// Get paginated list of hosts
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<HostListItemDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResponse<HostListItemDto>>> GetHosts(
        [FromQuery] PagedRequest request,
        [FromQuery] StatusType? status = null,
        [FromQuery] Guid? customerId = null,
        [FromQuery] Guid? locationId = null,
        [FromQuery] OsType? osType = null,
        [FromQuery] string[]? tags = null)
    {
        var organizationId = User.GetOrganizationId();
        var result = await _hostService.GetPagedAsync(
            organizationId,
            request,
            status,
            customerId,
            locationId,
            osType,
            tags
        );
        return Ok(result);
    }
    
    /// <summary>
    /// Get host by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(HostDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<HostDetailDto>> GetHost(Guid id)
    {
        var organizationId = User.GetOrganizationId();
        var host = await _hostService.GetByIdAsync(id, organizationId);
        return Ok(host);
    }
    
    /// <summary>
    /// Create new host
    /// </summary>
    [HttpPost]
    [Authorize(Policy = "RequireAdminRole")]
    [ProducesResponseType(typeof(HostDetailDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<HostDetailDto>> CreateHost([FromBody] CreateHostRequest request)
    {
        var organizationId = User.GetOrganizationId();
        var host = await _hostService.CreateAsync(organizationId, request);
        return CreatedAtAction(nameof(GetHost), new { id = host.Id }, host);
    }
    
    /// <summary>
    /// Update host
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Policy = "RequireAdminRole")]
    [ProducesResponseType(typeof(HostDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<HostDetailDto>> UpdateHost(Guid id, [FromBody] UpdateHostRequest request)
    {
        var organizationId = User.GetOrganizationId();
        var host = await _hostService.UpdateAsync(id, organizationId, request);
        return Ok(host);
    }
    
    /// <summary>
    /// Delete host
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Policy = "RequireAdminRole")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse>> DeleteHost(Guid id)
    {
        var organizationId = User.GetOrganizationId();
        await _hostService.DeleteAsync(id, organizationId);
        return Ok(ApiResponse.Ok("Host deleted successfully"));
    }
    
    /// <summary>
    /// Regenerate host API key
    /// </summary>
    [HttpPost("{id}/regenerate-api-key")]
    [Authorize(Policy = "RequireAdminRole")]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<string>>> RegenerateApiKey(Guid id)
    {
        var organizationId = User.GetOrganizationId();
        var newApiKey = await _hostService.RegenerateApiKeyAsync(id, organizationId);
        return Ok(ApiResponse<string>.Ok(newApiKey, "API key regenerated. Please update the agent configuration."));
    }
    
    /// <summary>
    /// Set maintenance mode
    /// </summary>
    [HttpPost("{id}/maintenance")]
    [Authorize(Policy = "RequireOperatorRole")]
    [ProducesResponseType(typeof(HostDetailDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<HostDetailDto>> SetMaintenance(Guid id, [FromBody] SetMaintenanceRequest request)
    {
        var organizationId = User.GetOrganizationId();
        var host = await _hostService.SetMaintenanceAsync(id, organizationId, request);
        return Ok(host);
    }
    
    /// <summary>
    /// Toggle monitoring enabled
    /// </summary>
    [HttpPost("{id}/toggle-monitoring")]
    [Authorize(Policy = "RequireOperatorRole")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<bool>>> ToggleMonitoring(Guid id)
    {
        var organizationId = User.GetOrganizationId();
        var isEnabled = await _hostService.ToggleMonitoringAsync(id, organizationId);
        return Ok(ApiResponse<bool>.Ok(isEnabled, isEnabled ? "Monitoring enabled" : "Monitoring disabled"));
    }
    
    /// <summary>
    /// Get host metrics history
    /// </summary>
    [HttpGet("{id}/metrics")]
    [ProducesResponseType(typeof(HostMetricsDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<HostMetricsDto>> GetMetrics(
        Guid id,
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null,
        [FromQuery] string interval = "5m")
    {
        var organizationId = User.GetOrganizationId();
        var fromDate = from ?? DateTime.UtcNow.AddHours(-24);
        var toDate = to ?? DateTime.UtcNow;
        
        var metrics = await _hostService.GetMetricsAsync(id, organizationId, fromDate, toDate, interval);
        return Ok(metrics);
    }
    
    /// <summary>
    /// Get host status counts for dashboard
    /// </summary>
    [HttpGet("status-counts")]
    [ProducesResponseType(typeof(Dictionary<string, int>), StatusCodes.Status200OK)]
    public async Task<ActionResult<Dictionary<string, int>>> GetStatusCounts()
    {
        var organizationId = User.GetOrganizationId();
        var counts = await _hostService.GetStatusCountsAsync(organizationId);
        return Ok(counts.ToDictionary(k => k.Key.ToString(), v => v.Value));
    }
    
    /// <summary>
    /// Get recently down hosts
    /// </summary>
    [HttpGet("recently-down")]
    [ProducesResponseType(typeof(List<HostListItemDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<HostListItemDto>>> GetRecentlyDown([FromQuery] int limit = 10)
    {
        var organizationId = User.GetOrganizationId();
        var hosts = await _hostService.GetRecentlyDownAsync(organizationId, limit);
        return Ok(hosts);
    }
    
    /// <summary>
    /// Get hosts with high resource usage
    /// </summary>
    [HttpGet("high-usage")]
    [ProducesResponseType(typeof(List<HostListItemDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<HostListItemDto>>> GetHighResourceUsage([FromQuery] int limit = 10)
    {
        var organizationId = User.GetOrganizationId();
        var hosts = await _hostService.GetHighResourceUsageAsync(organizationId, limit);
        return Ok(hosts);
    }
}
