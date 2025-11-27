using ERAMonitor.Core.Enums;

namespace ERAMonitor.Core.DTOs.Notifications;

public class NotificationRuleDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public NotificationEventType EventType { get; set; }
    public IncidentSeverity? MinimumSeverity { get; set; }
    public bool IsEnabled { get; set; }
    public int Priority { get; set; }
    
    public NotificationChannelSummaryDto Channel { get; set; } = null!;
    
    public int TotalNotificationsSent { get; set; }
    public DateTime? LastTriggeredAt { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class NotificationChannelSummaryDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public NotificationChannelType Type { get; set; }
}

public class NotificationRuleDetailDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public NotificationEventType EventType { get; set; }
    public IncidentSeverity? MinimumSeverity { get; set; }
    
    // Filters
    public List<Guid> CustomerIds { get; set; } = new();
    public List<Guid> HostIds { get; set; } = new();
    public List<Guid> CheckIds { get; set; } = new();
    public List<string> Tags { get; set; } = new();
    
    // Channel & Template
    public NotificationChannelSummaryDto Channel { get; set; } = null!;
    public NotificationTemplateSummaryDto? Template { get; set; }
    
    // Recipients
    public List<string> Recipients { get; set; } = new();
    
    // Timing
    public bool IsEnabled { get; set; }
    public int DelaySeconds { get; set; }
    public int? CooldownMinutes { get; set; }
    
    // Working hours
    public WorkingHoursDto? WorkingHours { get; set; }
    
    // Escalation
    public int? EscalateAfterMinutes { get; set; }
    public NotificationRuleSummaryDto? EscalateToRule { get; set; }
    
    public int Priority { get; set; }
    public int TotalNotificationsSent { get; set; }
    public DateTime? LastTriggeredAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class NotificationTemplateSummaryDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class NotificationRuleSummaryDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class WorkingHoursDto
{
    public bool Enabled { get; set; }
    public string? StartTime { get; set; }
    public string? EndTime { get; set; }
    public List<int> Days { get; set; } = new();
    public string Timezone { get; set; } = "Europe/Istanbul";
}

public class CreateNotificationRuleRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public NotificationEventType EventType { get; set; }
    public IncidentSeverity? MinimumSeverity { get; set; }
    
    public List<Guid>? CustomerIds { get; set; }
    public List<Guid>? HostIds { get; set; }
    public List<Guid>? CheckIds { get; set; }
    public List<string>? Tags { get; set; }
    
    public Guid ChannelId { get; set; }
    public Guid? TemplateId { get; set; }
    public List<string>? Recipients { get; set; }
    
    public bool IsEnabled { get; set; } = true;
    public int DelaySeconds { get; set; } = 0;
    public int? CooldownMinutes { get; set; }
    
    public bool OnlyDuringWorkingHours { get; set; } = false;
    public string? WorkingHoursStart { get; set; }
    public string? WorkingHoursEnd { get; set; }
    public string? WorkingDays { get; set; }
    public string? Timezone { get; set; }
    
    public int? EscalateAfterMinutes { get; set; }
    public Guid? EscalateToRuleId { get; set; }
    
    public int Priority { get; set; } = 0;
}

public class UpdateNotificationRuleRequest : CreateNotificationRuleRequest
{
}
