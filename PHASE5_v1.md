PHASE 5: Notifications & Alerting System (Days 19-24)
5.1 Overview
Phase 5 focuses on implementing:

Multi-channel notification system (Email, SMS, Webhook, Telegram, Slack)
Notification templates with variable substitution
Notification rules and escalation policies
Alert grouping and deduplication
On-call schedules and rotation
Notification history and delivery tracking
User notification preferences


5.2 Entities
NotificationChannel Entity
csharp// src/ERAMonitor.Core/Entities/NotificationChannel.cs

using ERAMonitor.Core.Enums;

namespace ERAMonitor.Core.Entities;

public class NotificationChannel : BaseEntityWithOrganization
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public NotificationChannelType Type { get; set; }
    
    // Channel-specific configuration (stored as JSON)
    public string Configuration { get; set; } = "{}";
    
    // Status
    public bool IsEnabled { get; set; } = true;
    public bool IsVerified { get; set; } = false;
    public DateTime? VerifiedAt { get; set; }
    public DateTime? LastUsedAt { get; set; }
    public DateTime? LastFailedAt { get; set; }
    public string? LastError { get; set; }
    
    // Rate limiting
    public int RateLimitPerHour { get; set; } = 100;
    public int CurrentHourCount { get; set; } = 0;
    public DateTime? RateLimitResetAt { get; set; }
    
    // Default channel for organization
    public bool IsDefault { get; set; } = false;
    
    // Navigation
    public virtual ICollection<NotificationRule> Rules { get; set; } = new List<NotificationRule>();
    public virtual ICollection<NotificationLog> Logs { get; set; } = new List<NotificationLog>();
    
    // Helper method to get typed configuration
    public T GetConfiguration<T>() where T : class, new()
    {
        if (string.IsNullOrEmpty(Configuration))
            return new T();
        
        return System.Text.Json.JsonSerializer.Deserialize<T>(Configuration) ?? new T();
    }
    
    public void SetConfiguration<T>(T config) where T : class
    {
        Configuration = System.Text.Json.JsonSerializer.Serialize(config);
    }
}
Channel Configuration Classes
csharp// src/ERAMonitor.Core/Entities/Configurations/ChannelConfigurations.cs

namespace ERAMonitor.Core.Entities.Configurations;

public class EmailChannelConfig
{
    public string SmtpHost { get; set; } = string.Empty;
    public int SmtpPort { get; set; } = 587;
    public bool UseSsl { get; set; } = true;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FromAddress { get; set; } = string.Empty;
    public string FromName { get; set; } = string.Empty;
    public string? ReplyToAddress { get; set; }
}

public class SmsChannelConfig
{
    public string Provider { get; set; } = "Twilio"; // Twilio, Nexmo, etc.
    public string AccountSid { get; set; } = string.Empty;
    public string AuthToken { get; set; } = string.Empty;
    public string FromNumber { get; set; } = string.Empty;
}

public class WebhookChannelConfig
{
    public string Url { get; set; } = string.Empty;
    public string Method { get; set; } = "POST";
    public Dictionary<string, string> Headers { get; set; } = new();
    public string? SecretKey { get; set; } // For HMAC signature
    public bool IncludeSignature { get; set; } = true;
    public int TimeoutSeconds { get; set; } = 30;
    public int RetryCount { get; set; } = 3;
}

public class TelegramChannelConfig
{
    public string BotToken { get; set; } = string.Empty;
    public string ChatId { get; set; } = string.Empty;
    public bool ParseMode { get; set; } = true; // Use Markdown
    public bool DisableNotification { get; set; } = false;
}

public class SlackChannelConfig
{
    public string WebhookUrl { get; set; } = string.Empty;
    public string Channel { get; set; } = string.Empty;
    public string Username { get; set; } = "ERA Monitor";
    public string IconEmoji { get; set; } = ":warning:";
    public bool UseBlocks { get; set; } = true;
}

public class MsTeamsChannelConfig
{
    public string WebhookUrl { get; set; } = string.Empty;
    public string ThemeColor { get; set; } = "29ABE2"; // ERA brand color
}

public class PushoverChannelConfig
{
    public string ApiToken { get; set; } = string.Empty;
    public string UserKey { get; set; } = string.Empty;
    public string? Device { get; set; }
    public int Priority { get; set; } = 0; // -2 to 2
    public string? Sound { get; set; }
}
NotificationTemplate Entity
csharp// src/ERAMonitor.Core/Entities/NotificationTemplate.cs

using ERAMonitor.Core.Enums;

namespace ERAMonitor.Core.Entities;

public class NotificationTemplate : BaseEntityWithOrganization
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    
    // Template type
    public NotificationEventType EventType { get; set; }
    public NotificationChannelType ChannelType { get; set; }
    
    // Content templates (support variables like {{host_name}}, {{status}})
    public string Subject { get; set; } = string.Empty; // For email
    public string Body { get; set; } = string.Empty;
    public string? HtmlBody { get; set; } // For email HTML version
    
    // Formatting
    public string? Format { get; set; } // json, markdown, plain
    
    // Status
    public bool IsActive { get; set; } = true;
    public bool IsDefault { get; set; } = false;
    
    // Metadata
    public string? Variables { get; set; } // JSON list of available variables
    
    // Helper to get variable list
    public List<string> GetVariables()
    {
        if (string.IsNullOrEmpty(Variables))
            return new List<string>();
        
        return System.Text.Json.JsonSerializer.Deserialize<List<string>>(Variables) ?? new List<string>();
    }
}
NotificationRule Entity
csharp// src/ERAMonitor.Core/Entities/NotificationRule.cs

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
NotificationLog Entity
csharp// src/ERAMonitor.Core/Entities/NotificationLog.cs

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
ContactGroup Entity
csharp// src/ERAMonitor.Core/Entities/ContactGroup.cs

namespace ERAMonitor.Core.Entities;

public class ContactGroup : BaseEntityWithOrganization
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    
    // Members
    public virtual ICollection<ContactGroupMember> Members { get; set; } = new List<ContactGroupMember>();
    
    // Settings
    public bool IsActive { get; set; } = true;
}

public class ContactGroupMember : BaseEntity
{
    public Guid ContactGroupId { get; set; }
    
    // Either a user or external contact
    public Guid? UserId { get; set; }
    public string? ExternalEmail { get; set; }
    public string? ExternalPhone { get; set; }
    public string? ExternalName { get; set; }
    
    // Preferences for this member
    public bool ReceiveEmail { get; set; } = true;
    public bool ReceiveSms { get; set; } = false;
    public bool ReceivePush { get; set; } = true;
    
    // Navigation
    public virtual ContactGroup ContactGroup { get; set; } = null!;
    public virtual User? User { get; set; }
}
OnCallSchedule Entity
csharp// src/ERAMonitor.Core/Entities/OnCallSchedule.cs

namespace ERAMonitor.Core.Entities;

public class OnCallSchedule : BaseEntityWithOrganization
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Timezone { get; set; } = "Europe/Istanbul";
    
    // Rotation settings
    public OnCallRotationType RotationType { get; set; } = OnCallRotationType.Weekly;
    public int RotationLength { get; set; } = 1; // 1 week, 1 day, etc.
    public DateTime RotationStartDate { get; set; }
    public string? RotationStartTime { get; set; } = "09:00"; // When rotation changes
    
    // Current state
    public Guid? CurrentOnCallUserId { get; set; }
    public DateTime? CurrentRotationStart { get; set; }
    public DateTime? CurrentRotationEnd { get; set; }
    
    // Members in rotation order
    public virtual ICollection<OnCallScheduleMember> Members { get; set; } = new List<OnCallScheduleMember>();
    public virtual ICollection<OnCallOverride> Overrides { get; set; } = new List<OnCallOverride>();
    
    // Settings
    public bool IsActive { get; set; } = true;
    public bool NotifyOnRotation { get; set; } = true;
    
    // Navigation
    public virtual User? CurrentOnCallUser { get; set; }
}

public enum OnCallRotationType
{
    Daily,
    Weekly,
    BiWeekly,
    Monthly,
    Custom
}

public class OnCallScheduleMember : BaseEntity
{
    public Guid OnCallScheduleId { get; set; }
    public Guid UserId { get; set; }
    public int Order { get; set; } // Position in rotation
    
    // Navigation
    public virtual OnCallSchedule OnCallSchedule { get; set; } = null!;
    public virtual User User { get; set; } = null!;
}

public class OnCallOverride : BaseEntity
{
    public Guid OnCallScheduleId { get; set; }
    public Guid UserId { get; set; }
    
    public DateTime StartAt { get; set; }
    public DateTime EndAt { get; set; }
    public string? Reason { get; set; }
    
    // Who created the override
    public Guid CreatedByUserId { get; set; }
    
    // Navigation
    public virtual OnCallSchedule OnCallSchedule { get; set; } = null!;
    public virtual User User { get; set; } = null!;
    public virtual User CreatedByUser { get; set; } = null!;
}
UserNotificationPreference Entity
csharp// src/ERAMonitor.Core/Entities/UserNotificationPreference.cs

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

5.3 Enums
csharp// src/ERAMonitor.Core/Enums/NotificationEnums.cs

namespace ERAMonitor.Core.Enums;

public enum NotificationChannelType
{
    Email,
    Sms,
    Webhook,
    Telegram,
    Slack,
    MsTeams,
    Pushover,
    PushNotification
}

public enum NotificationEventType
{
    // Host events
    HostDown,
    HostUp,
    HostWarning,
    HostHighCpu,
    HostHighRam,
    HostHighDisk,
    HostMaintenanceStarted,
    HostMaintenanceEnded,
    
