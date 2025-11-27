using ERAMonitor.Core.Enums;

namespace ERAMonitor.Core.Entities;

public class Notification : BaseEntity
{
    public Guid OrganizationId { get; set; }
    public Organization Organization { get; set; } = null!;
    
    public string? TriggerType { get; set; }
    public Guid? TriggerId { get; set; }
    public Guid? RuleId { get; set; }
    
    public string Channel { get; set; } = string.Empty;
    public string Recipient { get; set; } = string.Empty;
    public string? Subject { get; set; }
    public string Content { get; set; } = string.Empty;
    public string? ContentHtml { get; set; }
    
    public NotificationStatus Status { get; set; } = NotificationStatus.Pending;
    public DateTime? SentAt { get; set; }
    public DateTime? DeliveredAt { get; set; }
    public string? ErrorMessage { get; set; }
    public int RetryCount { get; set; } = 0;
    public DateTime? NextRetryAt { get; set; }
    
    public string? ExternalMessageId { get; set; }
    public string? Metadata { get; set; } // JSONB
}
