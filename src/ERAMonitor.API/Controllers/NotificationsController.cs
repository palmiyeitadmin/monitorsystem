using ERAMonitor.Core.DTOs;
using ERAMonitor.Core.Enums;
using ERAMonitor.Core.Interfaces;
using ERAMonitor.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ERAMonitor.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class NotificationsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly INotificationService _notificationService;

    public NotificationsController(ApplicationDbContext context, INotificationService notificationService)
    {
        _context = context;
        _notificationService = notificationService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<NotificationDto>>> GetNotifications(
        [FromQuery] NotificationChannelType? channel,
        [FromQuery] NotificationStatus? status)
    {
        var query = _context.Notifications.AsQueryable();

        if (channel.HasValue)
            query = query.Where(n => n.Channel == channel.Value.ToString());

        if (status.HasValue)
            query = query.Where(n => n.Status == status);

        var notifications = await query
            .OrderByDescending(n => n.CreatedAt)
            .Select(n => new NotificationDto
            {
                Id = n.Id,
                Channel = n.Channel,
                Recipient = n.Recipient,
                Subject = n.Subject ?? string.Empty,
                Status = n.Status.ToString(),
                CreatedAt = n.CreatedAt,
                SentAt = n.SentAt,
                ErrorMessage = n.ErrorMessage,
                RetryCount = n.RetryCount
            })
            .ToListAsync();

        return Ok(notifications);
    }

    [HttpPost("{id}/resend")]
    public async Task<IActionResult> Resend(Guid id)
    {
        var notification = await _context.Notifications.FindAsync(id);
        if (notification == null) return NotFound();

        // Re-queue notification logic
        // For simplicity, we'll just call the service with a new request
        // Ideally we should have a Resend method in service to handle existing entity
        
        // Parse channel string back to enum
        if (!Enum.TryParse<NotificationChannelType>(notification.Channel, out var channelType))
        {
            return BadRequest("Invalid channel type");
        }
        
        await _notificationService.SendNotification(new NotificationRequest
        {
            OrganizationId = notification.OrganizationId,
            TriggerType = notification.TriggerType ?? string.Empty,
            TriggerId = notification.TriggerId,
            Channel = channelType,
            Recipient = notification.Recipient,
            Subject = notification.Subject ?? string.Empty,
            Content = notification.Content,
            ContentHtml = notification.ContentHtml
        });

        return Ok("Resent initiated");
    }
}
