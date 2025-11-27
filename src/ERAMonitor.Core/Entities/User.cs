using ERAMonitor.Core.Enums;

namespace ERAMonitor.Core.Entities;

public class User : BaseEntity
{
    public Guid OrganizationId { get; set; }

    
    // Authentication
    public string Email { get; set; } = string.Empty;
    public string? PasswordHash { get; set; }
    
    // Profile
    public string FullName { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public string? PhoneNumber { get; set; }
    public string? JobTitle { get; set; }
    public string Theme { get; set; } = "light";
    
    // Role & Permissions
    public UserRole Role { get; set; } = UserRole.Viewer;
    public string? Permissions { get; set; } // JSON for granular permissions
    
    // Notification Preferences
    public string? NotificationPreferences { get; set; } // JSON
    
    // Settings
    public string Timezone { get; set; } = "UTC";
    public string Locale { get; set; } = "en";
    
    // Status
    public bool IsActive { get; set; } = true;
    public bool EmailVerified { get; set; } = false;
    
    // Email Verification
    public string? EmailVerificationToken { get; set; }
    public DateTime? EmailVerificationTokenExpires { get; set; }
    
    // Password Reset
    public string? PasswordResetToken { get; set; }
    public DateTime? PasswordResetTokenExpires { get; set; }
    
    // Two-Factor Authentication
    public bool TwoFactorEnabled { get; set; } = false;
    public string? TwoFactorSecret { get; set; }
    public string[]? TwoFactorBackupCodes { get; set; }
    
    // Login Tracking
    public DateTime? LastLoginAt { get; set; }
    public string? LastLoginIp { get; set; }
    public int FailedLoginAttempts { get; set; } = 0;
    public DateTime? LockedUntil { get; set; }
    
    // Navigation
    public virtual Organization Organization { get; set; } = null!;
    public virtual ICollection<UserSession> Sessions { get; set; } = new List<UserSession>();
    public virtual ICollection<UserCustomerAssignment> CustomerAssignments { get; set; } = new List<UserCustomerAssignment>();
    public virtual ICollection<CustomerUser> CustomerUsers { get; set; } = new List<CustomerUser>();
    
    // Helper Properties
    public bool IsLocked => LockedUntil.HasValue && LockedUntil.Value > DateTime.UtcNow;
    
    public bool CanAccessCustomer(Guid customerId)
    {
        if (Role == UserRole.SuperAdmin) return true;
        return CustomerAssignments.Any(ca => ca.CustomerId == customerId);
    }
}
