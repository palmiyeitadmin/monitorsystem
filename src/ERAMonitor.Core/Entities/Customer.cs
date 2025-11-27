using ERAMonitor.Core.Enums;

namespace ERAMonitor.Core.Entities;

public class Customer : BaseEntity
{
    public Guid OrganizationId { get; set; }
    public Organization Organization { get; set; } = null!;
    
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
    
    // Address
    public string? AddressLine1 { get; set; }
    public string? AddressLine2 { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }
    public string? PostalCode { get; set; }
    
    // Secondary Contact
    public string? SecondaryContactName { get; set; }
    public string? SecondaryContactEmail { get; set; }
    public string? SecondaryContactPhone { get; set; }
    
    // Emergency Contact
    public string? EmergencyContactName { get; set; }
    public string? EmergencyContactPhone { get; set; }
    public string? EmergencyAvailableHours { get; set; }
    
    // Notification Settings
    public string? NotificationSettings { get; set; } // JSONB
    
    // Portal Access
    public bool PortalEnabled { get; set; } = true;
    public bool ApiEnabled { get; set; } = false;
    public string? ApiKey { get; set; }
    public int ApiRateLimit { get; set; } = 1000;
    
    public Guid? AssignedAdminId { get; set; }
    public User? AssignedAdmin { get; set; }
    
    public string? Notes { get; set; }
    public bool IsActive { get; set; } = true;
    
    public ICollection<Host> Hosts { get; set; } = new List<Host>();
    public ICollection<UserCustomerAssignment> UserAssignments { get; set; } = new List<UserCustomerAssignment>();
    public ICollection<CustomerUser> CustomerUsers { get; set; } = new List<CustomerUser>();
    public ICollection<Incident> Incidents { get; set; } = new List<Incident>();
}
