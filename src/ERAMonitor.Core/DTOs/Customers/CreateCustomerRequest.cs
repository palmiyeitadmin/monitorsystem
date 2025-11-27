using System.ComponentModel.DataAnnotations;

namespace ERAMonitor.Core.DTOs.Customers;

public class CreateCustomerRequest
{
    [Required(ErrorMessage = "Name is required")]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(100)]
    [RegularExpression(@"^[a-z0-9-]+$", ErrorMessage = "Slug can only contain lowercase letters, numbers and hyphens")]
    public string? Slug { get; set; }
    
    [MaxLength(500)]
    public string? LogoUrl { get; set; }
    
    [MaxLength(100)]
    public string? Industry { get; set; }
    
    // Primary Contact
    [MaxLength(200)]
    public string? ContactName { get; set; }
    
    [EmailAddress]
    [MaxLength(255)]
    public string? ContactEmail { get; set; }
    
    [Phone]
    [MaxLength(50)]
    public string? ContactPhone { get; set; }
    
    [Phone]
    [MaxLength(50)]
    public string? ContactMobile { get; set; }
    
    [MaxLength(100)]
    public string? ContactJobTitle { get; set; }
    
    // Secondary Contact
    [MaxLength(200)]
    public string? SecondaryContactName { get; set; }
    
    [EmailAddress]
    [MaxLength(255)]
    public string? SecondaryContactEmail { get; set; }
    
    [Phone]
    [MaxLength(50)]
    public string? SecondaryContactPhone { get; set; }
    
    // Emergency Contact
    [MaxLength(200)]
    public string? EmergencyContactName { get; set; }
    
    [Phone]
    [MaxLength(50)]
    public string? EmergencyContactPhone { get; set; }
    
    [MaxLength(50)]
    public string? EmergencyAvailableHours { get; set; }
    
    // Address
    [MaxLength(255)]
    public string? AddressLine1 { get; set; }
    
    [MaxLength(255)]
    public string? AddressLine2 { get; set; }
    
    [MaxLength(100)]
    public string? City { get; set; }
    
    [MaxLength(100)]
    public string? Country { get; set; }
    
    [MaxLength(20)]
    public string? PostalCode { get; set; }
    
    // Portal Access
    public bool PortalEnabled { get; set; } = true;
    public bool ApiEnabled { get; set; } = false;
    
    // Assignment
    public Guid? AssignedAdminId { get; set; }
    
    public string? Notes { get; set; }
}
