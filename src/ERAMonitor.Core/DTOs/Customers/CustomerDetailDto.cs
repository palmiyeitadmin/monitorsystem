namespace ERAMonitor.Core.DTOs.Customers;

public class CustomerDetailDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? LogoUrl { get; set; }
    public string? Industry { get; set; }
    
    // Primary Contact
    public string? ContactName { get; set; }
    public string? ContactEmail { get; set; }
    public string? ContactPhone { get; set; }
    public string? ContactMobile { get; set; }
    public string? ContactJobTitle { get; set; }
    
    // Secondary Contact
    public string? SecondaryContactName { get; set; }
    public string? SecondaryContactEmail { get; set; }
    public string? SecondaryContactPhone { get; set; }
    
    // Emergency Contact
    public string? EmergencyContactName { get; set; }
    public string? EmergencyContactPhone { get; set; }
    public string? EmergencyAvailableHours { get; set; }
    
    // Address
    public string? AddressLine1 { get; set; }
    public string? AddressLine2 { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }
    public string? PostalCode { get; set; }
    
    // Notification Settings
    public CustomerNotificationSettingsDto NotificationSettings { get; set; } = new();
    
    // Portal Access
    public bool PortalEnabled { get; set; }
    public bool ApiEnabled { get; set; }
    public string? ApiKey { get; set; }
    public int ApiRateLimit { get; set; }
    
    // Assignment
    public AssignedAdminDto? AssignedAdmin { get; set; }
    
    // Stats
    public int HostCount { get; set; }
    public int WebsiteCount { get; set; }
    public int ActiveIncidentCount { get; set; }
    public int PortalUserCount { get; set; }
    
    public string? Notes { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class AssignedAdminDto
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
}

public class CustomerNotificationSettingsDto
{
    public NotificationChannelsDto Channels { get; set; } = new();
    public List<string> EmailRecipients { get; set; } = new();
    public List<string> SmsNumbers { get; set; } = new();
    public string? TelegramChatId { get; set; }
    public string? WebhookUrl { get; set; }
    public NotifyOnDto NotifyOn { get; set; } = new();
    public QuietHoursDto QuietHours { get; set; } = new();
}

public class NotificationChannelsDto
{
    public bool Email { get; set; } = true;
    public bool Sms { get; set; } = false;
    public bool Telegram { get; set; } = false;
    public bool Webhook { get; set; } = false;
}

public class NotifyOnDto
{
    public bool HostDown { get; set; } = true;
    public bool ServiceStopped { get; set; } = true;
    public bool HighCpu { get; set; } = true;
    public bool HighRam { get; set; } = true;
    public bool DiskCritical { get; set; } = true;
    public bool SslExpiring { get; set; } = true;
    public bool AllIncidents { get; set; } = false;
    public bool WeeklySummary { get; set; } = false;
}

public class QuietHoursDto
{
    public bool Enabled { get; set; } = false;
    public string From { get; set; } = "22:00";
    public string To { get; set; } = "07:00";
    public string Timezone { get; set; } = "UTC";
}
