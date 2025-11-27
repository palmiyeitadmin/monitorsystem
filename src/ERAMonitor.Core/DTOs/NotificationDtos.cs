using ERAMonitor.Core.Enums;

namespace ERAMonitor.Core.DTOs;

public class NotificationDto
{
    public Guid Id { get; set; }
    public string Channel { get; set; } = string.Empty;
    public string Recipient { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? SentAt { get; set; }
    public string? ErrorMessage { get; set; }
    public int RetryCount { get; set; }
}

public class NotificationRequest
{
    public Guid OrganizationId { get; set; }
    public string TriggerType { get; set; } = string.Empty;
    public Guid? TriggerId { get; set; }
    public Guid? RuleId { get; set; }
    public NotificationChannelType Channel { get; set; }
    public string Recipient { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? ContentHtml { get; set; }
}

