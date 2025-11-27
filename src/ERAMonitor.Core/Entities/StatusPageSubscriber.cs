namespace ERAMonitor.Core.Entities;

public class StatusPageSubscriber : BaseEntity
{
    public Guid StatusPageId { get; set; }
    
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? WebhookUrl { get; set; }
    
    // Subscription preferences
    public bool EmailSubscribed { get; set; } = false;
    public bool SmsSubscribed { get; set; } = false;
    
    public bool IsVerified { get; set; } = false;
    public bool EmailVerified { get; set; } = false;
    public string? VerificationToken { get; set; }
    public DateTime? VerifiedAt { get; set; }
    
    public bool NotifyOnIncident { get; set; } = true;
    public bool NotifyOnMaintenance { get; set; } = true;
    public bool NotifyOnResolution { get; set; } = true;
    
    public bool IsActive { get; set; } = true;
    public DateTime? UnsubscribedAt { get; set; }
    
    // Navigation
    public virtual StatusPage StatusPage { get; set; } = null!;
}
