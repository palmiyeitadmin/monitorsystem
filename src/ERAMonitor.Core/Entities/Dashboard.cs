using ERAMonitor.Core.Enums;

namespace ERAMonitor.Core.Entities;

public class Dashboard : BaseEntityWithOrganization
{
    public Guid? UserId { get; set; } // null = shared dashboard
    
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Slug { get; set; } = string.Empty; // URL-friendly name
    
    // Layout
    public int Columns { get; set; } = 12; // Grid columns
    public string? LayoutConfig { get; set; } // JSON layout configuration
    
    // Visibility
    public DashboardVisibility Visibility { get; set; } = DashboardVisibility.Private;
    public bool IsDefault { get; set; } = false;
    
    // Filters (default filters for all widgets)
    public Guid? DefaultCustomerId { get; set; }
    public Guid? DefaultLocationId { get; set; }
    
    // Auto-refresh
    public int RefreshIntervalSeconds { get; set; } = 30;
    
    // Theme
    public string? Theme { get; set; } // light, dark, auto
    
    // Navigation
    public virtual User? User { get; set; }
    public virtual Customer? DefaultCustomer { get; set; }
    public virtual ICollection<DashboardWidget> Widgets { get; set; } = new List<DashboardWidget>();
}
