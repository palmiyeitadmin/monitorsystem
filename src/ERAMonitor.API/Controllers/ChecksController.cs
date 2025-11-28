using System.Security.Claims;
using System.Text.Json;
using ERAMonitor.Core.DTOs;
using ERAMonitor.Core.Entities;
using ERAMonitor.Core.Enums;
using ERAMonitor.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ERAMonitor.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ChecksController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public ChecksController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CheckDto>>> GetChecks(
        [FromQuery] Guid? customerId,
        [FromQuery] Guid? hostId,
        [FromQuery] CheckType? type,
        [FromQuery] StatusType? status,
        [FromQuery] string? search)
    {
        var query = _context.Checks.AsQueryable();

        if (customerId.HasValue)
            query = query.Where(c => c.CustomerId == customerId);

        if (hostId.HasValue)
            query = query.Where(c => c.HostId == hostId);

        if (type.HasValue)
            query = query.Where(c => c.CheckType == type);

        if (status.HasValue)
            query = query.Where(c => c.CurrentStatus == status);

        if (!string.IsNullOrEmpty(search))
            query = query.Where(c => c.Name.Contains(search) || c.Target.Contains(search));

        var checks = await query
            .Select(c => new CheckDto
            {
                Id = c.Id,
                Name = c.Name,
                CheckType = c.CheckType.ToString(),
                Target = c.Target,
                IntervalSeconds = c.IntervalSeconds,
                TimeoutSeconds = c.TimeoutSeconds,
                MonitoringEnabled = c.MonitoringEnabled,
                CurrentStatus = c.CurrentStatus.ToString(),
                LastCheckAt = c.LastCheckAt,
                LastResponseTimeMs = c.LastResponseTimeMs,
                LastStatusCode = c.LastStatusCode,
                LastErrorMessage = c.LastErrorMessage,
                MonitorSsl = c.MonitorSsl,
                SslDaysRemaining = c.SslDaysRemaining,
                HostId = c.HostId,
                CustomerId = c.CustomerId
            })
            .ToListAsync();

        // Populate history for each check (N+1 query, acceptable for small lists)
        foreach (var check in checks)
        {
            check.History = await _context.CheckResults
                .Where(r => r.CheckId == check.Id)
                .OrderByDescending(r => r.CheckedAt)
                .Take(20)
                .Select(r => new CheckResultDto
                {
                    Id = r.Id,
                    Status = r.Status.ToString(),
                    ResponseTimeMs = r.ResponseTimeMs,
                    StatusCode = r.StatusCode,
                    ErrorMessage = r.ErrorMessage,
                    CheckedAt = r.CheckedAt
                })
                .ToListAsync();
        }

        return Ok(checks);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<CheckDto>> GetCheck(Guid id)
    {
        var c = await _context.Checks.FindAsync(id);

        if (c == null)
        {
            return NotFound();
        }

        return new CheckDto
        {
            Id = c.Id,
            Name = c.Name,
            CheckType = c.CheckType.ToString(),
            Target = c.Target,
            IntervalSeconds = c.IntervalSeconds,
            TimeoutSeconds = c.TimeoutSeconds,
            MonitoringEnabled = c.MonitoringEnabled,
            CurrentStatus = c.CurrentStatus.ToString(),
            LastCheckAt = c.LastCheckAt,
            LastResponseTimeMs = c.LastResponseTimeMs,
            LastStatusCode = c.LastStatusCode,
            LastErrorMessage = c.LastErrorMessage,
            MonitorSsl = c.MonitorSsl,
            SslDaysRemaining = c.SslDaysRemaining,
            HostId = c.HostId,
            CustomerId = c.CustomerId
        };
    }

    [HttpPost]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<ActionResult<CheckDto>> CreateCheck(CreateCheckRequest request)
    {
        var orgId = Guid.Parse(User.FindFirstValue("OrganizationId")!);

        var check = new Check
        {
            OrganizationId = orgId,
            Name = request.Name,
            CheckType = request.CheckType,
            Target = request.Target,
            IntervalSeconds = request.IntervalSeconds,
            TimeoutSeconds = request.TimeoutSeconds,
            MonitoringEnabled = request.MonitoringEnabled,
            
            // HTTP
            HttpMethod = request.HttpMethod,
            RequestBody = request.RequestBody,
            RequestHeaders = request.RequestHeaders != null ? JsonSerializer.Serialize(request.RequestHeaders) : null,
            ExpectedStatusCode = request.ExpectedStatusCode ?? 200,
            ExpectedKeyword = request.ExpectedKeyword,
            KeywordShouldExist = request.KeywordShouldExist,
            MonitorSsl = request.MonitorSsl,
            SslExpiryWarningDays = request.SslExpiryWarningDays,
            
            // TCP
            TcpPort = request.TcpPort,
            
            HostId = request.HostId,
            CustomerId = request.CustomerId,
            CurrentStatus = StatusType.Unknown
        };

        _context.Checks.Add(check);
        await _context.SaveChangesAsync();

        // TODO: Trigger schedule update

        return CreatedAtAction(nameof(GetCheck), new { id = check.Id }, new CheckDto
        {
            Id = check.Id,
            Name = check.Name,
            CheckType = check.CheckType.ToString(),
            Target = check.Target,
            IntervalSeconds = check.IntervalSeconds,
            TimeoutSeconds = check.TimeoutSeconds,
            MonitoringEnabled = check.MonitoringEnabled,
            CurrentStatus = check.CurrentStatus.ToString(),
            HostId = check.HostId,
            CustomerId = check.CustomerId
        });
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> UpdateCheck(Guid id, UpdateCheckRequest request)
    {
        var check = await _context.Checks.FindAsync(id);

        if (check == null)
        {
            return NotFound();
        }

        check.Name = request.Name;
        check.Target = request.Target;
        check.IntervalSeconds = request.IntervalSeconds;
        check.TimeoutSeconds = request.TimeoutSeconds;
        check.MonitoringEnabled = request.MonitoringEnabled;
        
        check.HttpMethod = request.HttpMethod;
        check.RequestBody = request.RequestBody;
        check.RequestHeaders = request.RequestHeaders != null ? JsonSerializer.Serialize(request.RequestHeaders) : null;
        check.ExpectedStatusCode = request.ExpectedStatusCode ?? check.ExpectedStatusCode;
        check.ExpectedKeyword = request.ExpectedKeyword;
        check.KeywordShouldExist = request.KeywordShouldExist;
        check.MonitorSsl = request.MonitorSsl;
        check.SslExpiryWarningDays = request.SslExpiryWarningDays;
        check.TcpPort = request.TcpPort;
        check.HostId = request.HostId;
        check.CustomerId = request.CustomerId;

        try
        {
            await _context.SaveChangesAsync();
            // TODO: Trigger schedule update
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!CheckExists(id))
            {
                return NotFound();
            }
            else
            {
                throw;
            }
        }

        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> DeleteCheck(Guid id)
    {
        var check = await _context.Checks.FindAsync(id);
        if (check == null)
        {
            return NotFound();
        }

        _context.Checks.Remove(check);
        await _context.SaveChangesAsync();
        // TODO: Trigger schedule update

        return NoContent();
    }

    [HttpGet("{id}/results")]
    public async Task<ActionResult<IEnumerable<CheckResultDto>>> GetCheckResults(Guid id, [FromQuery] int limit = 50)
    {
        var results = await _context.CheckResults
            .Where(r => r.CheckId == id)
            .OrderByDescending(r => r.CheckedAt)
            .Take(limit)
            .Select(r => new CheckResultDto
            {
                Id = r.Id,
                Status = r.Status.ToString(),
                ResponseTimeMs = r.ResponseTimeMs,
                StatusCode = r.StatusCode,
                ErrorMessage = r.ErrorMessage,
                CheckedAt = r.CheckedAt
            })
            .ToListAsync();

        return Ok(results);
    }

    private bool CheckExists(Guid id)
    {
        return _context.Checks.Any(e => e.Id == id);
    }
}

