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
