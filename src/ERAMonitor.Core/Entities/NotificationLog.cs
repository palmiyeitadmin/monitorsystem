using ERAMonitor.Core.Enums;

namespace ERAMonitor.Core.Entities;

public class NotificationLog : BaseEntity
{
    public Guid OrganizationId { get; set; }
    public Guid ChannelId { get; set; }
    public Guid? RuleId { get; set; }
    public Guid? TemplateId { get; set; }
    
    // Event source
    public NotificationEventType EventType { get; set; }
    public string? SourceType { get; set; } // Host, Check, Incident, Service
    public Guid? SourceId { get; set; }
    public string? SourceName { get; set; }
    
    // Recipient
    public string? Recipient { get; set; } // Email, phone, channel name
    
    // Content
    public string? Subject { get; set; }
    public string? Body { get; set; }
    
    // Delivery status
    public NotificationStatus Status { get; set; } = NotificationStatus.Pending;
    public DateTime? SentAt { get; set; }
    public DateTime? DeliveredAt { get; set; }
    public DateTime? FailedAt { get; set; }
    public string? ErrorMessage { get; set; }
    public int RetryCount { get; set; } = 0;
    public DateTime? NextRetryAt { get; set; }
    
    // External references
    public string? ExternalId { get; set; } // Message ID from provider
    
    // Tracking
    public bool IsRead { get; set; } = false;
    public DateTime? ReadAt { get; set; }
    public bool IsClicked { get; set; } = false;
    public DateTime? ClickedAt { get; set; }
    
    // Metadata
    public string? Metadata { get; set; } // Additional JSON data
    
    // Navigation
    public virtual NotificationChannel Channel { get; set; } = null!;
    public virtual NotificationRule? Rule { get; set; }
}
