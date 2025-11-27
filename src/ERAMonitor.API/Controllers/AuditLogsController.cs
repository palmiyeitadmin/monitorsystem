using System.Security.Claims;
using ERAMonitor.Core.DTOs;
using ERAMonitor.Core.Enums;
using ERAMonitor.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ERAMonitor.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class AuditLogsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public AuditLogsController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<AuditLogDto>>> GetAuditLogs(
        [FromQuery] string? entityType,
        [FromQuery] Guid? userId,
        [FromQuery] DateTime? dateFrom,
        [FromQuery] DateTime? dateTo,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var orgId = Guid.Parse(User.FindFirstValue("OrganizationId")!);
        var role = User.FindFirstValue(ClaimTypes.Role);

        if (role != nameof(UserRole.SuperAdmin) && role != nameof(UserRole.Admin))
        {
            return Forbid();
        }

        var query = _context.AuditLogs
            .Where(l => l.OrganizationId == orgId)
            .AsQueryable();

        if (!string.IsNullOrEmpty(entityType))
            query = query.Where(l => l.EntityType == entityType);

        if (userId.HasValue)
            query = query.Where(l => l.UserId == userId);

        if (dateFrom.HasValue)
            query = query.Where(l => l.CreatedAt >= dateFrom.Value);

        if (dateTo.HasValue)
            query = query.Where(l => l.CreatedAt <= dateTo.Value);

        var logs = await query
            .OrderByDescending(l => l.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(l => new AuditLogDto
            {
                Id = l.Id,
                Action = l.Action,
                EntityType = l.EntityType,
                EntityId = l.EntityId,
                EntityName = l.EntityName,
                UserName = l.User != null ? l.User.FullName : "System",
                IpAddress = l.IpAddress,
                CreatedAt = l.CreatedAt,
                OldValues = l.OldValues,
                NewValues = l.NewValues
            })
            .ToListAsync();

        return Ok(logs);
    }
}
