using ERAMonitor.Core.Enums;

namespace ERAMonitor.Core.Entities;

public class StatusPage : BaseEntityWithOrganization
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Slug { get; set; } = string.Empty; // subdomain or path
    public string? CustomDomain { get; set; }
    
    // Branding
    public string? LogoUrl { get; set; }
    public string? FaviconUrl { get; set; }
    public string? CompanyName { get; set; }
    public string PrimaryColor { get; set; } = "#29ABE2";
    public string? CustomCss { get; set; }
    public string? WebsiteUrl { get; set; }
    public string? SupportUrl { get; set; }
    public string? Theme { get; set; }
    public string? GoogleAnalyticsId { get; set; }
    
    // Content
    public string? HeaderText { get; set; }
    public string? FooterText { get; set; }
    public string? AboutText { get; set; }
    
    // Features
    public bool ShowUptime { get; set; } = true;
    public int UptimeDays { get; set; } = 90;
    public bool ShowIncidents { get; set; } = true;
    public bool ShowMaintenances { get; set; } = true;
    public bool ShowSubscribe { get; set; } = true;
    public bool ShowResponseTime { get; set; } = false;
    
    // Visibility
    public bool IsPublic { get; set; } = true;
    public string? Password { get; set; } // Optional password protection
    
    // Components to show
    public virtual ICollection<StatusPageComponent> Components { get; set; } = new List<StatusPageComponent>();
    
    // Subscribers
    public virtual ICollection<StatusPageSubscriber> Subscribers { get; set; } = new List<StatusPageSubscriber>();
    
    // Active
    public bool IsActive { get; set; } = true;
}
