using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ERAMonitor.Core.DTOs.Common;
using ERAMonitor.Core.DTOs.Services;
using ERAMonitor.Core.Enums;
using ERAMonitor.Core.Interfaces.Services;
using ERAMonitor.API.Extensions;

namespace ERAMonitor.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ServicesController : ControllerBase
{
    private readonly IServiceMonitorService _serviceMonitorService;
    private readonly ILogger<ServicesController> _logger;
    
    public ServicesController(
        IServiceMonitorService serviceMonitorService, 
        ILogger<ServicesController> logger)
    {
        _serviceMonitorService = serviceMonitorService;
        _logger = logger;
    }
    
    /// <summary>
    /// Get paginated list of services
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<ServiceListItemDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResponse<ServiceListItemDto>>> GetServices(
        [FromQuery] PagedRequest request,
        [FromQuery] Guid? hostId = null,
        [FromQuery] Guid? customerId = null,
        [FromQuery] ServiceType? serviceType = null,
        [FromQuery] StatusType? status = null)
    {
        var organizationId = User.GetOrganizationId();
        var result = await _serviceMonitorService.GetPagedAsync(
            organizationId,
            request,
            hostId,
            customerId,
            serviceType,
            status
        );
        return Ok(result);
    }
    
    /// <summary>
    /// Get service by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ServiceDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ServiceDetailDto>> GetService(Guid id)
    {
        var service = await _serviceMonitorService.GetByIdAsync(id);
        return Ok(service);
    }
    
    /// <summary>
    /// Update service monitoring settings
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Policy = "RequireOperatorRole")]
    [ProducesResponseType(typeof(ServiceDetailDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<ServiceDetailDto>> UpdateService(Guid id, [FromBody] UpdateServiceRequest request)
    {
        var service = await _serviceMonitorService.UpdateAsync(id, request);
        return Ok(service);
    }
    
    /// <summary>
    /// Toggle service monitoring
    /// </summary>
    [HttpPost("{id}/toggle-monitoring")]
    [Authorize(Policy = "RequireOperatorRole")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<bool>>> ToggleMonitoring(Guid id)
    {
        var isEnabled = await _serviceMonitorService.ToggleMonitoringAsync(id);
        return Ok(ApiResponse<bool>.Ok(isEnabled, isEnabled ? "Monitoring enabled" : "Monitoring disabled"));
    }
    
    /// <summary>
    /// Get service status history
    /// </summary>
    [HttpGet("{id}/history")]
    [ProducesResponseType(typeof(List<ServiceStatusHistoryDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<ServiceStatusHistoryDto>>> GetStatusHistory(
        Guid id,
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null)
    {
        var fromDate = from ?? DateTime.UtcNow.AddDays(-7);
        var toDate = to ?? DateTime.UtcNow;
        
        var history = await _serviceMonitorService.GetStatusHistoryAsync(id, fromDate, toDate);
        return Ok(history);
    }
    
    /// <summary>
    /// Get service status counts
    /// </summary>
    [HttpGet("status-counts")]
    [ProducesResponseType(typeof(Dictionary<string, int>), StatusCodes.Status200OK)]
    public async Task<ActionResult<Dictionary<string, int>>> GetStatusCounts()
    {
        var organizationId = User.GetOrganizationId();
        var counts = await _serviceMonitorService.GetStatusCountsAsync(organizationId);
        return Ok(counts.ToDictionary(k => k.Key.ToString(), v => v.Value));
    }
    
    /// <summary>
    /// Get service counts by type
    /// </summary>
    [HttpGet("type-counts")]
    [ProducesResponseType(typeof(Dictionary<string, int>), StatusCodes.Status200OK)]
    public async Task<ActionResult<Dictionary<string, int>>> GetTypeCounts()
    {
        var organizationId = User.GetOrganizationId();
        var counts = await _serviceMonitorService.GetCountsByTypeAsync(organizationId);
        return Ok(counts.ToDictionary(k => k.Key.ToString(), v => v.Value));
    }
}
