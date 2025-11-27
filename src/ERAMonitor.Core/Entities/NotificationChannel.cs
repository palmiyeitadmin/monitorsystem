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
