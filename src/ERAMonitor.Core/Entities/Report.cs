using ERAMonitor.Core.Enums;

namespace ERAMonitor.Core.Entities;

public class Report : BaseEntityWithOrganization
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public ReportType Type { get; set; }
    
    // Schedule
    public bool IsScheduled { get; set; } = false;
    public ReportSchedule? Schedule { get; set; }
    public string? CronExpression { get; set; }
    public string Timezone { get; set; } = "Europe/Istanbul";
    public DateTime? NextRunAt { get; set; }
    public DateTime? LastRunAt { get; set; }
    
    // Filters
    public Guid? CustomerId { get; set; }
    public string? HostIds { get; set; } // JSON array
    public string? CheckIds { get; set; } // JSON array
    public string? Tags { get; set; } // JSON array
    
    // Time range
    public ReportTimeRange TimeRange { get; set; } = ReportTimeRange.Last7Days;
    public DateTime? CustomStartDate { get; set; }
    public DateTime? CustomEndDate { get; set; }
    
    // Output format
    public ReportFormat Format { get; set; } = ReportFormat.PDF;
    
    // Sections to include
    public string? Sections { get; set; } // JSON array of section types
    
    // Delivery
    public bool SendEmail { get; set; } = true;
    public string? EmailRecipients { get; set; } // JSON array
    public bool SaveToStorage { get; set; } = true;
    
    // Branding
    public string? LogoUrl { get; set; }
    public string? CompanyName { get; set; }
    public string? PrimaryColor { get; set; }
    
    // Status
    public bool IsActive { get; set; } = true;
    
    // Navigation
    public virtual Customer? Customer { get; set; }
    public virtual ICollection<ReportExecution> Executions { get; set; } = new List<ReportExecution>();
    
    // Helper methods
    public List<Guid> GetHostIds()
    {
        if (string.IsNullOrEmpty(HostIds)) return new();
        return System.Text.Json.JsonSerializer.Deserialize<List<Guid>>(HostIds) ?? new();
    }
    
    public List<string> GetEmailRecipients()
    {
        if (string.IsNullOrEmpty(EmailRecipients)) return new();
        return System.Text.Json.JsonSerializer.Deserialize<List<string>>(EmailRecipients) ?? new();
    }
    
    public (DateTime from, DateTime to) GetTimeRange()
    {
        var now = DateTime.UtcNow;
        
        if (TimeRange == ReportTimeRange.Custom && CustomStartDate.HasValue && CustomEndDate.HasValue)
        {
            return (CustomStartDate.Value, CustomEndDate.Value);
        }
        
        return TimeRange switch
        {
            ReportTimeRange.Today => (now.Date, now),
            ReportTimeRange.Yesterday => (now.Date.AddDays(-1), now.Date),
            ReportTimeRange.Last24Hours => (now.AddHours(-24), now),
            ReportTimeRange.Last7Days => (now.AddDays(-7), now),
            ReportTimeRange.Last30Days => (now.AddDays(-30), now),
            ReportTimeRange.LastMonth => (new DateTime(now.Year, now.Month, 1).AddMonths(-1), new DateTime(now.Year, now.Month, 1)),
            ReportTimeRange.LastQuarter => (now.AddMonths(-3), now),
            _ => (now.AddDays(-7), now)
        };
    }
}
