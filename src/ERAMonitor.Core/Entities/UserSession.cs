namespace ERAMonitor.Core.Entities;

public class UserSession : BaseEntity
{
    public Guid UserId { get; set; }
    
    // Token
    public string TokenHash { get; set; } = string.Empty;
    
    // Device Info
    public string? DeviceInfo { get; set; }
    public string? DeviceName { get; set; }
    public string? DeviceType { get; set; } // Desktop, Mobile, Tablet
    public string? Browser { get; set; }
    public string? OperatingSystem { get; set; }
    
    // Location
    public string? IpAddress { get; set; }
    public string? Location { get; set; } // City, Country
    
    // User Agent
    public string? UserAgent { get; set; }
    
    // Status
    public DateTime ExpiresAt { get; set; }
    public bool IsRevoked { get; set; } = false;
    public DateTime? RevokedAt { get; set; }
    public string? RevokedReason { get; set; }
    
    // Activity
    public DateTime LastActiveAt { get; set; } = DateTime.UtcNow;
    
    // Navigation
    public virtual User User { get; set; } = null!;
    
    // Helper Properties
    public bool IsValid => !IsRevoked && ExpiresAt > DateTime.UtcNow;
}
