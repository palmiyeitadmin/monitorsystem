using System.Security.Claims;
using ERAMonitor.Core.Entities;
using ERAMonitor.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ERAMonitor.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class PortalController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<PortalController> _logger;

    public PortalController(ApplicationDbContext context, ILogger<PortalController> logger)
    {
        _context = context;
        _logger = logger;
    }

    private Guid GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        return userIdClaim != null ? Guid.Parse(userIdClaim.Value) : Guid.Empty;
    }

    private async Task<Guid?> GetCustomerIdAsync()
    {
        var userId = GetUserId();
        var assignment = await _context.UserCustomerAssignments
            .FirstOrDefaultAsync(uca => uca.UserId == userId);
        
        return assignment?.CustomerId;
    }

    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboard()
    {
        var customerId = await GetCustomerIdAsync();
        if (customerId == null)
        {
            return NotFound("User is not assigned to any customer.");
        }

        var services = await _context.Services
            .Include(s => s.Host)
            .Where(s => s.Host.CustomerId == customerId)
            .CountAsync();

        var incidents = await _context.Incidents
            .Where(i => i.CustomerId == customerId && i.ResolvedAt == null)
            .CountAsync();

        var checks = await _context.Checks
            .Where(c => c.CustomerId == customerId && c.IsActive)
            .CountAsync();

        return Ok(new
        {
            TotalServices = services,
            ActiveIncidents = incidents,
            ActiveChecks = checks
        });
    }

    [HttpGet("services")]
    public async Task<IActionResult> GetServices()
    {
        var customerId = await GetCustomerIdAsync();
        if (customerId == null)
        {
            return NotFound("User is not assigned to any customer.");
        }

        var services = await _context.Services
            .Include(s => s.Host)
            .Where(s => s.Host.CustomerId == customerId)
            .Select(s => new
            {
                s.Id,
                Name = s.ServiceName,
                Type = s.ServiceType,
                Status = s.CurrentStatus,
                UptimePercent = 100 // TODO: Calculate uptime
            })
            .ToListAsync();

        return Ok(services);
    }

    [HttpGet("incidents")]
    public async Task<IActionResult> GetIncidents()
    {
        var customerId = await GetCustomerIdAsync();
        if (customerId == null)
        {
            return NotFound("User is not assigned to any customer.");
        }

        var incidents = await _context.Incidents
            .Where(i => i.CustomerId == customerId)
            .OrderByDescending(i => i.CreatedAt)
            .Select(i => new
            {
                i.Id,
                i.Title,
                i.Severity,
                i.Status,
                i.CreatedAt,
                i.ResolvedAt
            })
            .ToListAsync();

        return Ok(incidents);
    }
}