    // Service events
    ServiceDown,
    ServiceUp,
    ServiceWarning,
    
    // Check events
    CheckFailed,
    CheckRecovered,
    CheckSlowResponse,
    SslCertificateExpiring,
    SslCertificateExpired,
    
    // Incident events
    IncidentCreated,
    IncidentAcknowledged,
    IncidentAssigned,
    IncidentEscalated,
    IncidentResolved,
    IncidentClosed,
    IncidentReopened,
    IncidentCommentAdded,
    SlaResponseBreached,
    SlaResolutionBreached,
    
    // On-call events
    OnCallRotationChanged,
    OnCallOverrideCreated,
    
    // System events
    DailyDigest,
    WeeklyReport,
    SystemAlert,
    TestNotification
}

public enum NotificationStatus
{
    Pending,
    Queued,
    Sending,
    Sent,
    Delivered,
    Failed,
    Bounced,
    Cancelled
}

public enum NotificationPriority
{
    Low,
    Normal,
    High,
    Urgent
}

5.4 DTOs
Channel DTOs
csharp// src/ERAMonitor.Core/DTOs/Notifications/ChannelDtos.cs

using ERAMonitor.Core.Enums;

namespace ERAMonitor.Core.DTOs.Notifications;

public class NotificationChannelDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public NotificationChannelType Type { get; set; }
    public string TypeDisplay => Type.ToString();
    public bool IsEnabled { get; set; }
    public bool IsVerified { get; set; }
    public bool IsDefault { get; set; }
    public DateTime? LastUsedAt { get; set; }
    public DateTime? LastFailedAt { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class NotificationChannelDetailDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public NotificationChannelType Type { get; set; }
    
    // Configuration (sensitive fields masked)
    public object? Configuration { get; set; }
    
    public bool IsEnabled { get; set; }
    public bool IsVerified { get; set; }
    public DateTime? VerifiedAt { get; set; }
    public bool IsDefault { get; set; }
    
    public int RateLimitPerHour { get; set; }
    public int CurrentHourCount { get; set; }
    
    public DateTime? LastUsedAt { get; set; }
    public DateTime? LastFailedAt { get; set; }
    public string? LastError { get; set; }
    
    public int RuleCount { get; set; }
    public int NotificationsSent24h { get; set; }
    
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class CreateNotificationChannelRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public NotificationChannelType Type { get; set; }
    public object Configuration { get; set; } = new();
    public bool IsEnabled { get; set; } = true;
    public bool IsDefault { get; set; } = false;
    public int RateLimitPerHour { get; set; } = 100;
}

public class UpdateNotificationChannelRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public object Configuration { get; set; } = new();
    public bool IsEnabled { get; set; } = true;
    public bool IsDefault { get; set; } = false;
    public int RateLimitPerHour { get; set; } = 100;
}

public class TestChannelRequest
{
    public string? TestRecipient { get; set; }
    public string? TestMessage { get; set; }
}
Template DTOs
csharp// src/ERAMonitor.Core/DTOs/Notifications/TemplateDtos.cs

using ERAMonitor.Core.Enums;

namespace ERAMonitor.Core.DTOs.Notifications;

