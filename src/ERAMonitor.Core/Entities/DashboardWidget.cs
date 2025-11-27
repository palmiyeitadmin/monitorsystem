using ERAMonitor.Core.Enums;

namespace ERAMonitor.Core.Entities;

public class DashboardWidget : BaseEntity
{
    public Guid DashboardId { get; set; }
    
    public string Title { get; set; } = string.Empty;
    public WidgetType Type { get; set; }
    
    // Position in grid
    public int PositionX { get; set; } = 0;
    public int PositionY { get; set; } = 0;
    public int Width { get; set; } = 4;
    public int Height { get; set; } = 2;
    
    // Widget-specific configuration (JSON)
    public string Configuration { get; set; } = "{}";
    
    // Data source filters
    public Guid? CustomerId { get; set; }
    public Guid? HostId { get; set; }
    public Guid? CheckId { get; set; }
    public string? Tags { get; set; } // JSON array
    
    // Time range
    public string TimeRange { get; set; } = "24h"; // 1h, 6h, 24h, 7d, 30d, custom
    public DateTime? CustomStartDate { get; set; }
    public DateTime? CustomEndDate { get; set; }
    
    // Display options
    public bool ShowTitle { get; set; } = true;
    public bool ShowLegend { get; set; } = true;
    public string? ColorScheme { get; set; }
    
    // Thresholds for visual indicators
    public string? Thresholds { get; set; } // JSON: {"warning": 80, "critical": 95}
    
    // Order
    public int SortOrder { get; set; } = 0;
    
    // Navigation
    public virtual Dashboard Dashboard { get; set; } = null!;
    
    // Helper methods
    public T GetConfiguration<T>() where T : class, new()
    {
        if (string.IsNullOrEmpty(Configuration))
            return new T();
        return System.Text.Json.JsonSerializer.Deserialize<T>(Configuration) ?? new T();
    }
    
    public (DateTime from, DateTime to) GetTimeRange()
    {
        var now = DateTime.UtcNow;
        
        if (TimeRange == "custom" && CustomStartDate.HasValue && CustomEndDate.HasValue)
        {
            return (CustomStartDate.Value, CustomEndDate.Value);
        }
        
        return TimeRange switch
        {
            "1h" => (now.AddHours(-1), now),
            "6h" => (now.AddHours(-6), now),
            "24h" => (now.AddHours(-24), now),
            "7d" => (now.AddDays(-7), now),
            "30d" => (now.AddDays(-30), now),
            "90d" => (now.AddDays(-90), now),
            _ => (now.AddHours(-24), now)
        };
    }
}
