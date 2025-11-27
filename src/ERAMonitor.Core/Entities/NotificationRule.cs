using ERAMonitor.Core.Enums;

namespace ERAMonitor.Core.Entities;

public class NotificationRule : BaseEntityWithOrganization
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    
    // What triggers this rule
    public NotificationEventType EventType { get; set; }
    
    // Severity filter (null = all)
    public IncidentSeverity? MinimumSeverity { get; set; }
    
    // Source filters (JSON arrays)
    public string? CustomerIds { get; set; } // ["guid1", "guid2"] or null for all
    public string? HostIds { get; set; }
    public string? CheckIds { get; set; }
    public string? Tags { get; set; } // Match any tag
    
    // Channel to notify
    public Guid ChannelId { get; set; }
    public Guid? TemplateId { get; set; }
    
    // Recipients (for email/sms)
    public string? Recipients { get; set; } // JSON array of emails/phones
    
    // Timing
    public bool IsEnabled { get; set; } = true;
    public int DelaySeconds { get; set; } = 0; // Delay before sending
    public int? CooldownMinutes { get; set; } // Min time between notifications for same source
    
    // Working hours filter (optional)
    public bool OnlyDuringWorkingHours { get; set; } = false;
    public string? WorkingHoursStart { get; set; } // "09:00"
    public string? WorkingHoursEnd { get; set; } // "18:00"
    public string? WorkingDays { get; set; } // "1,2,3,4,5" (Monday-Friday)
    public string? Timezone { get; set; } = "Europe/Istanbul";
    
    // Escalation
    public int? EscalateAfterMinutes { get; set; }
    public Guid? EscalateToRuleId { get; set; }
    
    // Priority/Order
    public int Priority { get; set; } = 0;
    
    // Statistics
    public int TotalNotificationsSent { get; set; } = 0;
    public DateTime? LastTriggeredAt { get; set; }
    
    // Navigation
    public virtual NotificationChannel Channel { get; set; } = null!;
    public virtual NotificationTemplate? Template { get; set; }
    public virtual NotificationRule? EscalateToRule { get; set; }
    
    // Helper methods
    public List<Guid> GetCustomerIds()
    {
        if (string.IsNullOrEmpty(CustomerIds)) return new List<Guid>();
        return System.Text.Json.JsonSerializer.Deserialize<List<Guid>>(CustomerIds) ?? new List<Guid>();
    }
    
    public List<string> GetRecipients()
    {
        if (string.IsNullOrEmpty(Recipients)) return new List<string>();
        return System.Text.Json.JsonSerializer.Deserialize<List<string>>(Recipients) ?? new List<string>();
    }
    
    public bool MatchesSeverity(IncidentSeverity severity)
    {
        if (!MinimumSeverity.HasValue) return true;
        return severity <= MinimumSeverity.Value; // Critical < High < Medium < Low
    }
    
    public bool IsWithinWorkingHours(DateTime utcNow)
    {
        if (!OnlyDuringWorkingHours) return true;
        
        try
        {
            var tz = TimeZoneInfo.FindSystemTimeZoneById(Timezone ?? "Europe/Istanbul");
            var localTime = TimeZoneInfo.ConvertTimeFromUtc(utcNow, tz);
            
            // Check day
            if (!string.IsNullOrEmpty(WorkingDays))
            {
                var days = WorkingDays.Split(',').Select(int.Parse).ToList();
                if (!days.Contains((int)localTime.DayOfWeek))
                    return false;
            }
            
            // Check time
            if (!string.IsNullOrEmpty(WorkingHoursStart) && !string.IsNullOrEmpty(WorkingHoursEnd))
            {
                var start = TimeOnly.Parse(WorkingHoursStart);
                var end = TimeOnly.Parse(WorkingHoursEnd);
                var current = TimeOnly.FromDateTime(localTime);
                
                return current >= start && current <= end;
            }
            
            return true;
        }
        catch
        {
            return true; // Default to allowing if timezone parse fails
        }
    }
}
