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

public enum OnCallRotationType
{
    Daily,
    Weekly,
    BiWeekly,
    Monthly,
    Custom
}