public class NotificationTemplateDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public NotificationEventType EventType { get; set; }
    public NotificationChannelType ChannelType { get; set; }
    public bool IsActive { get; set; }
    public bool IsDefault { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class NotificationTemplateDetailDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public NotificationEventType EventType { get; set; }
    public NotificationChannelType ChannelType { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string? HtmlBody { get; set; }
    public string? Format { get; set; }
    public bool IsActive { get; set; }
    public bool IsDefault { get; set; }
    public List<string> AvailableVariables { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class CreateNotificationTemplateRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public NotificationEventType EventType { get; set; }
    public NotificationChannelType ChannelType { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string? HtmlBody { get; set; }
    public string? Format { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsDefault { get; set; } = false;
}

public class UpdateNotificationTemplateRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string? HtmlBody { get; set; }
    public string? Format { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsDefault { get; set; } = false;
}

public class PreviewTemplateRequest
{
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string? HtmlBody { get; set; }
    public Dictionary<string, string> SampleData { get; set; } = new();
}

public class PreviewTemplateResponse
{
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string? HtmlBody { get; set; }
}
Rule DTOs
csharp// src/ERAMonitor.Core/DTOs/Notifications/RuleDtos.cs

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
Log DTOs
csharp// src/ERAMonitor.Core/DTOs/Notifications/LogDtos.cs

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

5.5 Service Interfaces
csharp// src/ERAMonitor.Core/Interfaces/Services/INotificationService.cs

using ERAMonitor.Core.DTOs.Notifications;
using ERAMonitor.Core.Entities;
using ERAMonitor.Core.Enums;

namespace ERAMonitor.Core.Interfaces.Services;

public interface INotificationService
{
    // Send notifications
    Task SendNotificationAsync(NotificationRequest request);
    Task SendBulkNotificationsAsync(List<NotificationRequest> requests);
    
    // Event-triggered notifications
    Task ProcessEventAsync(NotificationEventType eventType, NotificationEventData data);
    
    // Convenience methods for specific events
    Task SendHostDownNotificationAsync(Host host);
    Task SendHostUpNotificationAsync(Host host);
    Task SendServiceDownNotificationAsync(Service service);
    Task SendServiceUpNotificationAsync(Service service);
    Task SendCheckFailedNotificationAsync(Check check, CheckResult result);
    Task SendCheckRecoveredNotificationAsync(Check check);
    Task SendIncidentCreatedNotificationAsync(Incident incident);
    Task SendIncidentAssignedNotificationAsync(Incident incident, User assignee);
    Task SendIncidentResolvedNotificationAsync(Incident incident);
    Task SendIncidentEscalatedNotificationAsync(Incident incident, string reason);
    Task SendSlaBreachNotificationAsync(Incident incident, string breachType);
    Task SendSslExpiryNotificationAsync(Check check, int daysUntilExpiry);
    
    // Test notifications
    Task<bool> SendTestNotificationAsync(Guid channelId, Guid organizationId, string? recipient = null);
    
    // Retry failed notifications
    Task RetryFailedNotificationsAsync();
    
    // On-call notifications
    Task<User?> GetCurrentOnCallUserAsync(Guid scheduleId);
    Task SendOnCallNotificationAsync(Guid scheduleId, NotificationRequest request);
}

public class NotificationRequest
{
    public Guid OrganizationId { get; set; }
    public NotificationEventType EventType { get; set; }
    public NotificationPriority Priority { get; set; } = NotificationPriority.Normal;
    
    // Source info
    public string? SourceType { get; set; }
    public Guid? SourceId { get; set; }
    public string? SourceName { get; set; }
    public Guid? CustomerId { get; set; }
    
    // Content
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string? HtmlBody { get; set; }
    
    // Template variables
    public Dictionary<string, string> Variables { get; set; } = new();
    
    // Targeting
    public Guid? ChannelId { get; set; }
    public List<string>? Recipients { get; set; }
    public List<Guid>? UserIds { get; set; }
    public List<Guid>? ContactGroupIds { get; set; }
    public Guid? OnCallScheduleId { get; set; }
    
    // Options
    public int DelaySeconds { get; set; } = 0;
    public bool BypassRules { get; set; } = false;
    public bool BypassQuietHours { get; set; } = false;
}

public class NotificationEventData
{
    public Guid OrganizationId { get; set; }
    public Guid? CustomerId { get; set; }
    public string SourceType { get; set; } = string.Empty;
    public Guid SourceId { get; set; }
    public string SourceName { get; set; } = string.Empty;
    public IncidentSeverity? Severity { get; set; }
    public Dictionary<string, string> Data { get; set; } = new();
    public List<string>? Tags { get; set; }
}
csharp// src/ERAMonitor.Core/Interfaces/Services/INotificationChannelService.cs

using ERAMonitor.Core.DTOs.Common;
using ERAMonitor.Core.DTOs.Notifications;
using ERAMonitor.Core.Enums;

namespace ERAMonitor.Core.Interfaces.Services;

public interface INotificationChannelService
{
    Task<PagedResponse<NotificationChannelDto>> GetPagedAsync(
        Guid organizationId,
        PagedRequest request,
        NotificationChannelType? type = null);
    
    Task<NotificationChannelDetailDto> GetByIdAsync(Guid id, Guid organizationId);
    Task<NotificationChannelDetailDto> CreateAsync(Guid organizationId, CreateNotificationChannelRequest request);
    Task<NotificationChannelDetailDto> UpdateAsync(Guid id, Guid organizationId, UpdateNotificationChannelRequest request);
    Task DeleteAsync(Guid id, Guid organizationId);
    
    Task<bool> TestChannelAsync(Guid id, Guid organizationId, TestChannelRequest request);
    Task<bool> VerifyChannelAsync(Guid id, Guid organizationId);
    Task SetDefaultAsync(Guid id, Guid organizationId);
}
csharp// src/ERAMonitor.Core/Interfaces/Services/INotificationTemplateService.cs

using ERAMonitor.Core.DTOs.Common;
using ERAMonitor.Core.DTOs.Notifications;
using ERAMonitor.Core.Enums;

namespace ERAMonitor.Core.Interfaces.Services;

public interface INotificationTemplateService
{
    Task<PagedResponse<NotificationTemplateDto>> GetPagedAsync(
        Guid organizationId,
        PagedRequest request,
        NotificationEventType? eventType = null,
        NotificationChannelType? channelType = null);
    
    Task<NotificationTemplateDetailDto> GetByIdAsync(Guid id, Guid organizationId);
    Task<NotificationTemplateDetailDto> CreateAsync(Guid organizationId, CreateNotificationTemplateRequest request);
    Task<NotificationTemplateDetailDto> UpdateAsync(Guid id, Guid organizationId, UpdateNotificationTemplateRequest request);
    Task DeleteAsync(Guid id, Guid organizationId);
    
    Task<PreviewTemplateResponse> PreviewAsync(PreviewTemplateRequest request);
    Task<List<string>> GetAvailableVariablesAsync(NotificationEventType eventType);
    
    Task<NotificationTemplate?> GetDefaultTemplateAsync(
        Guid organizationId,
        NotificationEventType eventType,
        NotificationChannelType channelType);
}
csharp// src/ERAMonitor.Core/Interfaces/Services/INotificationRuleService.cs

using ERAMonitor.Core.DTOs.Common;
using ERAMonitor.Core.DTOs.Notifications;
using ERAMonitor.Core.Enums;

namespace ERAMonitor.Core.Interfaces.Services;

public interface INotificationRuleService
{
    Task<PagedResponse<NotificationRuleDto>> GetPagedAsync(
        Guid organizationId,
        PagedRequest request,
        NotificationEventType? eventType = null,
        bool? isEnabled = null);
    
    Task<NotificationRuleDetailDto> GetByIdAsync(Guid id, Guid organizationId);
    Task<NotificationRuleDetailDto> CreateAsync(Guid organizationId, CreateNotificationRuleRequest request);
    Task<NotificationRuleDetailDto> UpdateAsync(Guid id, Guid organizationId, UpdateNotificationRuleRequest request);
    Task DeleteAsync(Guid id, Guid organizationId);
    
    Task<bool> ToggleAsync(Guid id, Guid organizationId);
    Task ReorderAsync(Guid organizationId, List<Guid> ruleIds);
    
    Task<List<NotificationRule>> GetMatchingRulesAsync(
        Guid organizationId,
        NotificationEventType eventType,
        NotificationEventData data);
}

5.6 Channel Providers
Base Provider Interface
csharp// src/ERAMonitor.Infrastructure/Notifications/Providers/INotificationProvider.cs

using ERAMonitor.Core.Entities;
using ERAMonitor.Core.Enums;

namespace ERAMonitor.Infrastructure.Notifications.Providers;

public interface INotificationProvider
{
    NotificationChannelType ChannelType { get; }
    
    Task<NotificationDeliveryResult> SendAsync(
        NotificationChannel channel,
        string recipient,
        string subject,
        string body,
        string? htmlBody = null,
        Dictionary<string, string>? metadata = null);
    
    Task<bool> ValidateConfigurationAsync(NotificationChannel channel);
    Task<bool> TestConnectionAsync(NotificationChannel channel, string? testRecipient = null);
}

public class NotificationDeliveryResult
{
    public bool Success { get; set; }
    public string? ExternalId { get; set; }
    public string? ErrorMessage { get; set; }
    public string? ErrorCode { get; set; }
    public bool ShouldRetry { get; set; }
    public int? RetryAfterSeconds { get; set; }
}
Email Provider
csharp// src/ERAMonitor.Infrastructure/Notifications/Providers/EmailNotificationProvider.cs

using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Logging;
using ERAMonitor.Core.Entities;
using ERAMonitor.Core.Entities.Configurations;
using ERAMonitor.Core.Enums;

namespace ERAMonitor.Infrastructure.Notifications.Providers;

public class EmailNotificationProvider : INotificationProvider
{
    private readonly ILogger<EmailNotificationProvider> _logger;
    
    public NotificationChannelType ChannelType => NotificationChannelType.Email;
    
    public EmailNotificationProvider(ILogger<EmailNotificationProvider> logger)
    {
        _logger = logger;
    }
    
    public async Task<NotificationDeliveryResult> SendAsync(
        NotificationChannel channel,
        string recipient,
        string subject,
        string body,
        string? htmlBody = null,
        Dictionary<string, string>? metadata = null)
    {
        try
        {
            var config = channel.GetConfiguration<EmailChannelConfig>();
            
            using var client = new SmtpClient(config.SmtpHost, config.SmtpPort)
            {
                Credentials = new NetworkCredential(config.Username, config.Password),
                EnableSsl = config.UseSsl
            };
            
            var message = new MailMessage
            {
                From = new MailAddress(config.FromAddress, config.FromName),
                Subject = subject,
                IsBodyHtml = !string.IsNullOrEmpty(htmlBody)
            };
            
            message.To.Add(recipient);
            
            if (!string.IsNullOrEmpty(htmlBody))
            {
                message.Body = htmlBody;
                
                // Add plain text alternative
                var plainView = AlternateView.CreateAlternateViewFromString(body, null, "text/plain");
                var htmlView = AlternateView.CreateAlternateViewFromString(htmlBody, null, "text/html");
                message.AlternateViews.Add(plainView);
                message.AlternateViews.Add(htmlView);
            }
            else
            {
                message.Body = body;
            }
            
            if (!string.IsNullOrEmpty(config.ReplyToAddress))
            {
                message.ReplyToList.Add(config.ReplyToAddress);
            }
            
            await client.SendMailAsync(message);
            
            _logger.LogDebug("Email sent to {Recipient}", recipient);
            
            return new NotificationDeliveryResult
            {
                Success = true,
                ExternalId = Guid.NewGuid().ToString() // Generate local ID
            };
        }
        catch (SmtpException ex)
        {
            _logger.LogError(ex, "SMTP error sending email to {Recipient}", recipient);
            
            return new NotificationDeliveryResult
            {
                Success = false,
                ErrorMessage = ex.Message,
                ErrorCode = ex.StatusCode.ToString(),
                ShouldRetry = ex.StatusCode == SmtpStatusCode.ServiceNotAvailable ||
                              ex.StatusCode == SmtpStatusCode.MailboxBusy
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending email to {Recipient}", recipient);
            
            return new NotificationDeliveryResult
            {
                Success = false,
                ErrorMessage = ex.Message,
                ShouldRetry = true
            };
        }
    }
    
    public async Task<bool> ValidateConfigurationAsync(NotificationChannel channel)
    {
        var config = channel.GetConfiguration<EmailChannelConfig>();
        
        return !string.IsNullOrEmpty(config.SmtpHost) &&
               config.SmtpPort > 0 &&
               !string.IsNullOrEmpty(config.FromAddress);
    }
    
    public async Task<bool> TestConnectionAsync(NotificationChannel channel, string? testRecipient = null)
    {
        try
        {
            var config = channel.GetConfiguration<EmailChannelConfig>();
            
            using var client = new SmtpClient(config.SmtpHost, config.SmtpPort)
            {
                Credentials = new NetworkCredential(config.Username, config.Password),
                EnableSsl = config.UseSsl,
                Timeout = 10000
            };
            
            // Test SMTP connection
            var message = new MailMessage
            {
                From = new MailAddress(config.FromAddress, config.FromName),
                Subject = "ERA Monitor - Test Notification",
                Body = "This is a test notification from ERA Monitor.",
                IsBodyHtml = false
            };
            
            var recipient = testRecipient ?? config.FromAddress;
            message.To.Add(recipient);
            
            await client.SendMailAsync(message);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Email channel test failed");
            return false;
        }
    }
}
Telegram Provider
csharp// src/ERAMonitor.Infrastructure/Notifications/Providers/TelegramNotificationProvider.cs

using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using ERAMonitor.Core.Entities;
using ERAMonitor.Core.Entities.Configurations;
using ERAMonitor.Core.Enums;

namespace ERAMonitor.Infrastructure.Notifications.Providers;

public class TelegramNotificationProvider : INotificationProvider
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<TelegramNotificationProvider> _logger;
    
    public NotificationChannelType ChannelType => NotificationChannelType.Telegram;
    
    public TelegramNotificationProvider(
        IHttpClientFactory httpClientFactory,
        ILogger<TelegramNotificationProvider> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }
    
    public async Task<NotificationDeliveryResult> SendAsync(
        NotificationChannel channel,
        string recipient,
        string subject,
        string body,
        string? htmlBody = null,
        Dictionary<string, string>? metadata = null)
    {
        try
        {
            var config = channel.GetConfiguration<TelegramChannelConfig>();
            var client = _httpClientFactory.CreateClient();
            
            var chatId = !string.IsNullOrEmpty(recipient) ? recipient : config.ChatId;
            
            // Format message with subject and body
            var message = $"*{EscapeMarkdown(subject)}*\n\n{EscapeMarkdown(body)}";
            
            var url = $"https://api.telegram.org/bot{config.BotToken}/sendMessage";
            
            var payload = new
            {
                chat_id = chatId,
                text = message,
                parse_mode = config.ParseMode ? "Markdown" : null,
                disable_notification = config.DisableNotification
            };
            
            var response = await client.PostAsJsonAsync(url, payload);
            
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<TelegramResponse>();
                
                return new NotificationDeliveryResult
                {
                    Success = true,
                    ExternalId = result?.Result?.MessageId.ToString()
                };
            }
            
            var error = await response.Content.ReadAsStringAsync();
            _logger.LogError("Telegram API error: {Error}", error);
            
            return new NotificationDeliveryResult
            {
                Success = false,
                ErrorMessage = error,
                ShouldRetry = (int)response.StatusCode >= 500
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending Telegram message");
            
            return new NotificationDeliveryResult
            {
                Success = false,
                ErrorMessage = ex.Message,
                ShouldRetry = true
            };
        }
    }
    
    public async Task<bool> ValidateConfigurationAsync(NotificationChannel channel)
    {
        var config = channel.GetConfiguration<TelegramChannelConfig>();
        return !string.IsNullOrEmpty(config.BotToken) && !string.IsNullOrEmpty(config.ChatId);
    }
    
    public async Task<bool> TestConnectionAsync(NotificationChannel channel, string? testRecipient = null)
    {
        var result = await SendAsync(
            channel,
            testRecipient ?? string.Empty,
            "🔔 ERA Monitor Test",
            "This is a test notification from ERA Monitor. If you received this, your Telegram integration is working correctly!"
        );
        
        return result.Success;
    }
    
    private static string EscapeMarkdown(string text)
    {
        // Escape Markdown special characters
        return text
            .Replace("_", "\\_")
            .Replace("*", "\\*")
            .Replace("[", "\\[")
            .Replace("`", "\\`");
    }
    
    private class TelegramResponse
    {
        public bool Ok { get; set; }
        public TelegramMessage? Result { get; set; }
    }
    
    private class TelegramMessage
    {
        public long MessageId { get; set; }
    }
}
Slack Provider
csharp// src/ERAMonitor.Infrastructure/Notifications/Providers/SlackNotificationProvider.cs

using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using ERAMonitor.Core.Entities;
using ERAMonitor.Core.Entities.Configurations;
using ERAMonitor.Core.Enums;

namespace ERAMonitor.Infrastructure.Notifications.Providers;

public class SlackNotificationProvider : INotificationProvider
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<SlackNotificationProvider> _logger;
    
    public NotificationChannelType ChannelType => NotificationChannelType.Slack;
    
    public SlackNotificationProvider(
        IHttpClientFactory httpClientFactory,
        ILogger<SlackNotificationProvider> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }
    
    public async Task<NotificationDeliveryResult> SendAsync(
        NotificationChannel channel,
        string recipient,
        string subject,
        string body,
        string? htmlBody = null,
        Dictionary<string, string>? metadata = null)
    {
        try
        {
            var config = channel.GetConfiguration<SlackChannelConfig>();
            var client = _httpClientFactory.CreateClient();
            
            object payload;
            
            if (config.UseBlocks)
            {
                // Rich formatting with blocks
                payload = new
                {
                    channel = !string.IsNullOrEmpty(recipient) ? recipient : config.Channel,
                    username = config.Username,
                    icon_emoji = config.IconEmoji,
                    blocks = new object[]
                    {
                        new
                        {
                            type = "header",
                            text = new { type = "plain_text", text = subject }
                        },
                        new
                        {
                            type = "section",
                            text = new { type = "mrkdwn", text = body }
                        },
                        new
                        {
                            type = "context",
                            elements = new object[]
                            {
                                new { type = "mrkdwn", text = $"Sent by ERA Monitor at {DateTime.UtcNow:yyyy-MM-dd HH:mm} UTC" }
                            }
                        }
                    }
                };
            }
            else
            {
                // Simple message
                payload = new
                {
                    channel = !string.IsNullOrEmpty(recipient) ? recipient : config.Channel,
                    username = config.Username,
                    icon_emoji = config.IconEmoji,
                    text = $"*{subject}*\n{body}"
                };
            }
            
            var response = await client.PostAsJsonAsync(config.WebhookUrl, payload);
            
            if (response.IsSuccessStatusCode)
            {
                return new NotificationDeliveryResult
                {
                    Success = true,
                    ExternalId = Guid.NewGuid().ToString()
                };
            }
            
            var error = await response.Content.ReadAsStringAsync();
            _logger.LogError("Slack API error: {Error}", error);
            
            return new NotificationDeliveryResult
            {
                Success = false,
                ErrorMessage = error,
                ShouldRetry = (int)response.StatusCode >= 500
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending Slack message");
            
            return new NotificationDeliveryResult
            {
                Success = false,
                ErrorMessage = ex.Message,
                ShouldRetry = true
            };
        }
    }
    
    public async Task<bool> ValidateConfigurationAsync(NotificationChannel channel)
    {
        var config = channel.GetConfiguration<SlackChannelConfig>();
        return !string.IsNullOrEmpty(config.WebhookUrl) &&
               config.WebhookUrl.Contains("hooks.slack.com");
    }
    
    public async Task<bool> TestConnectionAsync(NotificationChannel channel, string? testRecipient = null)
    {
        var result = await SendAsync(
            channel,
            testRecipient ?? string.Empty,
            "🔔 ERA Monitor Test",
            "This is a test notification from ERA Monitor. If you see this, your Slack integration is working correctly!"
        );
        
        return result.Success;
    }
}
Webhook Provider
csharp// src/ERAMonitor.Infrastructure/Notifications/Providers/WebhookNotificationProvider.cs

using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using ERAMonitor.Core.Entities;
using ERAMonitor.Core.Entities.Configurations;
using ERAMonitor.Core.Enums;

namespace ERAMonitor.Infrastructure.Notifications.Providers;

public class WebhookNotificationProvider : INotificationProvider
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<WebhookNotificationProvider> _logger;
    
    public NotificationChannelType ChannelType => NotificationChannelType.Webhook;
    
    public WebhookNotificationProvider(
        IHttpClientFactory httpClientFactory,
        ILogger<WebhookNotificationProvider> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }
    
    public async Task<NotificationDeliveryResult> SendAsync(
        NotificationChannel channel,
        string recipient,
        string subject,
        string body,
        string? htmlBody = null,
        Dictionary<string, string>? metadata = null)
    {
        try
        {
            var config = channel.GetConfiguration<WebhookChannelConfig>();
            var client = _httpClientFactory.CreateClient();
            client.Timeout = TimeSpan.FromSeconds(config.TimeoutSeconds);
            
            var payload = new
            {
                event_type = metadata?.GetValueOrDefault("event_type", "notification"),
                timestamp = DateTime.UtcNow.ToString("O"),
                subject,
                body,
                metadata
            };
            
            var jsonPayload = JsonSerializer.Serialize(payload);
            var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
            
            // Add custom headers
            foreach (var header in config.Headers)
            {
                content.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }
            
            // Add HMAC signature if configured
            if (config.IncludeSignature && !string.IsNullOrEmpty(config.SecretKey))
            {
                var signature = ComputeHmacSignature(jsonPayload, config.SecretKey);
                content.Headers.TryAddWithoutValidation("X-ERA-Signature", signature);
            }
            
            HttpResponseMessage response;
            
            if (config.Method.ToUpper() == "POST")
            {
                response = await client.PostAsync(config.Url, content);
            }
            else if (config.Method.ToUpper() == "PUT")
            {
                response = await client.PutAsync(config.Url, content);
            }
            else
            {
                throw new NotSupportedException($"HTTP method {config.Method} not supported");
            }
            
            if (response.IsSuccessStatusCode)
            {
                return new NotificationDeliveryResult
                {
                    Success = true,
                    ExternalId = Guid.NewGuid().ToString()
                };
            }
            
            var error = await response.Content.ReadAsStringAsync();
            
            return new NotificationDeliveryResult
            {
                Success = false,
                ErrorMessage = $"HTTP {(int)response.StatusCode}: {error}",
                ErrorCode = ((int)response.StatusCode).ToString(),
                ShouldRetry = (int)response.StatusCode >= 500 || response.StatusCode == System.Net.HttpStatusCode.TooManyRequests
            };
        }
        catch (TaskCanceledException)
        {
            return new NotificationDeliveryResult
            {
                Success = false,
                ErrorMessage = "Request timed out",
                ErrorCode = "TIMEOUT",
                ShouldRetry = true
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending webhook");
            
            return new NotificationDeliveryResult
            {
                Success = false,
                ErrorMessage = ex.Message,
                ShouldRetry = true
            };
        }
    }
    
    public async Task<bool> ValidateConfigurationAsync(NotificationChannel channel)
    {
        var config = channel.GetConfiguration<WebhookChannelConfig>();
        
        return !string.IsNullOrEmpty(config.Url) &&
               Uri.TryCreate(config.Url, UriKind.Absolute, out _);
    }
    
    public async Task<bool> TestConnectionAsync(NotificationChannel channel, string? testRecipient = null)
    {
        var result = await SendAsync(
            channel,
            string.Empty,
            "ERA Monitor Test",
            "This is a test notification from ERA Monitor.",
            metadata: new Dictionary<string, string>
            {
                ["event_type"] = "test",
                ["is_test"] = "true"
            }
        );
        
        return result.Success;
    }
    
    private static string ComputeHmacSignature(string payload, string secret)
    {
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
        return $"sha256={Convert.ToHexString(hash).ToLower()}";
    }
}

5.7 NotificationService Implementation
csharp// src/ERAMonitor.Infrastructure/Services/NotificationService.cs

using Microsoft.Extensions.Logging;
using ERAMonitor.Core.Entities;
using ERAMonitor.Core.Enums;
using ERAMonitor.Core.Interfaces.Repositories;
using ERAMonitor.Core.Interfaces.Services;
using ERAMonitor.Infrastructure.Notifications.Providers;

namespace ERAMonitor.Infrastructure.Services;

public class NotificationService : INotificationService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificationRuleService _ruleService;
    private readonly INotificationTemplateService _templateService;
    private readonly IEnumerable<INotificationProvider> _providers;
    private readonly ILogger<NotificationService> _logger;
    
    public NotificationService(
        IUnitOfWork unitOfWork,
        INotificationRuleService ruleService,
        INotificationTemplateService templateService,
        IEnumerable<INotificationProvider> providers,
        ILogger<NotificationService> logger)
    {
        _unitOfWork = unitOfWork;
        _ruleService = ruleService;
        _templateService = templateService;
        _providers = providers;
        _logger = logger;
    }
    
    public async Task SendNotificationAsync(NotificationRequest request)
    {
        _logger.LogDebug("Processing notification request: {EventType}", request.EventType);
        
        try
        {
            // Get channel
            NotificationChannel? channel = null;
            
            if (request.ChannelId.HasValue)
            {
                channel = await _unitOfWork.NotificationChannels.GetByIdAsync(request.ChannelId.Value);
            }
            else
            {
                // Get default channel for organization
                channel = await _unitOfWork.NotificationChannels.GetDefaultAsync(request.OrganizationId);
            }
            
            if (channel == null || !channel.IsEnabled)
            {
                _logger.LogWarning("No valid channel found for notification");
                return;
            }
            
            // Get provider
            var provider = _providers.FirstOrDefault(p => p.ChannelType == channel.Type);
            if (provider == null)
            {
                _logger.LogError("No provider found for channel type {ChannelType}", channel.Type);
                return;
            }
            
            // Apply template if not already formatted
            var subject = request.Subject;
            var body = request.Body;
            var htmlBody = request.HtmlBody;
            
            if (request.Variables.Any())
            {
                subject = ApplyVariables(subject, request.Variables);
                body = ApplyVariables(body, request.Variables);
                if (!string.IsNullOrEmpty(htmlBody))
                {
                    htmlBody = ApplyVariables(htmlBody, request.Variables);
                }
            }
            
            // Determine recipients
            var recipients = await ResolveRecipientsAsync(request, channel);
            
            // Send to each recipient
            foreach (var recipient in recipients)
            {
                // Check rate limiting
                if (IsRateLimited(channel))
                {
                    _logger.LogWarning("Channel {ChannelId} is rate limited", channel.Id);
                    await CreatePendingLogAsync(request, channel, recipient, "Rate limited");
                    continue;
                }
                
                // Create log entry
                var log = new NotificationLog
                {
                    OrganizationId = request.OrganizationId,
                    ChannelId = channel.Id,
                    EventType = request.EventType,
                    SourceType = request.SourceType,
                    SourceId = request.SourceId,
                    SourceName = request.SourceName,
                    Recipient = recipient,
                    Subject = subject,
                    Body = body,
                    Status = NotificationStatus.Sending
                };
                
                await _unitOfWork.NotificationLogs.AddAsync(log);
                await _unitOfWork.SaveChangesAsync();
                
                // Send notification
                var result = await provider.SendAsync(
                    channel,
                    recipient,
                    subject,
                    body,
                    htmlBody,
                    new Dictionary<string, string>
                    {
                        ["event_type"] = request.EventType.ToString(),
                        ["source_type"] = request.SourceType ?? "",
                        ["source_id"] = request.SourceId?.ToString() ?? ""
                    }
                );
                
                // Update log
                if (result.Success)
                {
                    log.Status = NotificationStatus.Sent;
                    log.SentAt = DateTime.UtcNow;
                    log.ExternalId = result.ExternalId;
                    
                    channel.LastUsedAt = DateTime.UtcNow;
                    channel.CurrentHourCount++;
                }
                else
                {
                    log.Status = result.ShouldRetry ? NotificationStatus.Pending : NotificationStatus.Failed;
                    log.FailedAt = DateTime.UtcNow;
                    log.ErrorMessage = result.ErrorMessage;
                    log.RetryCount = 0;
                    log.NextRetryAt = result.ShouldRetry ? DateTime.UtcNow.AddMinutes(5) : null;
                    
                    channel.LastFailedAt = DateTime.UtcNow;
                    channel.LastError = result.ErrorMessage;
                }
                
                _unitOfWork.NotificationLogs.Update(log);
                _unitOfWork.NotificationChannels.Update(channel);
                await _unitOfWork.SaveChangesAsync();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending notification");
            throw;
        }
    }
    
    public async Task SendBulkNotificationsAsync(List<NotificationRequest> requests)
    {
        foreach (var request in requests)
        {
            try
            {
                await SendNotificationAsync(request);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in bulk notification for {EventType}", request.EventType);
            }
        }
    }
    
    public async Task ProcessEventAsync(NotificationEventType eventType, NotificationEventData data)
    {
        _logger.LogDebug("Processing event: {EventType} from {SourceType}:{SourceName}",
            eventType, data.SourceType, data.SourceName);
        
        // Get matching rules
        var rules = await _ruleService.GetMatchingRulesAsync(data.OrganizationId, eventType, data);
        
        if (!rules.Any())
        {
            _logger.LogDebug("No matching rules for event {EventType}", eventType);
            return;
        }
        
        _logger.LogDebug("Found {Count} matching rules", rules.Count);
        
        foreach (var rule in rules.OrderBy(r => r.Priority))
        {
            try
            {
                // Check cooldown
                if (rule.CooldownMinutes.HasValue && rule.LastTriggeredAt.HasValue)
                {
                    var cooldownEnd = rule.LastTriggeredAt.Value.AddMinutes(rule.CooldownMinutes.Value);
                    if (DateTime.UtcNow < cooldownEnd)
                    {
                        _logger.LogDebug("Rule {RuleId} is in cooldown", rule.Id);
                        continue;
                    }
                }
                
                // Check working hours
                if (!rule.IsWithinWorkingHours(DateTime.UtcNow))
                {
                    _logger.LogDebug("Rule {RuleId} is outside working hours", rule.Id);
                    
                    // Check for escalation
                    if (rule.EscalateToRuleId.HasValue && rule.EscalateAfterMinutes.HasValue)
                    {
                        // Schedule escalation
                        _logger.LogDebug("Scheduling escalation for rule {RuleId}", rule.Id);
                    }
                    
                    continue;
                }
                
                // Get template
                var template = rule.Template ?? await _templateService.GetDefaultTemplateAsync(
                    data.OrganizationId, eventType, rule.Channel.Type);
                
                // Build variables
                var variables = BuildVariables(eventType, data);
                
                // Apply template
                var subject = template != null ? ApplyVariables(template.Subject, variables) : GetDefaultSubject(eventType, data);
                var body = template != null ? ApplyVariables(template.Body, variables) : GetDefaultBody(eventType, data);
                var htmlBody = template?.HtmlBody != null ? ApplyVariables(template.HtmlBody, variables) : null;
                
                // Build request
                var request = new NotificationRequest
                {
                    OrganizationId = data.OrganizationId,
                    EventType = eventType,
                    SourceType = data.SourceType,
                    SourceId = data.SourceId,
                    SourceName = data.SourceName,
                    CustomerId = data.CustomerId,
                    Subject = subject,
                    Body = body,
                    HtmlBody = htmlBody,
                    ChannelId = rule.ChannelId,
                    Recipients = rule.GetRecipients(),
                    DelaySeconds = rule.DelaySeconds
                };
                
                // Send with delay if specified
                if (rule.DelaySeconds > 0)
                {
                    // Schedule for later (would use Hangfire in production)
                    _logger.LogDebug("Scheduling notification with {Delay}s delay", rule.DelaySeconds);
                    await Task.Delay(TimeSpan.FromSeconds(rule.DelaySeconds));
                }
                
                await SendNotificationAsync(request);
                
                // Update rule statistics
                rule.TotalNotificationsSent++;
                rule.LastTriggeredAt = DateTime.UtcNow;
                _unitOfWork.NotificationRules.Update(rule);
                await _unitOfWork.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing rule {RuleId}", rule.Id);
            }
        }
    }
    
    #region Convenience Methods
    
    public async Task SendHostDownNotificationAsync(Host host)
    {
        await ProcessEventAsync(NotificationEventType.HostDown, new NotificationEventData
        {
            OrganizationId = host.OrganizationId,
            CustomerId = host.CustomerId,
            SourceType = "Host",
            SourceId = host.Id,
            SourceName = host.Name,
            Severity = IncidentSeverity.Critical,
            Data = new Dictionary<string, string>
            {
                ["hostname"] = host.Hostname,
                ["ip"] = host.PrimaryIp ?? "",
                ["os"] = host.OsType.ToString(),
                ["last_seen"] = host.LastSeenAt?.ToString("yyyy-MM-dd HH:mm:ss UTC") ?? "Never"
            },
            Tags = host.Tags?.ToList()
        });
    }
    
    public async Task SendHostUpNotificationAsync(Host host)
    {
        await ProcessEventAsync(NotificationEventType.HostUp, new NotificationEventData
        {
            OrganizationId = host.OrganizationId,
            CustomerId = host.CustomerId,
            SourceType = "Host",
            SourceId = host.Id,
            SourceName = host.Name,
            Severity = IncidentSeverity.Info,
            Data = new Dictionary<string, string>
            {
                ["hostname"] = host.Hostname,
                ["downtime"] = host.StatusChangedAt.HasValue
                    ? (DateTime.UtcNow - host.StatusChangedAt.Value).ToString(@"hh\:mm\:ss")
                    : "Unknown"
            }
        });
    }
    
    public async Task SendServiceDownNotificationAsync(Service service)
    {
        await ProcessEventAsync(NotificationEventType.ServiceDown, new NotificationEventData
        {
            OrganizationId = service.Host.OrganizationId,
            CustomerId = service.Host.CustomerId,
            SourceType = "Service",
            SourceId = service.Id,
            SourceName = service.DisplayName ?? service.ServiceName,
            Severity = IncidentSeverity.High,
            Data = new Dictionary<string, string>
            {
                ["service_name"] = service.ServiceName,
                ["service_type"] = service.ServiceType.ToString(),
                ["host_name"] = service.Host.Name
            }
        });
    }
    
    public async Task SendServiceUpNotificationAsync(Service service)
    {
        await ProcessEventAsync(NotificationEventType.ServiceUp, new NotificationEventData
        {
            OrganizationId = service.Host.OrganizationId,
            CustomerId = service.Host.CustomerId,
            SourceType = "Service",
            SourceId = service.Id,
            SourceName = service.DisplayName ?? service.ServiceName,
            Severity = IncidentSeverity.Info,
            Data = new Dictionary<string, string>
            {
                ["service_name"] = service.ServiceName,
                ["host_name"] = service.Host.Name
            }
        });
    }
    
    public async Task SendCheckFailedNotificationAsync(Check check, CheckResult result)
    {
        await ProcessEventAsync(NotificationEventType.CheckFailed, new NotificationEventData
        {
            OrganizationId = check.OrganizationId,
            CustomerId = check.CustomerId,
            SourceType = "Check",
            SourceId = check.Id,
            SourceName = check.Name,
            Severity = IncidentSeverity.High,
            Data = new Dictionary<string, string>
            {
                ["check_type"] = check.Type.ToString(),
                ["target"] = check.Target,
                ["error"] = result.ErrorMessage ?? "Unknown error",
                ["response_time"] = $"{result.ResponseTimeMs}ms"
            },
            Tags = check.Tags?.ToList()
        });
    }
    
    public async Task SendCheckRecoveredNotificationAsync(Check check)
    {
        await ProcessEventAsync(NotificationEventType.CheckRecovered, new NotificationEventData
        {
            OrganizationId = check.OrganizationId,
            CustomerId = check.CustomerId,
            SourceType = "Check",
            SourceId = check.Id,
            SourceName = check.Name,
            Severity = IncidentSeverity.Info,
            Data = new Dictionary<string, string>
            {
                ["check_type"] = check.Type.ToString(),
                ["target"] = check.Target
            }
        });
    }
    
    public async Task SendIncidentCreatedNotificationAsync(Incident incident)
    {
        await ProcessEventAsync(NotificationEventType.IncidentCreated, new NotificationEventData
        {
            OrganizationId = incident.OrganizationId,
            CustomerId = incident.CustomerId,
            SourceType = "Incident",
            SourceId = incident.Id,
            SourceName = incident.GetIncidentId(),
            Severity = incident.Severity,
            Data = new Dictionary<string, string>
            {
                ["title"] = incident.Title,
                ["description"] = incident.Description ?? "",
                ["severity"] = incident.Severity.ToString(),
                ["category"] = incident.Category ?? "",
                ["source_type"] = incident.SourceType ?? "",
                ["source_name"] = incident.SourceName ?? ""
            }
        });
    }
    
    public async Task SendIncidentAssignedNotificationAsync(Incident incident, User assignee)
    {
        await ProcessEventAsync(NotificationEventType.IncidentAssigned, new NotificationEventData
        {
            OrganizationId = incident.OrganizationId,
            CustomerId = incident.CustomerId,
            SourceType = "Incident",
            SourceId = incident.Id,
            SourceName = incident.GetIncidentId(),
            Severity = incident.Severity,
            Data = new Dictionary<string, string>
            {
                ["title"] = incident.Title,
                ["assignee_name"] = assignee.FullName,
                ["assignee_email"] = assignee.Email
            }
        });
    }
    
    public async Task SendIncidentResolvedNotificationAsync(Incident incident)
    {
        await ProcessEventAsync(NotificationEventType.IncidentResolved, new NotificationEventData
        {
            OrganizationId = incident.OrganizationId,
            CustomerId = incident.CustomerId,
            SourceType = "Incident",
            SourceId = incident.Id,
            SourceName = incident.GetIncidentId(),
            Severity = IncidentSeverity.Info,
            Data = new Dictionary<string, string>
            {
                ["title"] = incident.Title,
                ["resolution"] = incident.ResolutionNotes ?? "",
                ["duration"] = incident.GetDuration()?.ToString(@"hh\:mm\:ss") ?? ""
            }
        });
    }
    
    public async Task SendIncidentEscalatedNotificationAsync(Incident incident, string reason)
    {
        await ProcessEventAsync(NotificationEventType.IncidentEscalated, new NotificationEventData
        {
            OrganizationId = incident.OrganizationId,
            CustomerId = incident.CustomerId,
            SourceType = "Incident",
            SourceId = incident.Id,
            SourceName = incident.GetIncidentId(),
            Severity = incident.Severity,
            Data = new Dictionary<string, string>
            {
                ["title"] = incident.Title,
                ["severity"] = incident.Severity.ToString(),
                ["reason"] = reason
            }
        });
    }
    
    public async Task SendSlaBreachNotificationAsync(Incident incident, string breachType)
    {
        var eventType = breachType == "Response"
            ? NotificationEventType.SlaResponseBreached
            : NotificationEventType.SlaResolutionBreached;
        
        await ProcessEventAsync(eventType, new NotificationEventData
        {
            OrganizationId = incident.OrganizationId,
            CustomerId = incident.CustomerId,
            SourceType = "Incident",
            SourceId = incident.Id,
            SourceName = incident.GetIncidentId(),
            Severity = IncidentSeverity.High,
            Data = new Dictionary<string, string>
            {
                ["title"] = incident.Title,
                ["breach_type"] = breachType,
                ["severity"] = incident.Severity.ToString()
            }
        });
    }
    
    public async Task SendSslExpiryNotificationAsync(Check check, int daysUntilExpiry)
    {
        var eventType = daysUntilExpiry <= 0
            ? NotificationEventType.SslCertificateExpired
            : NotificationEventType.SslCertificateExpiring;
        
        await ProcessEventAsync(eventType, new NotificationEventData
        {
            OrganizationId = check.OrganizationId,
            CustomerId = check.CustomerId,
            SourceType = "Check",
            SourceId = check.Id,
            SourceName = check.Name,
            Severity = daysUntilExpiry <= 0 ? IncidentSeverity.Critical : IncidentSeverity.High,
            Data = new Dictionary<string, string>
            {
                ["target"] = check.Target,
                ["days_until_expiry"] = daysUntilExpiry.ToString(),
                ["expiry_date"] = check.SslExpiresAt?.ToString("yyyy-MM-dd") ?? ""
            }
        });
    }
    
    #endregion
    
    public async Task<bool> SendTestNotificationAsync(Guid channelId, Guid organizationId, string? recipient = null)
    {
        var channel = await _unitOfWork.NotificationChannels.GetByIdAsync(channelId);
        
        if (channel == null || channel.OrganizationId != organizationId)
        {
            throw new Core.Exceptions.NotFoundException($"Channel {channelId} not found");
        }
        
        var provider = _providers.FirstOrDefault(p => p.ChannelType == channel.Type);
        if (provider == null)
        {
            throw new Core.Exceptions.BusinessException($"No provider for channel type {channel.Type}", "NO_PROVIDER");
        }
        
        return await provider.TestConnectionAsync(channel, recipient);
    }
    
    public async Task RetryFailedNotificationsAsync()
    {
        var failedLogs = await _unitOfWork.NotificationLogs.GetPendingRetriesAsync(DateTime.UtcNow);
        
        foreach (var log in failedLogs)
        {
            if (log.RetryCount >= 3)
            {
                log.Status = NotificationStatus.Failed;
                log.NextRetryAt = null;
                _unitOfWork.NotificationLogs.Update(log);
                continue;
            }
            
            try
            {
                var channel = await _unitOfWork.NotificationChannels.GetByIdAsync(log.ChannelId);
                if (channel == null || !channel.IsEnabled)
                {
                    log.Status = NotificationStatus.Cancelled;
                    _unitOfWork.NotificationLogs.Update(log);
                    continue;
                }
                
                var provider = _providers.FirstOrDefault(p => p.ChannelType == channel.Type);
                if (provider == null)
                {
                    log.Status = NotificationStatus.Failed;
                    log.ErrorMessage = "Provider not found";
                    _unitOfWork.NotificationLogs.Update(log);
                    continue;
                }
                
                var result = await provider.SendAsync(
                    channel,
                    log.Recipient ?? "",
                    log.Subject ?? "",
                    log.Body ?? ""
                );
                
                if (result.Success)
                {
                    log.Status = NotificationStatus.Sent;
                    log.SentAt = DateTime.UtcNow;
                    log.ExternalId = result.ExternalId;
                    log.NextRetryAt = null;
                }
                else
                {
                    log.RetryCount++;
                    log.ErrorMessage = result.ErrorMessage;
                    log.NextRetryAt = DateTime.UtcNow.AddMinutes(5 * Math.Pow(2, log.RetryCount)); // Exponential backoff
                }
                
                _unitOfWork.NotificationLogs.Update(log);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrying notification {LogId}", log.Id);
                log.RetryCount++;
                log.ErrorMessage = ex.Message;
                log.NextRetryAt = DateTime.UtcNow.AddMinutes(5 * Math.Pow(2, log.RetryCount));
                _unitOfWork.NotificationLogs.Update(log);
            }
        }
        
        await _unitOfWork.SaveChangesAsync();
    }
    
    public async Task<User?> GetCurrentOnCallUserAsync(Guid scheduleId)
    {
        var schedule = await _unitOfWork.OnCallSchedules.GetByIdAsync(scheduleId);
        
        if (schedule == null || !schedule.IsActive)
            return null;
        
        // Check for active override
        var now = DateTime.UtcNow;
        var activeOverride = schedule.Overrides
            .FirstOrDefault(o => o.StartAt <= now && o.EndAt > now);
        
        if (activeOverride != null)
        {
            return activeOverride.User;
        }
        
        return schedule.CurrentOnCallUser;
    }
    
    public async Task SendOnCallNotificationAsync(Guid scheduleId, NotificationRequest request)
    {
        var onCallUser = await GetCurrentOnCallUserAsync(scheduleId);
        
        if (onCallUser == null)
        {
            _logger.LogWarning("No on-call user found for schedule {ScheduleId}", scheduleId);
            return;
        }
        
        request.UserIds = new List<Guid> { onCallUser.Id };
        await SendNotificationAsync(request);
    }
    
    #region Private Helper Methods
    
    private async Task<List<string>> ResolveRecipientsAsync(NotificationRequest request, NotificationChannel channel)
    {
        var recipients = new List<string>();
        
        // Add direct recipients
        if (request.Recipients?.Any() == true)
        {
            recipients.AddRange(request.Recipients);
        }
        
        // Add users
        if (request.UserIds?.Any() == true)
        {
            foreach (var userId in request.UserIds)
            {
                var user = await _unitOfWork.Users.GetByIdAsync(userId);
                if (user != null)
                {
                    var recipient = GetUserRecipient(user, channel.Type);
                    if (!string.IsNullOrEmpty(recipient))
                    {
                        recipients.Add(recipient);
                    }
                }
            }
        }
        
        // Add contact groups
        if (request.ContactGroupIds?.Any() == true)
        {
            foreach (var groupId in request.ContactGroupIds)
            {
                var group = await _unitOfWork.ContactGroups.GetByIdAsync(groupId);
                if (group != null)
                {
                    foreach (var member in group.Members)
                    {
                        if (member.UserId.HasValue)
                        {
                            var user = await _unitOfWork.Users.GetByIdAsync(member.UserId.Value);
                            if (user != null)
                            {
                                var recipient = GetUserRecipient(user, channel.Type);
                                if (!string.IsNullOrEmpty(recipient))
                                {
                                    recipients.Add(recipient);
                                }
                            }
                        }
                        else if (!string.IsNullOrEmpty(member.ExternalEmail) && channel.Type == NotificationChannelType.Email)
                        {
                            recipients.Add(member.ExternalEmail);
                        }
                        else if (!string.IsNullOrEmpty(member.ExternalPhone) && channel.Type == NotificationChannelType.Sms)
                        {
                            recipients.Add(member.ExternalPhone);
                        }
                    }
                }
            }
        }
        
        return recipients.Distinct().ToList();
    }
    
    private static string? GetUserRecipient(User user, NotificationChannelType channelType)
    {
        return channelType switch
        {
            NotificationChannelType.Email => user.Email,
            NotificationChannelType.Sms => user.Phone,
            _ => user.Email
        };
    }
    
    private static bool IsRateLimited(NotificationChannel channel)
    {
        if (channel.
        RateLimitResetAt.HasValue && DateTime.UtcNow > channel.RateLimitResetAt.Value)
{
channel.CurrentHourCount = 0;
channel.RateLimitResetAt = DateTime.UtcNow.AddHours(1);
}
    return channel.CurrentHourCount >= channel.RateLimitPerHour;
}

private static string ApplyVariables(string template, Dictionary<string, string> variables)
{
    foreach (var variable in variables)
    {
        template = template.Replace($"{{{{{variable.Key}}}}}", variable.Value);
    }
    return template;
}

private static Dictionary<string, string> BuildVariables(NotificationEventType eventType, NotificationEventData data)
{
    var variables = new Dictionary<string, string>
    {
        ["event_type"] = eventType.ToString(),
        ["source_type"] = data.SourceType,
        ["source_id"] = data.SourceId.ToString(),
        ["source_name"] = data.SourceName,
        ["severity"] = data.Severity?.ToString() ?? "",
        ["timestamp"] = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss UTC"),
        ["date"] = DateTime.UtcNow.ToString("yyyy-MM-dd"),
        ["time"] = DateTime.UtcNow.ToString("HH:mm:ss")
    };
    
    foreach (var item in data.Data)
    {
        variables[item.Key] = item.Value;
    }
    
    return variables;
}

private static string GetDefaultSubject(NotificationEventType eventType, NotificationEventData data)
{
    return eventType switch
    {
        NotificationEventType.HostDown => $"🔴 Host Down: {data.SourceName}",
        NotificationEventType.HostUp => $"🟢 Host Recovered: {data.SourceName}",
        NotificationEventType.ServiceDown => $"🔴 Service Down: {data.SourceName}",
        NotificationEventType.ServiceUp => $"🟢 Service Recovered: {data.SourceName}",
        NotificationEventType.CheckFailed => $"🔴 Check Failed: {data.SourceName}",
        NotificationEventType.CheckRecovered => $"🟢 Check Recovered: {data.SourceName}",
        NotificationEventType.IncidentCreated => $"🚨 New Incident: {data.SourceName}",
        NotificationEventType.IncidentResolved => $"✅ Incident Resolved: {data.SourceName}",
        NotificationEventType.SslCertificateExpiring => $"⚠️ SSL Certificate Expiring: {data.SourceName}",
        NotificationEventType.SslCertificateExpired => $"🔴 SSL Certificate Expired: {data.SourceName}",
        _ => $"ERA Monitor: {eventType}"
    };
}

private static string GetDefaultBody(NotificationEventType eventType, NotificationEventData data)
{
    var sb = new System.Text.StringBuilder();
    sb.AppendLine($"Event: {eventType}");
    sb.AppendLine($"Source: {data.SourceType} - {data.SourceName}");
    sb.AppendLine($"Time: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss UTC}");
    
    if (data.Severity.HasValue)
    {
        sb.AppendLine($"Severity: {data.Severity}");
    }
    
    if (data.Data.Any())
    {
        sb.AppendLine();
        sb.AppendLine("Details:");
        foreach (var item in data.Data)
        {
            sb.AppendLine($"  {item.Key}: {item.Value}");
        }
    }
    
    return sb.ToString();
}

private async Task CreatePendingLogAsync(NotificationRequest request, NotificationChannel channel, string recipient, string reason)
{
    var log = new NotificationLog
    {
        OrganizationId = request.OrganizationId,
        ChannelId = channel.Id,
        EventType = request.EventType,
        SourceType = request.SourceType,
        SourceId = request.SourceId,
        SourceName = request.SourceName,
        Recipient = recipient,
        Subject = request.Subject,
        Body = request.Body,
        Status = NotificationStatus.Pending,
        ErrorMessage = reason,
        NextRetryAt = DateTime.UtcNow.AddMinutes(5)
    };
    
    await _unitOfWork.NotificationLogs.AddAsync(log);
    await _unitOfWork.SaveChangesAsync();
}

#endregion
}

---

## 5.8 Controllers
```csharp
// src/ERAMonitor.API/Controllers/NotificationChannelsController.cs

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ERAMonitor.Core.DTOs.Common;
using ERAMonitor.Core.DTOs.Notifications;
using ERAMonitor.Core.Enums;
using ERAMonitor.Core.Interfaces.Services;
using ERAMonitor.API.Extensions;

namespace ERAMonitor.API.Controllers;

[ApiController]
[Route("api/notification-channels")]
[Authorize]
public class NotificationChannelsController : ControllerBase
{
    private readonly INotificationChannelService _channelService;
    private readonly INotificationService _notificationService;
    
    public NotificationChannelsController(
        INotificationChannelService channelService,
        INotificationService notificationService)
    {
        _channelService = channelService;
        _notificationService = notificationService;
    }
    
    [HttpGet]
    public async Task<ActionResult<PagedResponse<NotificationChannelDto>>> GetChannels(
        [FromQuery] PagedRequest request,
        [FromQuery] NotificationChannelType? type = null)
    {
        var organizationId = User.GetOrganizationId();
        var result = await _channelService.GetPagedAsync(organizationId, request, type);
        return Ok(result);
    }
    
    [HttpGet("{id}")]
    public async Task<ActionResult<NotificationChannelDetailDto>> GetChannel(Guid id)
    {
        var organizationId = User.GetOrganizationId();
        var channel = await _channelService.GetByIdAsync(id, organizationId);
        return Ok(channel);
    }
    
    [HttpPost]
    [Authorize(Policy = "RequireAdminRole")]
    public async Task<ActionResult<NotificationChannelDetailDto>> CreateChannel([FromBody] CreateNotificationChannelRequest request)
    {
        var organizationId = User.GetOrganizationId();
        var channel = await _channelService.CreateAsync(organizationId, request);
        return CreatedAtAction(nameof(GetChannel), new { id = channel.Id }, channel);
    }
    
    [HttpPut("{id}")]
    [Authorize(Policy = "RequireAdminRole")]
    public async Task<ActionResult<NotificationChannelDetailDto>> UpdateChannel(Guid id, [FromBody] UpdateNotificationChannelRequest request)
    {
        var organizationId = User.GetOrganizationId();
        var channel = await _channelService.UpdateAsync(id, organizationId, request);
        return Ok(channel);
    }
    
    [HttpDelete("{id}")]
    [Authorize(Policy = "RequireAdminRole")]
    public async Task<ActionResult<ApiResponse>> DeleteChannel(Guid id)
    {
        var organizationId = User.GetOrganizationId();
        await _channelService.DeleteAsync(id, organizationId);
        return Ok(ApiResponse.Ok("Channel deleted"));
    }
    
    [HttpPost("{id}/test")]
    [Authorize(Policy = "RequireOperatorRole")]
    public async Task<ActionResult<ApiResponse<bool>>> TestChannel(Guid id, [FromBody] TestChannelRequest request)
    {
        var organizationId = User.GetOrganizationId();
        var success = await _notificationService.SendTestNotificationAsync(id, organizationId, request.TestRecipient);
        return Ok(ApiResponse<bool>.Ok(success, success ? "Test notification sent" : "Test failed"));
    }
    
    [HttpPost("{id}/set-default")]
    [Authorize(Policy = "RequireAdminRole")]
    public async Task<ActionResult<ApiResponse>> SetDefault(Guid id)
    {
        var organizationId = User.GetOrganizationId();
        await _channelService.SetDefaultAsync(id, organizationId);
        return Ok(ApiResponse.Ok("Channel set as default"));
    }
}
```
```csharp
// src/ERAMonitor.API/Controllers/NotificationRulesController.cs

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ERAMonitor.Core.DTOs.Common;
using ERAMonitor.Core.DTOs.Notifications;
using ERAMonitor.Core.Enums;
using ERAMonitor.Core.Interfaces.Services;
using ERAMonitor.API.Extensions;

namespace ERAMonitor.API.Controllers;

[ApiController]
[Route("api/notification-rules")]
[Authorize]
public class NotificationRulesController : ControllerBase
{
    private readonly INotificationRuleService _ruleService;
    
    public NotificationRulesController(INotificationRuleService ruleService)
    {
        _ruleService = ruleService;
    }
    
    [HttpGet]
    public async Task<ActionResult<PagedResponse<NotificationRuleDto>>> GetRules(
        [FromQuery] PagedRequest request,
        [FromQuery] NotificationEventType? eventType = null,
        [FromQuery] bool? isEnabled = null)
    {
        var organizationId = User.GetOrganizationId();
        var result = await _ruleService.GetPagedAsync(organizationId, request, eventType, isEnabled);
        return Ok(result);
    }
    
    [HttpGet("{id}")]
    public async Task<ActionResult<NotificationRuleDetailDto>> GetRule(Guid id)
    {
        var organizationId = User.GetOrganizationId();
        var rule = await _ruleService.GetByIdAsync(id, organizationId);
        return Ok(rule);
    }
    
    [HttpPost]
    [Authorize(Policy = "RequireAdminRole")]
    public async Task<ActionResult<NotificationRuleDetailDto>> CreateRule([FromBody] CreateNotificationRuleRequest request)
    {
        var organizationId = User.GetOrganizationId();
        var rule = await _ruleService.CreateAsync(organizationId, request);
        return CreatedAtAction(nameof(GetRule), new { id = rule.Id }, rule);
    }
    
    [HttpPut("{id}")]
    [Authorize(Policy = "RequireAdminRole")]
    public async Task<ActionResult<NotificationRuleDetailDto>> UpdateRule(Guid id, [FromBody] UpdateNotificationRuleRequest request)
    {
        var organizationId = User.GetOrganizationId();
        var rule = await _ruleService.UpdateAsync(id, organizationId, request);
        return Ok(rule);
    }
    
    [HttpDelete("{id}")]
    [Authorize(Policy = "RequireAdminRole")]
    public async Task<ActionResult<ApiResponse>> DeleteRule(Guid id)
    {
        var organizationId = User.GetOrganizationId();
        await _ruleService.DeleteAsync(id, organizationId);
        return Ok(ApiResponse.Ok("Rule deleted"));
    }
    
    [HttpPost("{id}/toggle")]
    [Authorize(Policy = "RequireOperatorRole")]
    public async Task<ActionResult<ApiResponse<bool>>> ToggleRule(Guid id)
    {
        var organizationId = User.GetOrganizationId();
        var isEnabled = await _ruleService.ToggleAsync(id, organizationId);
        return Ok(ApiResponse<bool>.Ok(isEnabled, isEnabled ? "Rule enabled" : "Rule disabled"));
    }
    
    [HttpPost("reorder")]
    [Authorize(Policy = "RequireAdminRole")]
    public async Task<ActionResult<ApiResponse>> ReorderRules([FromBody] List<Guid> ruleIds)
    {
        var organizationId = User.GetOrganizationId();
        await _ruleService.ReorderAsync(organizationId, ruleIds);
        return Ok(ApiResponse.Ok("Rules reordered"));
    }
}
```
```csharp
// src/ERAMonitor.API/Controllers/NotificationLogsController.cs

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ERAMonitor.Core.DTOs.Common;
using ERAMonitor.Core.DTOs.Notifications;
using ERAMonitor.Core.Enums;
using ERAMonitor.Core.Interfaces.Repositories;
using ERAMonitor.API.Extensions;

namespace ERAMonitor.API.Controllers;

[ApiController]
[Route("api/notification-logs")]
[Authorize]
public class NotificationLogsController : ControllerBase
{
    private readonly INotificationLogRepository _logRepository;
    
    public NotificationLogsController(INotificationLogRepository logRepository)
    {
        _logRepository = logRepository;
    }
    
    [HttpGet]
    public async Task<ActionResult<PagedResponse<NotificationLogDto>>> GetLogs(
        [FromQuery] PagedRequest request,
        [FromQuery] NotificationChannelType? channelType = null,
        [FromQuery] NotificationStatus? status = null,
        [FromQuery] NotificationEventType? eventType = null,
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null)
    {
        var organizationId = User.GetOrganizationId();
        var result = await _logRepository.GetPagedAsync(
            organizationId, request, channelType, status, eventType, from, to);
        return Ok(result);
    }
    
    [HttpGet("{id}")]
    public async Task<ActionResult<NotificationLogDetailDto>> GetLog(Guid id)
    {
        var organizationId = User.GetOrganizationId();
        var log = await _logRepository.GetDetailAsync(id, organizationId);
        
        if (log == null)
            return NotFound();
        
        return Ok(log);
    }
    
    [HttpGet("statistics")]
    public async Task<ActionResult<NotificationStatsDto>> GetStatistics(
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null)
    {
        var organizationId = User.GetOrganizationId();
        var fromDate = from ?? DateTime.UtcNow.AddDays(-30);
        var toDate = to ?? DateTime.UtcNow;
        
        var stats = await _logRepository.GetStatisticsAsync(organizationId, fromDate, toDate);
        return Ok(stats);
    }
}
```

---

## 5.9 Phase 5 Checklist
```markdown
# Phase 5 Completion Checklist

## Entities
- [ ] NotificationChannel with configuration storage
- [ ] NotificationTemplate with variable support
- [ ] NotificationRule with filters and escalation
- [ ] NotificationLog for delivery tracking
- [ ] ContactGroup and ContactGroupMember
- [ ] OnCallSchedule with rotation
- [ ] OnCallOverride for schedule exceptions
- [ ] UserNotificationPreference

## Channel Configurations
- [ ] EmailChannelConfig
- [ ] SmsChannelConfig  
- [ ] WebhookChannelConfig
- [ ] TelegramChannelConfig
- [ ] SlackChannelConfig
- [ ] MsTeamsChannelConfig
- [ ] PushoverChannelConfig

## Enums
- [ ] NotificationChannelType
- [ ] NotificationEventType (all host/service/check/incident events)
- [ ] NotificationStatus
- [ ] NotificationPriority

## Notification Providers
- [ ] INotificationProvider interface
- [ ] EmailNotificationProvider
- [ ] TelegramNotificationProvider
- [ ] SlackNotificationProvider
- [ ] WebhookNotificationProvider
- [ ] (Optional) SmsNotificationProvider
- [ ] (Optional) MsTeamsNotificationProvider

## Services
- [ ] INotificationService with all event methods
- [ ] INotificationChannelService
- [ ] INotificationTemplateService
- [ ] INotificationRuleService

## Controllers
- [ ] NotificationChannelsController
- [ ] NotificationRulesController
- [ ] NotificationTemplatesController
- [ ] NotificationLogsController
- [ ] ContactGroupsController
- [ ] OnCallSchedulesController

## API Endpoints

Channels:
- [ ] GET /api/notification-channels
- [ ] GET /api/notification-channels/{id}
- [ ] POST /api/notification-channels
- [ ] PUT /api/notification-channels/{id}
- [ ] DELETE /api/notification-channels/{id}
- [ ] POST /api/notification-channels/{id}/test
- [ ] POST /api/notification-channels/{id}/set-default

Rules:
- [ ] GET /api/notification-rules
- [ ] GET /api/notification-rules/{id}
- [ ] POST /api/notification-rules
- [ ] PUT /api/notification-rules/{id}
- [ ] DELETE /api/notification-rules/{id}
- [ ] POST /api/notification-rules/{id}/toggle
- [ ] POST /api/notification-rules/reorder

Templates:
- [ ] GET /api/notification-templates
- [ ] GET /api/notification-templates/{id}
- [ ] POST /api/notification-templates
- [ ] PUT /api/notification-templates/{id}
- [ ] DELETE /api/notification-templates/{id}
- [ ] POST /api/notification-templates/preview
- [ ] GET /api/notification-templates/variables/{eventType}

Logs:
- [ ] GET /api/notification-logs
- [ ] GET /api/notification-logs/{id}
- [ ] GET /api/notification-logs/statistics

## Features
- [ ] Multi-channel notification delivery
- [ ] Template variable substitution
- [ ] Rule-based routing with filters
- [ ] Severity-based filtering
- [ ] Working hours restrictions
- [ ] Cooldown between notifications
- [ ] Escalation policies
- [ ] Rate limiting per channel
- [ ] Retry failed notifications
- [ ] Delivery status tracking
- [ ] User notification preferences
- [ ] Quiet hours support
- [ ] On-call schedule rotation
- [ ] On-call override support

## Background Jobs
- [ ] NotificationRetryJob
- [ ] OnCallRotationJob
- [ ] DailyDigestJob
- [ ] WeeklyReportJob

## Testing
- [ ] Provider unit tests
- [ ] Rule matching tests
- [ ] Template rendering tests
- [ ] Working hours calculation tests
```

---
