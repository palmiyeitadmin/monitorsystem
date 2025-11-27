using ERAMonitor.Core.Enums;

namespace ERAMonitor.Core.DTOs.Notifications;

public class NotificationLogDto
{
    public Guid Id { get; set; }
    public NotificationEventType EventType { get; set; }
    public string? SourceType { get; set; }
    public string? SourceName { get; set; }
    public string? Recipient { get; set; }
    public string? Subject { get; set; }
    public NotificationStatus Status { get; set; }
    public string StatusDisplay => Status.ToString();
    public DateTime? SentAt { get; set; }
    public DateTime? DeliveredAt { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime CreatedAt { get; set; }
    
    public NotificationChannelSummaryDto Channel { get; set; } = null!;
}

public class NotificationLogDetailDto
{
    public Guid Id { get; set; }
    public NotificationEventType EventType { get; set; }
    public string? SourceType { get; set; }
    public Guid? SourceId { get; set; }
    public string? SourceName { get; set; }
    
    public string? Recipient { get; set; }
    public string? Subject { get; set; }
    public string? Body { get; set; }
    
    public NotificationStatus Status { get; set; }
    public DateTime? SentAt { get; set; }
    public DateTime? DeliveredAt { get; set; }
    public DateTime? FailedAt { get; set; }
    public string? ErrorMessage { get; set; }
    public int RetryCount { get; set; }
    public DateTime? NextRetryAt { get; set; }
    
    public string? ExternalId { get; set; }
    public bool IsRead { get; set; }
    public DateTime? ReadAt { get; set; }
    
    public NotificationChannelSummaryDto Channel { get; set; } = null!;
    public NotificationRuleSummaryDto? Rule { get; set; }
    
    public DateTime CreatedAt { get; set; }
}

public class NotificationStatsDto
{
    public int TotalSent { get; set; }
    public int TotalDelivered { get; set; }
    public int TotalFailed { get; set; }
    public int TotalPending { get; set; }
    public decimal DeliveryRate { get; set; }
    public Dictionary<string, int> ByChannel { get; set; } = new();
    public Dictionary<string, int> ByEventType { get; set; } = new();
    public List<NotificationTrendDataPoint> Trend { get; set; } = new();
}

public class NotificationTrendDataPoint
{
    public DateTime Date { get; set; }
    public int Sent { get; set; }
    public int Delivered { get; set; }
    public int Failed { get; set; }
}
