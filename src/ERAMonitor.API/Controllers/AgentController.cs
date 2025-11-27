using Microsoft.AspNetCore.Mvc;
using ERAMonitor.Core.DTOs.Agent;
using ERAMonitor.Core.DTOs.Common;
using ERAMonitor.Core.Interfaces.Services;

namespace ERAMonitor.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AgentController : ControllerBase
{
    private readonly IHeartbeatService _heartbeatService;
    private readonly ILogger<AgentController> _logger;
    
    private const string API_KEY_HEADER = "X-API-Key";
    
    public AgentController(IHeartbeatService heartbeatService, ILogger<AgentController> logger)
    {
        _heartbeatService = heartbeatService;
        _logger = logger;
    }
    
    /// <summary>
    /// Receive heartbeat from monitoring agent
    /// </summary>
    /// <remarks>
    /// Agents should send heartbeats at regular intervals (default 60 seconds).
    /// The heartbeat contains system metrics, disk usage, and service status information.
    /// </remarks>
    [HttpPost("heartbeat")]
    [ProducesResponseType(typeof(HeartbeatResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<HeartbeatResponse>> Heartbeat([FromBody] HeartbeatRequest request)
    {
        // Get API key from header
        if (!Request.Headers.TryGetValue(API_KEY_HEADER, out var apiKeyHeader))
        {
            return Unauthorized(new ErrorResponse
            {
                Code = "MISSING_API_KEY",
                Message = "X-API-Key header is required"
            });
        }
        
        var apiKey = apiKeyHeader.ToString();
        
        if (string.IsNullOrEmpty(apiKey))
        {
            return Unauthorized(new ErrorResponse
            {
                Code = "INVALID_API_KEY",
                Message = "API key cannot be empty"
            });
        }
        
        try
        {
            var response = await _heartbeatService.ProcessHeartbeatAsync(apiKey, request);
            return Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("Unauthorized heartbeat attempt: {Message}", ex.Message);
            return Unauthorized(new ErrorResponse
            {
                Code = "UNAUTHORIZED",
                Message = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing heartbeat");
            return BadRequest(new ErrorResponse
            {
                Code = "HEARTBEAT_ERROR",
                Message = "Error processing heartbeat"
            });
        }
    }
    
    /// <summary>
    /// Get agent configuration and commands
    /// </summary>
    [HttpGet("config")]
    [ProducesResponseType(typeof(AgentConfigResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AgentConfigResponse>> GetConfig()
    {
        if (!Request.Headers.TryGetValue(API_KEY_HEADER, out var apiKeyHeader))
        {
            return Unauthorized();
        }
        
        // Return configuration for the agent
        // This could include check interval, which services to monitor, etc.
        return Ok(new AgentConfigResponse
        {
            CheckIntervalSeconds = 60,
            // Add other configuration options
        });
    }
}

public class AgentConfigResponse
{
    public int CheckIntervalSeconds { get; set; }
    public bool CollectCpu { get; set; } = true;
    public bool CollectRam { get; set; } = true;
    public bool CollectDisk { get; set; } = true;
    public bool CollectNetwork { get; set; } = false;
    public bool CollectServices { get; set; } = true;
    public List<string>? ServicesToMonitor { get; set; }
}
