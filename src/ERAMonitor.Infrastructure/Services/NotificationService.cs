using ERAMonitor.Core.DTOs;
using ERAMonitor.Core.Entities;
using ERAMonitor.Core.Enums;
using ERAMonitor.Core.Interfaces;
using ERAMonitor.Core.Interfaces.Services;
using ERAMonitor.Core.Interfaces.Repositories;
using ERAMonitor.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ERAMonitor.Infrastructure.Services;

public class NotificationService : INotificationService
{
    private readonly ApplicationDbContext _context;
    private readonly IEmailService _emailService;

    public NotificationService(ApplicationDbContext context, IEmailService emailService)
    {
        _context = context;
        _emailService = emailService;
    }

    public async Task SendNotification(NotificationRequest request)
    {
        var notification = new Notification
        {
            OrganizationId = request.OrganizationId,
            TriggerType = request.TriggerType,
            TriggerId = request.TriggerId,
            Channel = request.Channel.ToString(),
            Recipient = request.Recipient,
            Subject = request.Subject,
            Content = request.Content,
            ContentHtml = request.ContentHtml,
            Status = NotificationStatus.Pending
        };

        _context.Notifications.Add(notification);
        await _context.SaveChangesAsync();

        try
        {
            switch (request.Channel)
            {
                case NotificationChannelType.Email:
                    await _emailService.SendAsync(
                        request.Recipient,
                        request.Subject,
                        request.ContentHtml ?? request.Content
                    );
                    break;

                case NotificationChannelType.Telegram:
                    // Stub: Implement Telegram sending logic
                    // await _telegramService.SendMessageAsync(request.Recipient, request.Content);
                    break;

                case NotificationChannelType.Webhook:
                    // Stub: Implement Webhook sending logic
                    // await _webhookService.SendAsync(request.Recipient, ...);
                    break;
            }

            notification.Status = NotificationStatus.Sent;
            notification.SentAt = DateTime.UtcNow;
        }
        catch (Exception ex)
        {
            notification.Status = NotificationStatus.Failed;
            notification.ErrorMessage = ex.Message;
            notification.RetryCount++;
            notification.NextRetryAt = DateTime.UtcNow.AddMinutes(5);
        }

        await _context.SaveChangesAsync();
    }

    public async Task SendCheckDownNotification(Check check, CheckResult result)
    {
        // Logic to find recipients based on rules would go here.
        // For now, we'll just log it or send to a default if configured.
        // This requires implementing the Rule Engine which is part of NotificationService logic in requirements.
        
        // Simplified: Send to Organization Admin (if we had a way to get them easily without rules)
        // Or just stub it for now as "Ready to implement rules"
    }

    public async Task SendCheckUpNotification(Check check, CheckResult result)
    {
        // Stub
    }

    public async Task SendSslExpiryWarning(Check check, int daysRemaining)
    {
        // Stub
    }

    public async Task SendIncidentCreatedNotification(Incident incident)
    {
        // Stub: Find users to notify
    }

    public async Task SendIncidentResolvedNotification(Incident incident)
    {
        // Stub
    }

    // Phase 3 Methods
    public async Task SendHostDownNotificationAsync(Host host)
    {
        // Stub
        await Task.CompletedTask;
    }

    public async Task SendHostRecoveredNotificationAsync(Host host)
    {
        // Stub
        await Task.CompletedTask;
    }

    public async Task SendServiceDownNotificationAsync(Service service, Host host)
    {
        // Stub
        await Task.CompletedTask;
    }

    public async Task SendServiceRecoveredNotificationAsync(Service service, Host host)
    {
        // Stub
        await Task.CompletedTask;
    }
}
