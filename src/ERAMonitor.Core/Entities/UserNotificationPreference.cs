using ERAMonitor.Core.Enums;

namespace ERAMonitor.Core.Entities;

public class UserNotificationPreference : BaseEntity
{
    public Guid UserId { get; set; }
    
    // Global preferences
    public bool EmailEnabled { get; set; } = true;
    public bool SmsEnabled { get; set; } = false;
    public bool PushEnabled { get; set; } = true;
    public bool SlackEnabled { get; set; } = true;
    public bool TelegramEnabled { get; set; } = false;
    
    // Quiet hours
    public bool QuietHoursEnabled { get; set; } = false;
    public string? QuietHoursStart { get; set; } = "22:00";
    public string? QuietHoursEnd { get; set; } = "08:00";
    public string Timezone { get; set; } = "Europe/Istanbul";
    public bool BypassQuietHoursForCritical { get; set; } = true;
    
    // Digest preferences
    public bool DailyDigestEnabled { get; set; } = false;
    public string? DailyDigestTime { get; set; } = "09:00";
    public bool WeeklyReportEnabled { get; set; } = true;
    public int WeeklyReportDay { get; set; } = 1; // Monday
    
    // Per-event type preferences (JSON)
    public string? EventPreferences { get; set; }
    // Format: {"HostDown": {"email": true, "sms": true}, "IncidentCreated": {"email": true}}
    
    // Contact info (override from user profile)
    public string? PreferredEmail { get; set; }
    public string? PreferredPhone { get; set; }
    public string? TelegramChatId { get; set; }
    public string? SlackUserId { get; set; }
    
    // Navigation
    public virtual User User { get; set; } = null!;
    
    // Helper methods
    public Dictionary<NotificationEventType, Dictionary<string, bool>> GetEventPreferences()
    {
        if (string.IsNullOrEmpty(EventPreferences))
            return new Dictionary<NotificationEventType, Dictionary<string, bool>>();
        
        return System.Text.Json.JsonSerializer.Deserialize<Dictionary<NotificationEventType, Dictionary<string, bool>>>(EventPreferences)
            ?? new Dictionary<NotificationEventType, Dictionary<string, bool>>();
    }
    
    public bool ShouldNotify(NotificationEventType eventType, string channel, IncidentSeverity? severity = null)
    {
        // Check quiet hours
        if (IsInQuietHours() && !(BypassQuietHoursForCritical && severity == IncidentSeverity.Critical))
            return false;
        
        // Check channel enabled
        var channelEnabled = channel.ToLower() switch
        {
            "email" => EmailEnabled,
            "sms" => SmsEnabled,
            "push" => PushEnabled,
            "slack" => SlackEnabled,
            "telegram" => TelegramEnabled,
            _ => true
        };
        
        if (!channelEnabled) return false;
        
        // Check event-specific preferences
        var prefs = GetEventPreferences();
        if (prefs.TryGetValue(eventType, out var eventPref))
        {
            if (eventPref.TryGetValue(channel.ToLower(), out var enabled))
                return enabled;
        }
        
        return true; // Default to allowing
    }
    
    public bool IsInQuietHours()
    {
        if (!QuietHoursEnabled) return false;
        
        try
        {
            var tz = TimeZoneInfo.FindSystemTimeZoneById(Timezone);
            var localTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tz);
            var currentTime = TimeOnly.FromDateTime(localTime);
            
            var start = TimeOnly.Parse(QuietHoursStart ?? "22:00");
            var end = TimeOnly.Parse(QuietHoursEnd ?? "08:00");
            
            // Handle overnight quiet hours
            if (start > end)
            {
                return currentTime >= start || currentTime <= end;
            }
            
            return currentTime >= start && currentTime <= end;
        }
        catch
        {
            return false;
        }
    }
}
