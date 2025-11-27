PHASE 6: Dashboard & Reporting (Days 25-30)
6.1 Overview
Phase 6 focuses on implementing:

Real-time dashboard with status overview
Interactive charts and graphs (CPU, RAM, uptime trends)
Custom dashboard widgets
Scheduled reports (daily, weekly, monthly)
Report templates (PDF, Excel export)
Uptime reports and SLA compliance
Performance analytics
Public status pages


6.2 Entities
Dashboard Entity
csharp// src/ERAMonitor.Core/Entities/Dashboard.cs

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

public enum DashboardVisibility
{
    Private,    // Only owner can see
    Team,       // All users in organization
    Public      // Public status page (no auth required)
}
DashboardWidget Entity
csharp// src/ERAMonitor.Core/Entities/DashboardWidget.cs

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

public enum WidgetType
{
    // Status widgets
    StatusOverview,
    HostStatusGrid,
    ServiceStatusList,
    CheckStatusList,
    IncidentList,
    
    // Metric widgets
    CpuChart,
    MemoryChart,
    DiskChart,
    NetworkChart,
    ResponseTimeChart,
    
    // Aggregated widgets
    UptimeGauge,
    UptimeChart,
    AvailabilityHeatmap,
    
    // Counter widgets
    HostCount,
    IncidentCount,
    CheckCount,
    AlertCount,
    
    // Table widgets
    TopHostsByCpu,
    TopHostsByMemory,
    RecentIncidents,
    RecentAlerts,
    FailingChecks,
    ExpiringCertificates,
    
    // Map widgets
    HostMap,
    
    // Custom
    Markdown,
    IFrame,
    Image
}
Report Entity
csharp// src/ERAMonitor.Core/Entities/Report.cs

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

public enum ReportType
{
    Executive,          // High-level summary
    Uptime,            // Detailed uptime/availability
    Performance,       // CPU, RAM, response times
    Incident,          // Incident analysis
    SLA,               // SLA compliance
    Security,          // SSL, vulnerabilities
    Capacity,          // Disk, resource trends
    Custom             // User-defined
}

public enum ReportSchedule
{
    Daily,
    Weekly,
    BiWeekly,
    Monthly,
    Quarterly
}

public enum ReportTimeRange
{
    Today,
    Yesterday,
    Last24Hours,
    Last7Days,
    Last30Days,
    LastMonth,
    LastQuarter,
    Custom
}

public enum ReportFormat
{
    PDF,
    Excel,
    HTML,
    JSON
}
ReportExecution Entity
csharp// src/ERAMonitor.Core/Entities/ReportExecution.cs

namespace ERAMonitor.Core.Entities;

public class ReportExecution : BaseEntity
{
    public Guid ReportId { get; set; }
    
    // Execution info
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public ReportExecutionStatus Status { get; set; } = ReportExecutionStatus.Pending;
    
    // Time range used
    public DateTime DataFromDate { get; set; }
    public DateTime DataToDate { get; set; }
    
    // Output
    public string? FilePath { get; set; }
    public string? FileUrl { get; set; }
    public long? FileSizeBytes { get; set; }
    public ReportFormat Format { get; set; }
    
    // Error info
    public string? ErrorMessage { get; set; }
    
    // Delivery status
    public bool EmailSent { get; set; } = false;
    public DateTime? EmailSentAt { get; set; }
    public string? EmailError { get; set; }
    
    // Triggered by
    public bool IsManual { get; set; } = false;
    public Guid? TriggeredByUserId { get; set; }
    
    // Navigation
    public virtual Report Report { get; set; } = null!;
    public virtual User? TriggeredByUser { get; set; }
}

public enum ReportExecutionStatus
{
    Pending,
    Running,
    Completed,
    Failed,
    Cancelled
}
StatusPage Entity
csharp// src/ERAMonitor.Core/Entities/StatusPage.cs

namespace ERAMonitor.Core.Entities;

public class StatusPage : BaseEntityWithOrganization
{
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty; // subdomain or path
    public string? CustomDomain { get; set; }
    
    // Branding
    public string? LogoUrl { get; set; }
    public string? FaviconUrl { get; set; }
    public string? CompanyName { get; set; }
    public string PrimaryColor { get; set; } = "#29ABE2";
    public string? CustomCss { get; set; }
    
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

public class StatusPageComponent : BaseEntity
{
    public Guid StatusPageId { get; set; }
    
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    
    // What this component represents
    public StatusPageComponentType Type { get; set; }
    public Guid? HostId { get; set; }
    public Guid? CheckId { get; set; }
    public Guid? ServiceId { get; set; }
    
    // Grouping
    public Guid? GroupId { get; set; }
    public int SortOrder { get; set; } = 0;
    
    // Display
    public bool ShowUptime { get; set; } = true;
    public bool ShowResponseTime { get; set; } = false;
    
    // Override status (for manual control)
    public bool OverrideStatus { get; set; } = false;
    public StatusPageComponentStatus? ManualStatus { get; set; }
    public string? ManualStatusMessage { get; set; }
    
    // Navigation
    public virtual StatusPage StatusPage { get; set; } = null!;
    public virtual Host? Host { get; set; }
    public virtual Check? Check { get; set; }
    public virtual Service? Service { get; set; }
    public virtual StatusPageComponentGroup? Group { get; set; }
}

public class StatusPageComponentGroup : BaseEntity
{
    public Guid StatusPageId { get; set; }
    
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int SortOrder { get; set; } = 0;
    public bool IsExpanded { get; set; } = true;
    
    // Navigation
    public virtual StatusPage StatusPage { get; set; } = null!;
    public virtual ICollection<StatusPageComponent> Components { get; set; } = new List<StatusPageComponent>();
}

public class StatusPageSubscriber : BaseEntity
{
    public Guid StatusPageId { get; set; }
    
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? WebhookUrl { get; set; }
    
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

public enum StatusPageComponentType
{
    Host,
    Check,
    Service,
    Manual // Manually controlled component
}

public enum StatusPageComponentStatus
{
    Operational,
    DegradedPerformance,
    PartialOutage,
    MajorOutage,
    UnderMaintenance
}

6.3 DTOs
Dashboard DTOs
csharp// src/ERAMonitor.Core/DTOs/Dashboard/DashboardDtos.cs

namespace ERAMonitor.Core.DTOs.Dashboard;

public class DashboardDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Slug { get; set; } = string.Empty;
    public DashboardVisibility Visibility { get; set; }
    public bool IsDefault { get; set; }
    public int WidgetCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class DashboardDetailDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Slug { get; set; } = string.Empty;
    public int Columns { get; set; }
    public DashboardVisibility Visibility { get; set; }
    public bool IsDefault { get; set; }
    public int RefreshIntervalSeconds { get; set; }
    public string? Theme { get; set; }
    
    public Guid? DefaultCustomerId { get; set; }
    public string? DefaultCustomerName { get; set; }
    
    public List<DashboardWidgetDto> Widgets { get; set; } = new();
    
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class DashboardWidgetDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public WidgetType Type { get; set; }
    public string TypeDisplay => Type.ToString();
    
    // Position
    public int PositionX { get; set; }
    public int PositionY { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    
    // Configuration
    public object? Configuration { get; set; }
    
    // Filters
    public Guid? CustomerId { get; set; }
    public Guid? HostId { get; set; }
    public Guid? CheckId { get; set; }
    public List<string>? Tags { get; set; }
    
    // Time range
    public string TimeRange { get; set; } = "24h";
    
    // Display options
    public bool ShowTitle { get; set; }
    public bool ShowLegend { get; set; }
    public object? Thresholds { get; set; }
    
    public int SortOrder { get; set; }
}

public class CreateDashboardRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Slug { get; set; }
    public int Columns { get; set; } = 12;
    public DashboardVisibility Visibility { get; set; } = DashboardVisibility.Private;
    public bool IsDefault { get; set; } = false;
    public int RefreshIntervalSeconds { get; set; } = 30;
    public string? Theme { get; set; }
    public Guid? DefaultCustomerId { get; set; }
}

public class UpdateDashboardRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int Columns { get; set; } = 12;
    public DashboardVisibility Visibility { get; set; }
    public bool IsDefault { get; set; }
    public int RefreshIntervalSeconds { get; set; } = 30;
    public string? Theme { get; set; }
    public Guid? DefaultCustomerId { get; set; }
}

public class CreateWidgetRequest
{
    public string Title { get; set; } = string.Empty;
    public WidgetType Type { get; set; }
    public int PositionX { get; set; } = 0;
    public int PositionY { get; set; } = 0;
    public int Width { get; set; } = 4;
    public int Height { get; set; } = 2;
    public object? Configuration { get; set; }
    public Guid? CustomerId { get; set; }
    public Guid? HostId { get; set; }
    public Guid? CheckId { get; set; }
    public List<string>? Tags { get; set; }
    public string TimeRange { get; set; } = "24h";
    public bool ShowTitle { get; set; } = true;
    public bool ShowLegend { get; set; } = true;
    public object? Thresholds { get; set; }
}

public class UpdateWidgetRequest : CreateWidgetRequest
{
}

public class UpdateWidgetPositionRequest
{
    public int PositionX { get; set; }
    public int PositionY { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
}

public class BatchUpdateWidgetPositionsRequest
{
    public List<WidgetPositionUpdate> Positions { get; set; } = new();
}

public class WidgetPositionUpdate
{
    public Guid WidgetId { get; set; }
    public int PositionX { get; set; }
    public int PositionY { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
}
Widget Data DTOs
csharp// src/ERAMonitor.Core/DTOs/Dashboard/WidgetDataDtos.cs

using ERAMonitor.Core.Enums;

namespace ERAMonitor.Core.DTOs.Dashboard;

// Status Overview Widget
public class StatusOverviewData
{
    public int TotalHosts { get; set; }
    public int HostsUp { get; set; }
    public int HostsDown { get; set; }
    public int HostsWarning { get; set; }
    public int HostsInMaintenance { get; set; }
    
    public int TotalServices { get; set; }
    public int ServicesUp { get; set; }
    public int ServicesDown { get; set; }
    
    public int TotalChecks { get; set; }
    public int ChecksPassing { get; set; }
    public int ChecksFailing { get; set; }
    
    public int OpenIncidents { get; set; }
    public int CriticalIncidents { get; set; }
    public int HighIncidents { get; set; }
    
    public decimal OverallHealth { get; set; } // Percentage
}

// Host Status Grid Widget
public class HostStatusGridData
{
    public List<HostStatusItem> Hosts { get; set; } = new();
}

public class HostStatusItem
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public StatusType Status { get; set; }
    public string StatusDisplay => Status.ToString();
    public decimal? CpuPercent { get; set; }
    public decimal? RamPercent { get; set; }
    public DateTime? LastSeenAt { get; set; }
    public string? CustomerName { get; set; }
    public bool IsInMaintenance { get; set; }
}

// Metric Chart Widget
public class MetricChartData
{
    public string MetricName { get; set; } = string.Empty;
    public string Unit { get; set; } = string.Empty;
    public List<MetricDataPoint> DataPoints { get; set; } = new();
    public MetricSummary Summary { get; set; } = new();
}

public class MetricDataPoint
{
    public DateTime Timestamp { get; set; }
    public decimal Value { get; set; }
    public string? Label { get; set; }
}

public class MetricSummary
{
    public decimal Current { get; set; }
    public decimal Average { get; set; }
    public decimal Min { get; set; }
    public decimal Max { get; set; }
    public decimal? P95 { get; set; }
}

// Uptime Widget
public class UptimeData
{
    public Guid? SourceId { get; set; }
    public string SourceName { get; set; } = string.Empty;
    public string SourceType { get; set; } = string.Empty;
    
    public decimal UptimePercent { get; set; }
    public TimeSpan TotalUptime { get; set; }
    public TimeSpan TotalDowntime { get; set; }
    public int OutageCount { get; set; }
    public TimeSpan? LongestOutage { get; set; }
    public TimeSpan? AverageOutage { get; set; }
    
    public List<UptimeDataPoint> History { get; set; } = new();
}

public class UptimeDataPoint
{
    public DateTime Date { get; set; }
    public decimal UptimePercent { get; set; }
    public int Outages { get; set; }
    public StatusType Status { get; set; }
}

// Heatmap Widget
public class AvailabilityHeatmapData
{
    public List<HeatmapRow> Rows { get; set; } = new();
    public List<string> TimeLabels { get; set; } = new();
}

public class HeatmapRow
{
    public Guid SourceId { get; set; }
    public string SourceName { get; set; } = string.Empty;
    public List<HeatmapCell> Cells { get; set; } = new();
}

public class HeatmapCell
{
    public DateTime Timestamp { get; set; }
    public decimal Value { get; set; } // 0-100
    public StatusType Status { get; set; }
    public string? Tooltip { get; set; }
}

// Counter Widget
public class CounterData
{
    public string Label { get; set; } = string.Empty;
    public int Value { get; set; }
    public int? PreviousValue { get; set; }
    public decimal? ChangePercent { get; set; }
    public string? Trend { get; set; } // up, down, stable
    public string? Icon { get; set; }
    public string? Color { get; set; }
}

// Table Widget
public class TableWidgetData<T>
{
    public List<T> Rows { get; set; } = new();
    public int TotalCount { get; set; }
    public bool HasMore { get; set; }
}

public class TopHostItem
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Value { get; set; }
    public string Unit { get; set; } = string.Empty;
    public StatusType Status { get; set; }
    public string? CustomerName { get; set; }
}

public class RecentIncidentItem
{
    public Guid Id { get; set; }
    public string IncidentId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public IncidentSeverity Severity { get; set; }
    public IncidentStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? Duration { get; set; }
}

public class FailingCheckItem
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Target { get; set; } = string.Empty;
    public string? ErrorMessage { get; set; }
    public int FailureCount { get; set; }
    public DateTime? LastCheckAt { get; set; }
}

public class ExpiringCertificateItem
{
    public Guid CheckId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Target { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public int DaysUntilExpiry { get; set; }
    public string Status { get; set; } = string.Empty; // Valid, Warning, Critical, Expired
}
Report DTOs
csharp// src/ERAMonitor.Core/DTOs/Reports/ReportDtos.cs

namespace ERAMonitor.Core.DTOs.Reports;

public class ReportDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public ReportType Type { get; set; }
    public string TypeDisplay => Type.ToString();
    public bool IsScheduled { get; set; }
    public ReportSchedule? Schedule { get; set; }
    public ReportFormat Format { get; set; }
    public bool IsActive { get; set; }
    public DateTime? LastRunAt { get; set; }
    public DateTime? NextRunAt { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class ReportDetailDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public ReportType Type { get; set; }
    
    // Schedule
    public bool IsScheduled { get; set; }
    public ReportSchedule? Schedule { get; set; }
    public string? CronExpression { get; set; }
    public string Timezone { get; set; } = string.Empty;
    public DateTime? NextRunAt { get; set; }
    public DateTime? LastRunAt { get; set; }
    
    // Filters
    public Guid? CustomerId { get; set; }
    public string? CustomerName { get; set; }
    public List<Guid> HostIds { get; set; } = new();
    public List<Guid> CheckIds { get; set; } = new();
    public List<string> Tags { get; set; } = new();
    
    // Time range
    public ReportTimeRange TimeRange { get; set; }
    public DateTime? CustomStartDate { get; set; }
    public DateTime? CustomEndDate { get; set; }
    
    // Format & delivery
    public ReportFormat Format { get; set; }
    public bool SendEmail { get; set; }
    public List<string> EmailRecipients { get; set; } = new();
    public bool SaveToStorage { get; set; }
    
    // Branding
    public string? LogoUrl { get; set; }
    public string? CompanyName { get; set; }
    
    // Status
    public bool IsActive { get; set; }
    
    // Recent executions
    public List<ReportExecutionDto> RecentExecutions { get; set; } = new();
    
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class ReportExecutionDto
{
    public Guid Id { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public ReportExecutionStatus Status { get; set; }
    public string StatusDisplay => Status.ToString();
    public ReportFormat Format { get; set; }
    public string? FileUrl { get; set; }
    public long? FileSizeBytes { get; set; }
    public string? FileSizeDisplay { get; set; }
    public string? ErrorMessage { get; set; }
    public bool EmailSent { get; set; }
    public bool IsManual { get; set; }
    public string? TriggeredByUserName { get; set; }
}

public class CreateReportRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public ReportType Type { get; set; }
    
    public bool IsScheduled { get; set; } = false;
    public ReportSchedule? Schedule { get; set; }
    public string? CronExpression { get; set; }
    public string Timezone { get; set; } = "Europe/Istanbul";
    
    public Guid? CustomerId { get; set; }
    public List<Guid>? HostIds { get; set; }
    public List<Guid>? CheckIds { get; set; }
    public List<string>? Tags { get; set; }
    
    public ReportTimeRange TimeRange { get; set; } = ReportTimeRange.Last7Days;
    public DateTime? CustomStartDate { get; set; }
    public DateTime? CustomEndDate { get; set; }
    
    public ReportFormat Format { get; set; } = ReportFormat.PDF;
    public bool SendEmail { get; set; } = true;
    public List<string>? EmailRecipients { get; set; }
    public bool SaveToStorage { get; set; } = true;
    
    public string? LogoUrl { get; set; }
    public string? CompanyName { get; set; }
    public string? PrimaryColor { get; set; }
    
    public bool IsActive { get; set; } = true;
}

public class UpdateReportRequest : CreateReportRequest
{
}

public class GenerateReportRequest
{
    public ReportTimeRange? TimeRangeOverride { get; set; }
    public DateTime? CustomStartDate { get; set; }
    public DateTime? CustomEndDate { get; set; }
    public ReportFormat? FormatOverride { get; set; }
}
Status Page DTOs
csharp// src/ERAMonitor.Core/DTOs/StatusPage/StatusPageDtos.cs

namespace ERAMonitor.Core.DTOs.StatusPage;

public class StatusPageDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? CustomDomain { get; set; }
    public string Url { get; set; } = string.Empty;
    public bool IsPublic { get; set; }
    public bool IsActive { get; set; }
    public int ComponentCount { get; set; }
    public int SubscriberCount { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class StatusPageDetailDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? CustomDomain { get; set; }
    
    // Branding
    public string? LogoUrl { get; set; }
    public string? FaviconUrl { get; set; }
    public string? CompanyName { get; set; }
    public string PrimaryColor { get; set; } = string.Empty;
    public string? CustomCss { get; set; }
    
    // Content
    public string? HeaderText { get; set; }
    public string? FooterText { get; set; }
    public string? AboutText { get; set; }
    
    // Features
    public bool ShowUptime { get; set; }
    public int UptimeDays { get; set; }
    public bool ShowIncidents { get; set; }
    public bool ShowMaintenances { get; set; }
    public bool ShowSubscribe { get; set; }
    public bool ShowResponseTime { get; set; }
    
    // Visibility
    public bool IsPublic { get; set; }
    public bool HasPassword { get; set; }
    
    // Components
    public List<StatusPageComponentGroupDto> Groups { get; set; } = new();
    public List<StatusPageComponentDto> UngroupedComponents { get; set; } = new();
    
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class StatusPageComponentGroupDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int SortOrder { get; set; }
    public bool IsExpanded { get; set; }
    public List<StatusPageComponentDto> Components { get; set; } = new();
    public StatusPageComponentStatus OverallStatus { get; set; }
}

public class StatusPageComponentDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public StatusPageComponentType Type { get; set; }
    public StatusPageComponentStatus Status { get; set; }
    public string StatusDisplay => Status.ToString();
    public string? StatusMessage { get; set; }
    public bool ShowUptime { get; set; }
    public decimal? UptimePercent { get; set; }
    public List<UptimeDayStatus>? UptimeHistory { get; set; }
    public bool ShowResponseTime { get; set; }
    public int? AvgResponseTimeMs { get; set; }
    public int SortOrder { get; set; }
}

public class UptimeDayStatus
{
    public DateTime Date { get; set; }
    public StatusPageComponentStatus Status { get; set; }
    public decimal UptimePercent { get; set; }
    public int? Outages { get; set; }
}

public class CreateStatusPageRequest
{
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? CustomDomain { get; set; }
    
    public string? LogoUrl { get; set; }
    public string? FaviconUrl { get; set; }
    public string? CompanyName { get; set; }
    public string PrimaryColor { get; set; } = "#29ABE2";
    public string? CustomCss { get; set; }
    
    public string? HeaderText { get; set; }
    public string? FooterText { get; set; }
    public string? AboutText { get; set; }
    
    public bool ShowUptime { get; set; } = true;
    public int UptimeDays { get; set; } = 90;
    public bool ShowIncidents { get; set; } = true;
    public bool ShowMaintenances { get; set; } = true;
    public bool ShowSubscribe { get; set; } = true;
    public bool ShowResponseTime { get; set; } = false;
    
    public bool IsPublic { get; set; } = true;
    public string? Password { get; set; }
}

public class UpdateStatusPageRequest : CreateStatusPageRequest
{
    public bool IsActive { get; set; } = true;
}

public class AddComponentRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public StatusPageComponentType Type { get; set; }
    public Guid? HostId { get; set; }
    public Guid? CheckId { get; set; }
    public Guid? ServiceId { get; set; }
    public Guid? GroupId { get; set; }
    public bool ShowUptime { get; set; } = true;
    public bool ShowResponseTime { get; set; } = false;
}

public class UpdateComponentStatusRequest
{
    public bool OverrideStatus { get; set; }
    public StatusPageComponentStatus? ManualStatus { get; set; }
    public string? ManualStatusMessage { get; set; }
}

// Public status page response (no auth required)
public class PublicStatusPageDto
{
    public string Name { get; set; } = string.Empty;
    public string? CompanyName { get; set; }
    public string? LogoUrl { get; set; }
    public string PrimaryColor { get; set; } = string.Empty;
    public string? HeaderText { get; set; }
    public string? FooterText { get; set; }
    
    public StatusPageOverallStatus OverallStatus { get; set; }
    public string OverallStatusMessage { get; set; } = string.Empty;
    
    public List<StatusPageComponentGroupDto> Groups { get; set; } = new();
    public List<StatusPageComponentDto> Components { get; set; } = new();
    
    public List<PublicIncidentDto>? RecentIncidents { get; set; }
    public List<PublicMaintenanceDto>? ScheduledMaintenances { get; set; }
    
    public bool ShowSubscribe { get; set; }
    public DateTime LastUpdated { get; set; }
}

public enum StatusPageOverallStatus
{
    Operational,
    DegradedPerformance,
    PartialOutage,
    MajorOutage,
    Maintenance
}

public class PublicIncidentDto
{
    public string Title { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public List<PublicIncidentUpdateDto> Updates { get; set; } = new();
}

public class PublicIncidentUpdateDto
{
    public string Status { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}

public class PublicMaintenanceDto
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime ScheduledStart { get; set; }
    public DateTime ScheduledEnd { get; set; }
    public List<string> AffectedComponents { get; set; } = new();
}

public class SubscribeRequest
{
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public bool NotifyOnIncident { get; set; } = true;
    public bool NotifyOnMaintenance { get; set; } = true;
}

6.4 Service Interfaces
csharp// src/ERAMonitor.Core/Interfaces/Services/IDashboardService.cs

using ERAMonitor.Core.DTOs.Common;
using ERAMonitor.Core.DTOs.Dashboard;

namespace ERAMonitor.Core.Interfaces.Services;

public interface IDashboardService
{
    // Dashboard CRUD
    Task<PagedResponse<DashboardDto>> GetPagedAsync(Guid organizationId, Guid userId, PagedRequest request);
    Task<DashboardDetailDto> GetByIdAsync(Guid id, Guid organizationId, Guid userId);
    Task<DashboardDetailDto> GetBySlugAsync(string slug, Guid organizationId, Guid userId);
    Task<DashboardDetailDto> CreateAsync(Guid organizationId, Guid userId, CreateDashboardRequest request);
    Task<DashboardDetailDto> UpdateAsync(Guid id, Guid organizationId, UpdateDashboardRequest request);
    Task DeleteAsync(Guid id, Guid organizationId);
    Task SetDefaultAsync(Guid id, Guid organizationId, Guid userId);
    Task<DashboardDetailDto?> GetDefaultDashboardAsync(Guid organizationId, Guid userId);
    
    // Widget CRUD
    Task<DashboardWidgetDto> AddWidgetAsync(Guid dashboardId, Guid organizationId, CreateWidgetRequest request);
    Task<DashboardWidgetDto> UpdateWidgetAsync(Guid widgetId, Guid organizationId, UpdateWidgetRequest request);
    Task DeleteWidgetAsync(Guid widgetId, Guid organizationId);
    Task UpdateWidgetPositionsAsync(Guid dashboardId, Guid organizationId, BatchUpdateWidgetPositionsRequest request);
    
    // Widget Data
    Task<object> GetWidgetDataAsync(Guid widgetId, Guid organizationId);
    Task<StatusOverviewData> GetStatusOverviewAsync(Guid organizationId, Guid? customerId = null);
    Task<HostStatusGridData> GetHostStatusGridAsync(Guid organizationId, Guid? customerId = null, int limit = 50);
    Task<MetricChartData> GetMetricChartDataAsync(Guid hostId, string metricType, DateTime from, DateTime to);
    Task<UptimeData> GetUptimeDataAsync(string sourceType, Guid sourceId, DateTime from, DateTime to);
    Task<AvailabilityHeatmapData> GetAvailabilityHeatmapAsync(Guid organizationId, DateTime from, DateTime to, Guid? customerId = null);
}
csharp// src/ERAMonitor.Core/Interfaces/Services/IReportService.cs

using ERAMonitor.Core.DTOs.Common;
using ERAMonitor.Core.DTOs.Reports;

namespace ERAMonitor.Core.Interfaces.Services;

public interface IReportService
{
    // Report CRUD
    Task<PagedResponse<ReportDto>> GetPagedAsync(Guid organizationId, PagedRequest request, ReportType? type = null);
    Task<ReportDetailDto> GetByIdAsync(Guid id, Guid organizationId);
    Task<ReportDetailDto> CreateAsync(Guid organizationId, CreateReportRequest request);
    Task<ReportDetailDto> UpdateAsync(Guid id, Guid organizationId, UpdateReportRequest request);
    Task DeleteAsync(Guid id, Guid organizationId);
    Task<bool> ToggleActiveAsync(Guid id, Guid organizationId);
    
    // Report Execution
    Task<ReportExecutionDto> GenerateReportAsync(Guid id, Guid organizationId, Guid userId, GenerateReportRequest? request = null);
    Task<ReportExecutionDto> GetExecutionAsync(Guid executionId, Guid organizationId);
    Task<List<ReportExecutionDto>> GetExecutionsAsync(Guid reportId, Guid organizationId, int limit = 10);
    Task<byte[]> DownloadReportAsync(Guid executionId, Guid organizationId);
    
    // Scheduled execution
    Task ProcessScheduledReportsAsync();
}
csharp// src/ERAMonitor.Core/Interfaces/Services/IStatusPageService.cs

using ERAMonitor.Core.DTOs.Common;
using ERAMonitor.Core.DTOs.StatusPage;

namespace ERAMonitor.Core.Interfaces.Services;

public interface IStatusPageService
{
    // Status Page CRUD
    Task<PagedResponse<StatusPageDto>> GetPagedAsync(Guid organizationId, PagedRequest request);
    Task<StatusPageDetailDto> GetByIdAsync(Guid id, Guid organizationId);
    Task<StatusPageDetailDto> CreateAsync(Guid organizationId, CreateStatusPageRequest request);
    Task<StatusPageDetailDto> UpdateAsync(Guid id, Guid organizationId, UpdateStatusPageRequest request);
    Task DeleteAsync(Guid id, Guid organizationId);
    
    // Components
    Task<StatusPageComponentDto> AddComponentAsync(Guid statusPageId, Guid organizationId, AddComponentRequest request);
    Task<StatusPageComponentDto> UpdateComponentAsync(Guid componentId, Guid organizationId, AddComponentRequest request);
    Task DeleteComponentAsync(Guid componentId, Guid organizationId);
    Task UpdateComponentStatusAsync(Guid componentId, Guid organizationId, UpdateComponentStatusRequest request);
    Task ReorderComponentsAsync(Guid statusPageId, Guid organizationId, List<Guid> componentIds);
    
    // Groups
    Task<StatusPageComponentGroupDto> AddGroupAsync(Guid statusPageId, Guid organizationId, string name, string? description = null);
    Task<StatusPageComponentGroupDto> UpdateGroupAsync(Guid groupId, Guid organizationId, string name, string? description = null);
    Task DeleteGroupAsync(Guid groupId, Guid organizationId);
    
    // Public access
    Task<PublicStatusPageDto> GetPublicStatusPageAsync(string slug, string? password = null);
    Task<bool> SubscribeAsync(string slug, SubscribeRequest request);
    Task<bool> VerifySubscriptionAsync(string token);
    Task<bool> UnsubscribeAsync(string token);
    
    // Notifications
    Task NotifySubscribersAsync(Guid statusPageId, string subject, string message);
}

6.5 Dashboard Service Implementation
csharp// src/ERAMonitor.Infrastructure/Services/DashboardService.cs

using Microsoft.Extensions.Logging;
using ERAMonitor.Core.DTOs.Common;
using ERAMonitor.Core.DTOs.Dashboard;
using ERAMonitor.Core.Entities;
using ERAMonitor.Core.Enums;
using ERAMonitor.Core.Exceptions;
using ERAMonitor.Core.Interfaces.Repositories;
using ERAMonitor.Core.Interfaces.Services;

namespace ERAMonitor.Infrastructure.Services;

public class DashboardService : IDashboardService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DashboardService> _logger;
    
    public DashboardService(IUnitOfWork unitOfWork, ILogger<DashboardService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }
    
    public async Task<PagedResponse<DashboardDto>> GetPagedAsync(Guid organizationId, Guid userId, PagedRequest request)
    {
        return await _unitOfWork.Dashboards.GetPagedAsync(organizationId, userId, request);
    }
    
    public async Task<DashboardDetailDto> GetByIdAsync(Guid id, Guid organizationId, Guid userId)
    {
        var dashboard = await _unitOfWork.Dashboards.GetDetailAsync(id, organizationId, userId);
        
        if (dashboard == null)
            throw new NotFoundException($"Dashboard {id} not found");
        
        return dashboard;
    }
    
    public async Task<DashboardDetailDto> GetBySlugAsync(string slug, Guid organizationId, Guid userId)
    {
        var dashboard = await _unitOfWork.Dashboards.GetBySlugAsync(slug, organizationId, userId);
        
        if (dashboard == null)
            throw new NotFoundException($"Dashboard '{slug}' not found");
        
        return await GetByIdAsync(dashboard.Id, organizationId, userId);
    }
    
    public async Task<DashboardDetailDto> CreateAsync(Guid organizationId, Guid userId, CreateDashboardRequest request)
    {
        // Generate slug if not provided
        var slug = request.Slug ?? GenerateSlug(request.Name);
        
        // Check slug uniqueness
        if (await _unitOfWork.Dashboards.SlugExistsAsync(slug, organizationId))
        {
            slug = $"{slug}-{Guid.NewGuid().ToString("N")[..6]}";
        }
        
        var dashboard = new Dashboard
        {
            OrganizationId = organizationId,
            UserId = request.Visibility == DashboardVisibility.Private ? userId : null,
            Name = request.Name,
            Description = request.Description,
            Slug = slug,
            Columns = request.Columns,
            Visibility = request.Visibility,
            IsDefault = request.IsDefault,
            RefreshIntervalSeconds = request.RefreshIntervalSeconds,
            Theme = request.Theme,
            DefaultCustomerId = request.DefaultCustomerId
        };
        
        await _unitOfWork.Dashboards.AddAsync(dashboard);
        
        // If this is set as default, unset other defaults
        if (request.IsDefault)
        {
            await _unitOfWork.Dashboards.UnsetDefaultAsync(organizationId, userId, dashboard.Id);
        }
        
        await _unitOfWork.SaveChangesAsync();
        
        _logger.LogInformation("Dashboard created: {DashboardName} ({DashboardId})", dashboard.Name, dashboard.Id);
        
        return await GetByIdAsync(dashboard.Id, organizationId, userId);
    }
    
    public async Task<DashboardDetailDto> UpdateAsync(Guid id, Guid organizationId, UpdateDashboardRequest request)
    {
        var dashboard = await _unitOfWork.Dashboards.GetByIdAsync(id);
        
        if (dashboard == null || dashboard.OrganizationId != organizationId)
            throw new NotFoundException($"Dashboard {id} not found");
        
        dashboard.Name = request.Name;
        dashboard.Description = request.Description;
        dashboard.Columns = request.Columns;
        dashboard.Visibility = request.Visibility;
        dashboard.IsDefault = request.IsDefault;
        dashboard.RefreshIntervalSeconds = request.RefreshIntervalSeconds;
        dashboard.Theme = request.Theme;
        dashboard.DefaultCustomerId = request.DefaultCustomerId;
        
        _unitOfWork.Dashboards.Update(dashboard);
        await _unitOfWork.SaveChangesAsync();
        
        return await GetByIdAsync(id, organizationId, dashboard.UserId ?? Guid.Empty);
    }
    
    public async Task DeleteAsync(Guid id, Guid organizationId)
    {
        var dashboard = await _unitOfWork.Dashboards.GetByIdAsync(id);
        
        if (dashboard == null || dashboard.OrganizationId != organizationId)
            throw new NotFoundException($"Dashboard {id} not found");
        
        // Delete all widgets first
        var widgets = await _unitOfWork.DashboardWidgets.GetByDashboardAsync(id);
        foreach (var widget in widgets)
        {
            _unitOfWork.DashboardWidgets.Delete(widget);
        }
        
        _unitOfWork.Dashboards.Delete(dashboard);
        await _unitOfWork.SaveChangesAsync();
        
        _logger.LogInformation("Dashboard deleted: {DashboardId}", id);
    }
    
    public async Task SetDefaultAsync(Guid id, Guid organizationId, Guid userId)
    {
        var dashboard = await _unitOfWork.Dashboards.GetByIdAsync(id);
        
        if (dashboard == null || dashboard.OrganizationId != organizationId)
            throw new NotFoundException($"Dashboard {id} not found");
        
        await _unitOfWork.Dashboards.UnsetDefaultAsync(organizationId, userId, null);
        
        dashboard.IsDefault = true;
        _unitOfWork.Dashboards.Update(dashboard);
        await _unitOfWork.SaveChangesAsync();
    }
    
    public async Task<DashboardDetailDto?> GetDefaultDashboardAsync(Guid organizationId, Guid userId)
    {
        var dashboard = await _unitOfWork.Dashboards.GetDefaultAsync(organizationId, userId);
        
        if (dashboard == null)
            return null;
        
        return await GetByIdAsync(dashboard.Id, organizationId, userId);
    }
    
    #region Widget Methods
    
    public async Task<DashboardWidgetDto> AddWidgetAsync(Guid dashboardId, Guid organizationId, CreateWidgetRequest request)
    {
        var dashboard = await _unitOfWork.Dashboards.GetByIdAsync(dashboardId);
        
        if (dashboard == null || dashboard.OrganizationId != organizationId)
            throw new NotFoundException($"Dashboard {dashboardId} not found");
        
        var widget = new DashboardWidget
        {
            DashboardId = dashboardId,
            Title = request.Title,
            Type = request.Type,
            PositionX = request.PositionX,
            PositionY = request.PositionY,
            Width = request.Width,
            Height = request.Height,
            Configuration = request.Configuration != null 
                ? System.Text.Json.JsonSerializer.Serialize(request.Configuration) 
                : "{}",
            CustomerId = request.CustomerId,
            HostId = request.HostId,
            CheckId = request.CheckId,
            Tags = request.Tags != null 
                ? System.Text.Json.JsonSerializer.Serialize(request.Tags) 
                : null,
            TimeRange = request.TimeRange,
            ShowTitle = request.ShowTitle,
            ShowLegend = request.ShowLegend,
            Thresholds = request.Thresholds != null 
                ? System.Text.Json.JsonSerializer.Serialize(request.Thresholds) 
                : null
        };
        
        await _unitOfWork.DashboardWidgets.AddAsync(widget);
        await _unitOfWork.SaveChangesAsync();
        
        return MapToWidgetDto(widget);
    }
    
    public async Task<DashboardWidgetDto> UpdateWidgetAsync(Guid widgetId, Guid organizationId, UpdateWidgetRequest request)
    {
        var widget = await _unitOfWork.DashboardWidgets.GetByIdAsync(widgetId);
        
        if (widget == null)
            throw new NotFoundException($"Widget {widgetId} not found");
        
        var dashboard = await _unitOfWork.Dashboards.GetByIdAsync(widget.DashboardId);
        if (dashboard?.OrganizationId != organizationId)
            throw new NotFoundException($"Widget {widgetId} not found");
        
        widget.Title = request.Title;
        widget.Type = request.Type;
        widget.PositionX = request.PositionX;
        widget.PositionY = request.PositionY;
        widget.Width = request.Width;
        widget.Height = request.Height;
        widget.Configuration = request.Configuration != null 
            ? System.Text.Json.JsonSerializer.Serialize(request.Configuration) 
            : "{}";
        widget.CustomerId = request.CustomerId;
        widget.HostId = request.HostId;
        widget.CheckId = request.CheckId;
        widget.Tags = request.Tags != null 
            ? System.Text.Json.JsonSerializer.Serialize(request.Tags) 
            : null;
        widget.TimeRange = request.TimeRange;
        widget.ShowTitle = request.ShowTitle;
        widget.ShowLegend = request.ShowLegend;
        widget.Thresholds = request.Thresholds != null 
            ? System.Text.Json.JsonSerializer.Serialize(request.Thresholds) 
            : null;
        
        _unitOfWork.DashboardWidgets.Update(widget);
        await _unitOfWork.SaveChangesAsync();
        
        return MapToWidgetDto(widget);
    }
    
    public async Task DeleteWidgetAsync(Guid widgetId, Guid organizationId)
    {
        var widget = await _unitOfWork.DashboardWidgets.GetByIdAsync(widgetId);
        
        if (widget == null)
            throw new NotFoundException($"Widget {widgetId} not found");
        
        var dashboard = await _unitOfWork.Dashboards.GetByIdAsync(widget.DashboardId);
        if (dashboard?.OrganizationId != organizationId)
            throw new NotFoundException($"Widget {widgetId} not found");
        
        _unitOfWork.DashboardWidgets.Delete(widget);
        await _unitOfWork.SaveChangesAsync();
    }
    
    public async Task UpdateWidgetPositionsAsync(Guid dashboardId, Guid organizationId, BatchUpdateWidgetPositionsRequest request)
    {
        var dashboard = await _unitOfWork.Dashboards.GetByIdAsync(dashboardId);
        
        if (dashboard == null || dashboard.OrganizationId != organizationId)
            throw new NotFoundException($"Dashboard {dashboardId} not found");
        
        foreach (var position in request.Positions)
        {
            var widget = await _unitOfWork.DashboardWidgets.GetByIdAsync(position.WidgetId);
            if (widget != null && widget.DashboardId == dashboardId)
            {
                widget.PositionX = position.PositionX;
                widget.PositionY = position.PositionY;
                widget.Width = position.Width;
                widget.Height = position.Height;
                _unitOfWork.DashboardWidgets.Update(widget);
            }
        }
        
        await _unitOfWork.SaveChangesAsync();
    }
    
    #endregion
    
    #region Widget Data Methods
    
    public async Task<object> GetWidgetDataAsync(Guid widgetId, Guid organizationId)
    {
        var widget = await _unitOfWork.DashboardWidgets.GetByIdAsync(widgetId);
        
        if (widget == null)
            throw new NotFoundException($"Widget {widgetId} not found");
        
        var dashboard = await _unitOfWork.Dashboards.GetByIdAsync(widget.DashboardId);
        if (dashboard?.OrganizationId != organizationId)
            throw new NotFoundException($"Widget {widgetId} not found");
        
        var (from, to) = widget.GetTimeRange();
        
        return widget.Type switch
        {
            WidgetType.StatusOverview => await GetStatusOverviewAsync(organizationId, widget.CustomerId),
            WidgetType.HostStatusGrid => await GetHostStatusGridAsync(organizationId, widget.CustomerId),
            WidgetType.CpuChart when widget.HostId.HasValue => await GetMetricChartDataAsync(widget.HostId.Value, "cpu", from, to),
            WidgetType.MemoryChart when widget.HostId.HasValue => await GetMetricChartDataAsync(widget.HostId.Value, "memory", from, to),
            WidgetType.DiskChart when widget.HostId.HasValue => await GetMetricChartDataAsync(widget.HostId.Value, "disk", from, to),
            WidgetType.UptimeGauge when widget.HostId.HasValue => await GetUptimeDataAsync("Host", widget.HostId.Value, from, to),
            WidgetType.UptimeGauge when widget.CheckId.HasValue => await GetUptimeDataAsync("Check", widget.CheckId.Value, from, to),
            WidgetType.AvailabilityHeatmap => await GetAvailabilityHeatmapAsync(organizationId, from, to, widget.CustomerId),
            WidgetType.HostCount => await GetHostCountAsync(organizationId, widget.CustomerId),
            WidgetType.IncidentCount => await GetIncidentCountAsync(organizationId, widget.CustomerId),
            WidgetType.TopHostsByCpu => await GetTopHostsByCpuAsync(organizationId, widget.CustomerId),
            WidgetType.TopHostsByMemory => await GetTopHostsByMemoryAsync(organizationId, widget.CustomerId),
            WidgetType.RecentIncidents => await GetRecentIncidentsAsync(organizationId, widget.CustomerId),
            WidgetType.FailingChecks => await GetFailingChecksAsync(organizationId, widget.CustomerId),
            WidgetType.ExpiringCertificates => await GetExpiringCertificatesAsync(organizationId),
            _ => new { message = "Widget type not supported" }
        };
    }
    
    public async Task<StatusOverviewData> GetStatusOverviewAsync(Guid organizationId, Guid? customerId = null)
    {
        var hostCounts = await _unitOfWork.Hosts.GetStatusCountsAsync(organizationId, customerId);
        var serviceCounts = await _unitOfWork.Services.GetStatusCountsAsync(organizationId);
        var checkCounts = await _unitOfWork.Checks.GetStatusCountsAsync(organizationId);
        var incidentCounts = await _unitOfWork.Incidents.GetSeverityCountsAsync(organizationId, onlyOpen: true);
        
        var totalHosts = hostCounts.Values.Sum();
        var hostsUp = hostCounts.GetValueOrDefault(StatusType.Up, 0);
        
        return new StatusOverviewData
        {
            TotalHosts = totalHosts,
            HostsUp = hostsUp,
            HostsDown = hostCounts.GetValueOrDefault(StatusType.Down, 0),
            HostsWarning = hostCounts.GetValueOrDefault(StatusType.Warning, 0),
            HostsInMaintenance = hostCounts.GetValueOrDefault(StatusType.Maintenance, 0),
            
            TotalServices = serviceCounts.Values.Sum(),
            ServicesUp = serviceCounts.GetValueOrDefault(StatusType.Up, 0),
            ServicesDown = serviceCounts.GetValueOrDefault(StatusType.Down, 0),
            
            TotalChecks = checkCounts.Values.Sum(),
            ChecksPassing = checkCounts.GetValueOrDefault(StatusType.Up, 0),
            ChecksFailing = checkCounts.GetValueOrDefault(StatusType.Down, 0),
            
            OpenIncidents = incidentCounts.Values.Sum(),
            CriticalIncidents = incidentCounts.GetValueOrDefault(IncidentSeverity.Critical, 0),
            HighIncidents = incidentCounts.GetValueOrDefault(IncidentSeverity.High, 0),
            
            OverallHealth = totalHosts > 0 ? (decimal)hostsUp / totalHosts * 100 : 100
        };
    }
    
    public async Task<HostStatusGridData> GetHostStatusGridAsync(Guid organizationId, Guid? customerId = null, int limit = 50)
    {
        var hosts = await _unitOfWork.Hosts.GetForStatusGridAsync(organizationId, customerId, limit);
        
        return new HostStatusGridData
        {
            Hosts = hosts.Select(h => new HostStatusItem
            {
                Id = h.Id,
                Name = h.Name,
                Status = h.CurrentStatus,
                CpuPercent = h.CpuPercent,
                RamPercent = h.RamPercent,
                LastSeenAt = h.LastSeenAt,
                CustomerName = h.Customer?.Name,
                IsInMaintenance = h.IsInMaintenance()
            }).ToList()
        };
    }
    
    public async Task<MetricChartData> GetMetricChartDataAsync(Guid hostId, string metricType, DateTime from, DateTime to)
    {
        var metrics = await _unitOfWork.HostMetrics.GetByHostAsync(hostId, from, to);
        
        var dataPoints = metricType.ToLower() switch
        {
            "cpu" => metrics.Select(m => new MetricDataPoint { Timestamp = m.RecordedAt, Value = m.CpuPercent }).ToList(),
            "memory" or "ram" => metrics.Select(m => new MetricDataPoint { Timestamp = m.RecordedAt, Value = m.RamPercent }).ToList(),
            "disk" => metrics.Where(m => !string.IsNullOrEmpty(m.DiskInfo))
                            .Select(m => new MetricDataPoint { Timestamp = m.RecordedAt, Value = GetDiskUsageFromJson(m.DiskInfo) }).ToList(),
            _ => new List<MetricDataPoint>()
        };
        
        var values = dataPoints.Select(d => d.Value).ToList();
        
        return new MetricChartData
        {
            MetricName = metricType,
            Unit = "%",
            DataPoints = dataPoints,
            Summary = new MetricSummary
            {
                Current = values.LastOrDefault(),
                Average = values.Any() ? values.Average() : 0,
                Min = values.Any() ? values.Min() : 0,
                Max = values.Any() ? values.Max() : 0
            }
        };
    }
    
    public async Task<UptimeData> GetUptimeDataAsync(string sourceType, Guid sourceId, DateTime from, DateTime to)
    {
        decimal uptimePercent;
        string sourceName;
        
        if (sourceType == "Host")
        {
            var host = await _unitOfWork.Hosts.GetByIdAsync(sourceId);
            sourceName = host?.Name ?? "Unknown";
            uptimePercent = await _unitOfWork.HostMetrics.CalculateUptimeAsync(sourceId, from, to);
        }
        else if (sourceType == "Check")
        {
            var check = await _unitOfWork.Checks.GetByIdAsync(sourceId);
            sourceName = check?.Name ?? "Unknown";
            uptimePercent = await _unitOfWork.CheckResults.CalculateUptimeAsync(sourceId, from, to);
        }
        else
        {
            return new UptimeData();
        }
        
        var totalTime = to - from;
        var uptime = TimeSpan.FromTicks((long)(totalTime.Ticks * (double)uptimePercent / 100));
        var downtime = totalTime - uptime;
        
        return new UptimeData
        {
            SourceId = sourceId,
            SourceName = sourceName,
            SourceType = sourceType,
            UptimePercent = uptimePercent,
            TotalUptime = uptime,
            TotalDowntime = downtime
        };
    }
    
    public async Task<AvailabilityHeatmapData> GetAvailabilityHeatmapAsync(Guid organizationId, DateTime from, DateTime to, Guid? customerId = null)
    {
        var hosts = await _unitOfWork.Hosts.GetForStatusGridAsync(organizationId, customerId, 20);
        var rows = new List<HeatmapRow>();
        
        // Generate hourly time labels
        var timeLabels = new List<string>();
        var hours = (int)(to - from).TotalHours;
        for (int i = 0; i < Math.Min(hours, 24); i++)
        {
            timeLabels.Add(from.AddHours(i).ToString("HH:mm"));
        }
        
        foreach (var host in hosts)
        {
            var metrics = await _unitOfWork.HostMetrics.GetByHostAsync(host.Id, from, to);
            var cells = new List<HeatmapCell>();
            
            // Group by hour
            var groupedMetrics = metrics.GroupBy(m => new DateTime(m.RecordedAt.Year, m.RecordedAt.Month, m.RecordedAt.Day, m.RecordedAt.Hour, 0, 0));
            
            foreach (var group in groupedMetrics.OrderBy(g => g.Key))
            {
                var avgCpu = group.Average(m => m.CpuPercent);
                cells.Add(new HeatmapCell
                {
                    Timestamp = group.Key,
                    Value = avgCpu,
                    Status = avgCpu > 95 ? StatusType.Down : avgCpu > 80 ? StatusType.Warning : StatusType.Up,
                    Tooltip = $"CPU: {avgCpu:F1}%"
                });
            }
            
            rows.Add(new HeatmapRow
            {
                SourceId = host.Id,
                SourceName = host.Name,
                Cells = cells
            });
        }
        
        return new AvailabilityHeatmapData
        {
            Rows = rows,
            TimeLabels = timeLabels
        };
    }
    
    #endregion
    
    #region Private Helper Methods
    
    private async Task<CounterData> GetHostCountAsync(Guid organizationId, Guid? customerId)
    {
        var counts = await _unitOfWork.Hosts.GetStatusCountsAsync(organizationId, customerId);
        var total = counts.Values.Sum();
        var up = counts.GetValueOrDefault(StatusType.Up, 0);
        
        return new CounterData
        {
            Label = "Total Hosts",
            Value = total,
            Icon = "server",
            Color = up == total ? "green" : "yellow"
        };
    }
    
    private async Task<CounterData> GetIncidentCountAsync(Guid organizationId, Guid? customerId)
    {
        var counts = await _unitOfWork.Incidents.GetSeverityCountsAsync(organizationId, onlyOpen: true);
        var total = counts.Values.Sum();
        var critical = counts.GetValueOrDefault(IncidentSeverity.Critical, 0);
        
        return new CounterData
        {
            Label = "Open Incidents",
            Value = total,
            Icon = "alert-triangle",
            Color = critical > 0 ? "red" : total > 0 ? "yellow" : "green"
        };
    }
    
    private async Task<TableWidgetData<TopHostItem>> GetTopHostsByCpuAsync(Guid organizationId, Guid? customerId, int limit = 10)
    {
        var hosts = await _unitOfWork.Hosts.GetHighCpuHostsAsync(organizationId, customerId, limit);
        
        return new TableWidgetData<TopHostItem>
        {
            Rows = hosts.Select(h => new TopHostItem
            {
                Id = h.Id,
                Name = h.Name,
                Value = h.CpuPercent ?? 0,
                Unit = "%",
                Status = h.CurrentStatus,
                CustomerName = h.Customer?.Name
            }).ToList(),
            TotalCount = hosts.Count
        };
    }
    
    private async Task<TableWidgetData<TopHostItem>> GetTopHostsByMemoryAsync(Guid organizationId, Guid? customerId, int limit = 10)
    {
        var hosts = await _unitOfWork.Hosts.GetHighMemoryHostsAsync(organizationId, customerId, limit);
        
        return new TableWidgetData<TopHostItem>
        {
            Rows = hosts.Select(h => new TopHostItem
            {
                Id = h.Id,
                Name = h.Name,
                Value = h.RamPercent ?? 0,
                Unit = "%",
                Status = h.CurrentStatus,
                CustomerName = h.Customer?.Name
            }).ToList(),
            TotalCount = hosts.Count
        };
    }
    
    private async Task<TableWidgetData<RecentIncidentItem>> GetRecentIncidentsAsync(Guid organizationId, Guid? customerId, int limit = 10)
    {
        var incidents = await _unitOfWork.Incidents.GetRecentAsync(organizationId, customerId, limit);
        
        return new TableWidgetData<RecentIncidentItem>
        {
            Rows = incidents.Select(i => new RecentIncidentItem
            {
                Id = i.Id,
                IncidentId = i.GetIncidentId(),
                Title = i.Title,
                Severity = i.Severity,
                Status = i.Status,
                CreatedAt = i.CreatedAt,
                Duration = i.GetDuration()?.ToString(@"hh\:mm\:ss")
            }).ToList(),
            TotalCount = incidents.Count
        };
    }
    
    private async Task<TableWidgetData<FailingCheckItem>> GetFailingChecksAsync(Guid organizationId, Guid? customerId, int limit = 10)
    {
        var checks = await _unitOfWork.Checks.GetFailingAsync(organizationId, customerId, limit);
        
        return new TableWidgetData<FailingCheckItem>
        {
            Rows = checks.Select(c => new FailingCheckItem
            {
                Id = c.Id,
                Name = c.Name,
                Target = c.Target,
                ErrorMessage = c.LastError,
                FailureCount = c.CurrentFailureCount,
                LastCheckAt = c.LastCheckAt
            }).ToList(),
            TotalCount = checks.Count
        };
    }
    
    private async Task<TableWidgetData<ExpiringCertificateItem>> GetExpiringCertificatesAsync(Guid organizationId, int days = 30)
    {
        var checks = await _unitOfWork.Checks.GetExpiringCertificatesAsync(days);
        var filtered = checks.Where(c => c.OrganizationId == organizationId).ToList();
        
        return new TableWidgetData<ExpiringCertificateItem>
        {
            Rows = filtered.Select(c => new ExpiringCertificateItem
            {
                CheckId = c.Id,
                Name = c.Name,
                Target = c.Target,
                ExpiresAt = c.SslExpiresAt ?? DateTime.MaxValue,
                DaysUntilExpiry = c.SslExpiresAt.HasValue 
                    ? (int)(c.SslExpiresAt.Value - DateTime.UtcNow).TotalDays 
                    : 999,
                Status = GetCertificateStatus(c)
            }).OrderBy(c => c.DaysUntilExpiry).ToList(),
            TotalCount = filtered.Count
        };
    }
    
    private static string GenerateSlug(string name)
    {
        return name.ToLower()
            .Replace(" ", "-")
            .Replace("_", "-")
            .Replace(".", "-");
    }
    
    private static decimal GetDiskUsageFromJson(string? diskInfo)
    {
        if (string.IsNullOrEmpty(diskInfo)) return 0;
        
        try
        {
            var disks = System.Text.Json.JsonSerializer.Deserialize<List<dynamic>>(diskInfo);
            // Return average disk usage
            return 0; // Simplified - would need proper deserialization
        }
        catch
        {
            return 0;
        }
    }
    
    private static string GetCertificateStatus(Check check)
    {
        if (!check.SslExpiresAt.HasValue) return "Unknown";
        
        var days = (check.SslExpiresAt.Value - DateTime.UtcNow).TotalDays;
        
        if (days <= 0) return "Expired";
        if (days <= check.SslExpiryCriticalDays) return "Critical";
        if (days <= check.SslExpiryWarningDays) return "Warning";
        return "Valid";
    }
    
    private static DashboardWidgetDto MapToWidgetDto(DashboardWidget widget)
    {
        return new DashboardWidgetDto
        {
            Id = widget.Id,
            Title = widget.Title,
            Type = widget.Type,
            PositionX = widget.PositionX,
            PositionY = widget.PositionY,
            Width = widget.Width,
            Height = widget.Height,
            Configuration = !string.IsNullOrEmpty(widget.Configuration) 
                ? System.Text.Json.JsonSerializer.Deserialize<object>(widget.Configuration) 
                : null,
            CustomerId = widget.CustomerId,
            HostId = widget.HostId,
            CheckId = widget.CheckId,
            TimeRange = widget.TimeRange,
            ShowTitle = widget.ShowTitle,
            ShowLegend = widget.ShowLegend,
            SortOrder = widget.SortOrder
        };
    }
    
    #endregion
}

6.6 Report Generator Service
csharp// src/ERAMonitor.Infrastructure/Services/ReportGeneratorService.cs

using Microsoft.Extensions.Logging;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using ERAMonitor.Core.DTOs.Reports;
using ERAMonitor.Core.Entities;
using ERAMonitor.Core.Enums;
using ERAMonitor.Core.Interfaces.Repositories;

namespace ERAMonitor.Infrastructure.Services;

public interface IReportGeneratorService
{
    Task<byte[]> GeneratePdfReportAsync(Report report, DateTime from, DateTime to);
    Task<byte[]> GenerateExcelReportAsync(Report report, DateTime from, DateTime to);
}

public class ReportGeneratorService : IReportGeneratorService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDashboardService _dashboardService;
    private readonly ILogger<ReportGeneratorService> _logger;
    
    public ReportGeneratorService(
        IUnitOfWork unitOfWork,
        IDashboardService dashboardService,
        ILogger<ReportGeneratorService> logger)
    {
        _unitOfWork = unitOfWork;
        _dashboardService = dashboardService;
        _logger = logger;
    }
    
    public async Task<byte[]> GeneratePdfReportAsync(Report report, DateTime from, DateTime to)
    {
        // Gather report data
        var reportData = await GatherReportDataAsync(report, from, to);
        
        // Generate PDF using QuestPDF
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(40);
                page.DefaultTextStyle(x => x.FontSize(10));
                
                // Header
                page.Header().Element(c => ComposeHeader(c, report, from, to));
                
                // Content
                page.Content().Element(c => ComposeContent(c, report, reportData));
                
                // Footer
                page.Footer().Element(ComposeFooter);
            });
        });
        
        return document.GeneratePdf();
    }
    
    public async Task<byte[]> GenerateExcelReportAsync(Report report, DateTime from, DateTime to)
    {
        // Use ClosedXML or EPPlus to generate Excel
        // Simplified implementation
        throw new NotImplementedException("Excel generation to be implemented");
    }
    
    private async Task<ReportDataDto> GatherReportDataAsync(Report report, DateTime from, DateTime to)
    {
        var data = new ReportDataDto
        {
            ReportName = report.Name,
            ReportType = report.Type,
            FromDate = from,
            ToDate = to,
            GeneratedAt = DateTime.UtcNow
        };
        
        // Get status overview
        data.StatusOverview = await _dashboardService.GetStatusOverviewAsync(
            report.OrganizationId, report.CustomerId);
        
        // Get host data
        var hostIds = report.GetHostIds();
        if (hostIds.Any())
        {
            foreach (var hostId in hostIds)
            {
                var uptime = await _dashboardService.GetUptimeDataAsync("Host", hostId, from, to);
                data.HostUptimes.Add(uptime);
            }
        }
        else if (report.CustomerId.HasValue)
        {
            var hosts = await _unitOfWork.Hosts.GetByCustomerAsync(report.CustomerId.Value);
            foreach (var host in hosts.Take(20))
            {
                var uptime = await _dashboardService.GetUptimeDataAsync("Host", host.Id, from, to);
                data.HostUptimes.Add(uptime);
            }
        }
        
        // Get incident summary
        var incidents = await _unitOfWork.Incidents.GetByDateRangeAsync(
            report.OrganizationId, from, to, report.CustomerId);
        data.IncidentSummary = new IncidentSummaryDto
        {
            TotalIncidents = incidents.Count,
            CriticalIncidents = incidents.Count(i => i.Severity == IncidentSeverity.Critical),
            ResolvedIncidents = incidents.Count(i => i.Status == IncidentStatus.Resolved || i.Status == IncidentStatus.Closed),
            AvgResolutionTimeMinutes = incidents
                .Where(i => i.ResolvedAt.HasValue)
                .Select(i => (i.ResolvedAt!.Value - i.CreatedAt).TotalMinutes)
                .DefaultIfEmpty(0)
                .Average(),
            TopIncidents = incidents.OrderByDescending(i => i.Severity).Take(10).ToList()
        };
        
        return data;
    }
    
    private void ComposeHeader(IContainer container, Report report, DateTime from, DateTime to)
    {
        container.Row(row =>
        {
            row.RelativeItem().Column(col =>
            {
                col.Item().Text(report.CompanyName ?? "ERA Monitor")
                    .FontSize(20).Bold().FontColor(Colors.Blue.Darken2);
                col.Item().Text(report.Name).FontSize(14);
                col.Item().Text($"{from:yyyy-MM-dd} - {to:yyyy-MM-dd}").FontSize(10).FontColor(Colors.Grey.Darken1);
            });
            
            row.ConstantItem(100).Height(50).Placeholder();
        });
    }
    
    private void ComposeContent(IContainer container, Report report, ReportDataDto data)
    {
        container.PaddingVertical(20).Column(col =>
        {
            // Executive Summary
            col.Item().Element(c => ComposeExecutiveSummary(c, data));
            
            col.Item().PaddingTop(20);
            
            // Uptime Section
            if (data.HostUptimes.Any())
            {
                col.Item().Element(c => ComposeUptimeSection(c, data));
            }
            
            col.Item().PaddingTop(20);
            
            // Incident Section
            col.Item().Element(c => ComposeIncidentSection(c, data));
        });
    }
    
    private void ComposeExecutiveSummary(IContainer container, ReportDataDto data)
    {
        container.Column(col =>
        {
            col.Item().Text("Executive Summary").FontSize(14).Bold();
            col.Item().PaddingTop(10);
            
            col.Item().Row(row =>
            {
                row.RelativeItem().Border(1).Padding(10).Column(c =>
                {
                    c.Item().Text("Hosts").Bold();
                    c.Item().Text($"Total: {data.StatusOverview.TotalHosts}");
                    c.Item().Text($"Up: {data.StatusOverview.HostsUp}").FontColor(Colors.Green.Darken1);
                    c.Item().Text($"Down: {data.StatusOverview.HostsDown}").FontColor(Colors.Red.Darken1);
                });
                
                row.ConstantItem(10);
                
                row.RelativeItem().Border(1).Padding(10).Column(c =>
                {
                    c.Item().Text("Services").Bold();
                    c.Item().Text($"Total: {data.StatusOverview.TotalServices}");
                    c.Item().Text($"Up: {data.StatusOverview.ServicesUp}").FontColor(Colors.Green.Darken1);
                    c.Item().Text($"Down: {data.StatusOverview.ServicesDown}").FontColor(Colors.Red.Darken1);
                });
                
                row.ConstantItem(10);
                
                row.RelativeItem().Border(1).Padding(10).Column(c =>
                {
                    c.Item().Text("Incidents").Bold();
                    c.Item().Text($"Open: {data.StatusOverview.OpenIncidents}");
                    c.Item().Text($"Critical: {data.StatusOverview.CriticalIncidents}").FontColor(Colors.Red.Darken1);
                });
                
                row.ConstantItem(10);
                
                row.RelativeItem().Border(1).Padding(10).Column(c =>
                {
                    c.Item().Text("Overall Health").Bold();
                    c.Item().Text($"{data.StatusOverview.OverallHealth:F1}%").FontSize(16);
                });
            });
        });
    }
    
    private void ComposeUptimeSection(IContainer container, ReportDataDto data)
    {
        container.Column(col =>
        {
            col.Item().Text("Uptime Report").FontSize(14).Bold();
            col.Item().PaddingTop(10);
            
            col.Item().Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(3);
                    columns.RelativeColumn(1);
                    columns.RelativeColumn(2);
                    columns.RelativeColumn(2);
                });
                
                table.Header(header =>
                {
                    header.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Resource").Bold();
                    header.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Uptime %").Bold();
                    header.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Total Uptime").Bold();
                    header.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Downtime").Bold();
                });
                
                foreach (var uptime in data.HostUptimes)
                {
                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(uptime.SourceName);
                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text($"{uptime.UptimePercent:F2}%");
                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(FormatTimeSpan(uptime.TotalUptime));
                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(FormatTimeSpan(uptime.TotalDowntime));
                }
            });
        });
    }
    
    private void ComposeIncidentSection(IContainer container, ReportDataDto data)
    {
        container.Column(col =>
        {
            col.Item().Text("Incident Summary").FontSize(14).Bold();
            col.Item().PaddingTop(10);
            
            col.Item().Row(row =>
            {
                row.RelativeItem().Text($"Total Incidents: {data.IncidentSummary.TotalIncidents}");
                row.RelativeItem().Text($"Critical: {data.IncidentSummary.CriticalIncidents}");
                row.RelativeItem().Text($"Resolved: {data.IncidentSummary.ResolvedIncidents}");
                row.RelativeItem().Text($"Avg Resolution: {data.IncidentSummary.AvgResolutionTimeMinutes:F0} min");
            });
            
            if (data.IncidentSummary.TopIncidents.Any())
            {
                col.Item().PaddingTop(10);
                col.Item().Text("Top Incidents").Bold();
                col.Item().PaddingTop(5);
                
                col.Item().Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.ConstantColumn(80);
                        columns.RelativeColumn(3);
                        columns.RelativeColumn(1);
                        columns.RelativeColumn(1);
                    });
                    
                    table.Header(header =>
                    {
                        header.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("ID").Bold();
                        header.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Title").Bold();
                        header.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Severity").Bold();
                        header.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Status").Bold();
                    });
                    
                    foreach (var incident in data.IncidentSummary.TopIncidents)
                    {
                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(incident.GetIncidentId());
                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(incident.Title);
                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(incident.Severity.ToString());
                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(incident.Status.ToString());
                    }
                });
            }
        });
    }
    
    private void ComposeFooter(IContainer container)
    {
        container.Row(row =>
        {
            row.RelativeItem().Text(x =>
            {
                x.Span("Generated by ERA Monitor - ");
                x.Span(DateTime.UtRetry
                now.ToString("yyyy-MM-dd HH:mm UTC"));
});
        row.RelativeItem().AlignRight().Text(x =>
        {
            x.Span("Page ");
            x.CurrentPageNumber();
            x.Span(" of ");
            x.TotalPages();
        });
    });
}

private static string FormatTimeSpan(TimeSpan ts)
{
    if (ts.TotalDays >= 1)
        return $"{ts.Days}d {ts.Hours}h {ts.Minutes}m";
    if (ts.TotalHours >= 1)
        return $"{ts.Hours}h {ts.Minutes}m";
    return $"{ts.Minutes}m {ts.Seconds}s";
}
}
public class ReportDataDto
{
public string ReportName { get; set; } = string.Empty;
public ReportType ReportType { get; set; }
public DateTime FromDate { get; set; }
public DateTime ToDate { get; set; }
public DateTime GeneratedAt { get; set; }
public StatusOverviewData StatusOverview { get; set; } = new();
public List<UptimeData> HostUptimes { get; set; } = new();
public IncidentSummaryDto IncidentSummary { get; set; } = new();
}
public class IncidentSummaryDto
{
public int TotalIncidents { get; set; }
public int CriticalIncidents { get; set; }
public int ResolvedIncidents { get; set; }
public double AvgResolutionTimeMinutes { get; set; }
public List<Incident> TopIncidents { get; set; } = new();
}

---

## 6.7 Controllers
```csharp
// src/ERAMonitor.API/Controllers/DashboardsController.cs

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ERAMonitor.Core.DTOs.Common;
using ERAMonitor.Core.DTOs.Dashboard;
using ERAMonitor.Core.Interfaces.Services;
using ERAMonitor.API.Extensions;

namespace ERAMonitor.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DashboardsController : ControllerBase
{
    private readonly IDashboardService _dashboardService;
    
    public DashboardsController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }
    
    [HttpGet]
    public async Task<ActionResult<PagedResponse<DashboardDto>>> GetDashboards([FromQuery] PagedRequest request)
    {
        var organizationId = User.GetOrganizationId();
        var userId = User.GetUserId();
        var result = await _dashboardService.GetPagedAsync(organizationId, userId, request);
        return Ok(result);
    }
    
    [HttpGet("{id}")]
    public async Task<ActionResult<DashboardDetailDto>> GetDashboard(Guid id)
    {
        var organizationId = User.GetOrganizationId();
        var userId = User.GetUserId();
        var dashboard = await _dashboardService.GetByIdAsync(id, organizationId, userId);
        return Ok(dashboard);
    }
    
    [HttpGet("slug/{slug}")]
    public async Task<ActionResult<DashboardDetailDto>> GetDashboardBySlug(string slug)
    {
        var organizationId = User.GetOrganizationId();
        var userId = User.GetUserId();
        var dashboard = await _dashboardService.GetBySlugAsync(slug, organizationId, userId);
        return Ok(dashboard);
    }
    
    [HttpGet("default")]
    public async Task<ActionResult<DashboardDetailDto>> GetDefaultDashboard()
    {
        var organizationId = User.GetOrganizationId();
        var userId = User.GetUserId();
        var dashboard = await _dashboardService.GetDefaultDashboardAsync(organizationId, userId);
        
        if (dashboard == null)
            return NotFound("No default dashboard found");
        
        return Ok(dashboard);
    }
    
    [HttpPost]
    public async Task<ActionResult<DashboardDetailDto>> CreateDashboard([FromBody] CreateDashboardRequest request)
    {
        var organizationId = User.GetOrganizationId();
        var userId = User.GetUserId();
        var dashboard = await _dashboardService.CreateAsync(organizationId, userId, request);
        return CreatedAtAction(nameof(GetDashboard), new { id = dashboard.Id }, dashboard);
    }
    
    [HttpPut("{id}")]
    public async Task<ActionResult<DashboardDetailDto>> UpdateDashboard(Guid id, [FromBody] UpdateDashboardRequest request)
    {
        var organizationId = User.GetOrganizationId();
        var dashboard = await _dashboardService.UpdateAsync(id, organizationId, request);
        return Ok(dashboard);
    }
    
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteDashboard(Guid id)
    {
        var organizationId = User.GetOrganizationId();
        await _dashboardService.DeleteAsync(id, organizationId);
        return NoContent();
    }
    
    [HttpPost("{id}/set-default")]
    public async Task<ActionResult> SetDefault(Guid id)
    {
        var organizationId = User.GetOrganizationId();
        var userId = User.GetUserId();
        await _dashboardService.SetDefaultAsync(id, organizationId, userId);
        return Ok(ApiResponse.Ok("Dashboard set as default"));
    }
    
    // Widget endpoints
    [HttpPost("{dashboardId}/widgets")]
    public async Task<ActionResult<DashboardWidgetDto>> AddWidget(Guid dashboardId, [FromBody] CreateWidgetRequest request)
    {
        var organizationId = User.GetOrganizationId();
        var widget = await _dashboardService.AddWidgetAsync(dashboardId, organizationId, request);
        return Ok(widget);
    }
    
    [HttpPut("widgets/{widgetId}")]
    public async Task<ActionResult<DashboardWidgetDto>> UpdateWidget(Guid widgetId, [FromBody] UpdateWidgetRequest request)
    {
        var organizationId = User.GetOrganizationId();
        var widget = await _dashboardService.UpdateWidgetAsync(widgetId, organizationId, request);
        return Ok(widget);
    }
    
    [HttpDelete("widgets/{widgetId}")]
    public async Task<ActionResult> DeleteWidget(Guid widgetId)
    {
        var organizationId = User.GetOrganizationId();
        await _dashboardService.DeleteWidgetAsync(widgetId, organizationId);
        return NoContent();
    }
    
    [HttpPut("{dashboardId}/widgets/positions")]
    public async Task<ActionResult> UpdateWidgetPositions(Guid dashboardId, [FromBody] BatchUpdateWidgetPositionsRequest request)
    {
        var organizationId = User.GetOrganizationId();
        await _dashboardService.UpdateWidgetPositionsAsync(dashboardId, organizationId, request);
        return Ok(ApiResponse.Ok("Widget positions updated"));
    }
    
    [HttpGet("widgets/{widgetId}/data")]
    public async Task<ActionResult<object>> GetWidgetData(Guid widgetId)
    {
        var organizationId = User.GetOrganizationId();
        var data = await _dashboardService.GetWidgetDataAsync(widgetId, organizationId);
        return Ok(data);
    }
    
    // Quick data endpoints
    [HttpGet("status-overview")]
    public async Task<ActionResult<StatusOverviewData>> GetStatusOverview([FromQuery] Guid? customerId = null)
    {
        var organizationId = User.GetOrganizationId();
        var data = await _dashboardService.GetStatusOverviewAsync(organizationId, customerId);
        return Ok(data);
    }
    
    [HttpGet("host-status-grid")]
    public async Task<ActionResult<HostStatusGridData>> GetHostStatusGrid([FromQuery] Guid? customerId = null, [FromQuery] int limit = 50)
    {
        var organizationId = User.GetOrganizationId();
        var data = await _dashboardService.GetHostStatusGridAsync(organizationId, customerId, limit);
        return Ok(data);
    }
}
```
```csharp
// src/ERAMonitor.API/Controllers/ReportsController.cs

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ERAMonitor.Core.DTOs.Common;
using ERAMonitor.Core.DTOs.Reports;
using ERAMonitor.Core.Interfaces.Services;
using ERAMonitor.API.Extensions;

namespace ERAMonitor.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ReportsController : ControllerBase
{
    private readonly IReportService _reportService;
    
    public ReportsController(IReportService reportService)
    {
        _reportService = reportService;
    }
    
    [HttpGet]
    public async Task<ActionResult<PagedResponse<ReportDto>>> GetReports(
        [FromQuery] PagedRequest request,
        [FromQuery] ReportType? type = null)
    {
        var organizationId = User.GetOrganizationId();
        var result = await _reportService.GetPagedAsync(organizationId, request, type);
        return Ok(result);
    }
    
    [HttpGet("{id}")]
    public async Task<ActionResult<ReportDetailDto>> GetReport(Guid id)
    {
        var organizationId = User.GetOrganizationId();
        var report = await _reportService.GetByIdAsync(id, organizationId);
        return Ok(report);
    }
    
    [HttpPost]
    [Authorize(Policy = "RequireOperatorRole")]
    public async Task<ActionResult<ReportDetailDto>> CreateReport([FromBody] CreateReportRequest request)
    {
        var organizationId = User.GetOrganizationId();
        var report = await _reportService.CreateAsync(organizationId, request);
        return CreatedAtAction(nameof(GetReport), new { id = report.Id }, report);
    }
    
    [HttpPut("{id}")]
    [Authorize(Policy = "RequireOperatorRole")]
    public async Task<ActionResult<ReportDetailDto>> UpdateReport(Guid id, [FromBody] UpdateReportRequest request)
    {
        var organizationId = User.GetOrganizationId();
        var report = await _reportService.UpdateAsync(id, organizationId, request);
        return Ok(report);
    }
    
    [HttpDelete("{id}")]
    [Authorize(Policy = "RequireAdminRole")]
    public async Task<ActionResult> DeleteReport(Guid id)
    {
        var organizationId = User.GetOrganizationId();
        await _reportService.DeleteAsync(id, organizationId);
        return NoContent();
    }
    
    [HttpPost("{id}/generate")]
    [Authorize(Policy = "RequireOperatorRole")]
    public async Task<ActionResult<ReportExecutionDto>> GenerateReport(Guid id, [FromBody] GenerateReportRequest? request = null)
    {
        var organizationId = User.GetOrganizationId();
        var userId = User.GetUserId();
        var execution = await _reportService.GenerateReportAsync(id, organizationId, userId, request);
        return Ok(execution);
    }
    
    [HttpGet("{id}/executions")]
    public async Task<ActionResult<List<ReportExecutionDto>>> GetExecutions(Guid id, [FromQuery] int limit = 10)
    {
        var organizationId = User.GetOrganizationId();
        var executions = await _reportService.GetExecutionsAsync(id, organizationId, limit);
        return Ok(executions);
    }
    
    [HttpGet("executions/{executionId}")]
    public async Task<ActionResult<ReportExecutionDto>> GetExecution(Guid executionId)
    {
        var organizationId = User.GetOrganizationId();
        var execution = await _reportService.GetExecutionAsync(executionId, organizationId);
        return Ok(execution);
    }
    
    [HttpGet("executions/{executionId}/download")]
    public async Task<IActionResult> DownloadReport(Guid executionId)
    {
        var organizationId = User.GetOrganizationId();
        var execution = await _reportService.GetExecutionAsync(executionId, organizationId);
        var bytes = await _reportService.DownloadReportAsync(executionId, organizationId);
        
        var contentType = execution.Format switch
        {
            ReportFormat.PDF => "application/pdf",
            ReportFormat.Excel => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            ReportFormat.HTML => "text/html",
            _ => "application/octet-stream"
        };
        
        var extension = execution.Format switch
        {
            ReportFormat.PDF => "pdf",
            ReportFormat.Excel => "xlsx",
            ReportFormat.HTML => "html",
            _ => "bin"
        };
        
        return File(bytes, contentType, $"report-{executionId}.{extension}");
    }
    
    [HttpPost("{id}/toggle-active")]
    [Authorize(Policy = "RequireOperatorRole")]
    public async Task<ActionResult<ApiResponse<bool>>> ToggleActive(Guid id)
    {
        var organizationId = User.GetOrganizationId();
        var isActive = await _reportService.ToggleActiveAsync(id, organizationId);
        return Ok(ApiResponse<bool>.Ok(isActive, isActive ? "Report activated" : "Report deactivated"));
    }
}
```
```csharp
// src/ERAMonitor.API/Controllers/StatusPagesController.cs

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ERAMonitor.Core.DTOs.Common;
using ERAMonitor.Core.DTOs.StatusPage;
using ERAMonitor.Core.Interfaces.Services;
using ERAMonitor.API.Extensions;

namespace ERAMonitor.API.Controllers;

[ApiController]
[Route("api/status-pages")]
[Authorize]
public class StatusPagesController : ControllerBase
{
    private readonly IStatusPageService _statusPageService;
    
    public StatusPagesController(IStatusPageService statusPageService)
    {
        _statusPageService = statusPageService;
    }
    
    [HttpGet]
    public async Task<ActionResult<PagedResponse<StatusPageDto>>> GetStatusPages([FromQuery] PagedRequest request)
    {
        var organizationId = User.GetOrganizationId();
        var result = await _statusPageService.GetPagedAsync(organizationId, request);
        return Ok(result);
    }
    
    [HttpGet("{id}")]
    public async Task<ActionResult<StatusPageDetailDto>> GetStatusPage(Guid id)
    {
        var organizationId = User.GetOrganizationId();
        var statusPage = await _statusPageService.GetByIdAsync(id, organizationId);
        return Ok(statusPage);
    }
    
    [HttpPost]
    [Authorize(Policy = "RequireAdminRole")]
    public async Task<ActionResult<StatusPageDetailDto>> CreateStatusPage([FromBody] CreateStatusPageRequest request)
    {
        var organizationId = User.GetOrganizationId();
        var statusPage = await _statusPageService.CreateAsync(organizationId, request);
        return CreatedAtAction(nameof(GetStatusPage), new { id = statusPage.Id }, statusPage);
    }
    
    [HttpPut("{id}")]
    [Authorize(Policy = "RequireAdminRole")]
    public async Task<ActionResult<StatusPageDetailDto>> UpdateStatusPage(Guid id, [FromBody] UpdateStatusPageRequest request)
    {
        var organizationId = User.GetOrganizationId();
        var statusPage = await _statusPageService.UpdateAsync(id, organizationId, request);
        return Ok(statusPage);
    }
    
    [HttpDelete("{id}")]
    [Authorize(Policy = "RequireAdminRole")]
    public async Task<ActionResult> DeleteStatusPage(Guid id)
    {
        var organizationId = User.GetOrganizationId();
        await _statusPageService.DeleteAsync(id, organizationId);
        return NoContent();
    }
    
    // Components
    [HttpPost("{id}/components")]
    [Authorize(Policy = "RequireOperatorRole")]
    public async Task<ActionResult<StatusPageComponentDto>> AddComponent(Guid id, [FromBody] AddComponentRequest request)
    {
        var organizationId = User.GetOrganizationId();
        var component = await _statusPageService.AddComponentAsync(id, organizationId, request);
        return Ok(component);
    }
    
    [HttpPut("components/{componentId}")]
    [Authorize(Policy = "RequireOperatorRole")]
    public async Task<ActionResult<StatusPageComponentDto>> UpdateComponent(Guid componentId, [FromBody] AddComponentRequest request)
    {
        var organizationId = User.GetOrganizationId();
        var component = await _statusPageService.UpdateComponentAsync(componentId, organizationId, request);
        return Ok(component);
    }
    
    [HttpDelete("components/{componentId}")]
    [Authorize(Policy = "RequireOperatorRole")]
    public async Task<ActionResult> DeleteComponent(Guid componentId)
    {
        var organizationId = User.GetOrganizationId();
        await _statusPageService.DeleteComponentAsync(componentId, organizationId);
        return NoContent();
    }
    
    [HttpPost("components/{componentId}/status")]
    [Authorize(Policy = "RequireOperatorRole")]
    public async Task<ActionResult> UpdateComponentStatus(Guid componentId, [FromBody] UpdateComponentStatusRequest request)
    {
        var organizationId = User.GetOrganizationId();
        await _statusPageService.UpdateComponentStatusAsync(componentId, organizationId, request);
        return Ok(ApiResponse.Ok("Component status updated"));
    }
}

// Public endpoints (no auth required)
[ApiController]
[Route("api/public/status")]
public class PublicStatusController : ControllerBase
{
    private readonly IStatusPageService _statusPageService;
    
    public PublicStatusController(IStatusPageService statusPageService)
    {
        _statusPageService = statusPageService;
    }
    
    [HttpGet("{slug}")]
    public async Task<ActionResult<PublicStatusPageDto>> GetPublicStatus(string slug, [FromQuery] string? password = null)
    {
        var statusPage = await _statusPageService.GetPublicStatusPageAsync(slug, password);
        return Ok(statusPage);
    }
    
    [HttpPost("{slug}/subscribe")]
    public async Task<ActionResult<ApiResponse>> Subscribe(string slug, [FromBody] SubscribeRequest request)
    {
        var success = await _statusPageService.SubscribeAsync(slug, request);
        return Ok(ApiResponse.Ok(success ? "Subscription successful. Please check your email to verify." : "Subscription failed"));
    }
    
    [HttpGet("verify/{token}")]
    public async Task<ActionResult<ApiResponse>> VerifySubscription(string token)
    {
        var success = await _statusPageService.VerifySubscriptionAsync(token);
        return Ok(ApiResponse.Ok(success ? "Email verified successfully" : "Verification failed"));
    }
    
    [HttpGet("unsubscribe/{token}")]
    public async Task<ActionResult<ApiResponse>> Unsubscribe(string token)
    {
        var success = await _statusPageService.UnsubscribeAsync(token);
        return Ok(ApiResponse.Ok(success ? "Unsubscribed successfully" : "Unsubscribe failed"));
    }
}
```

---

## 6.8 Phase 6 Checklist
```markdown
# Phase 6 Completion Checklist

## Entities
- [ ] Dashboard with visibility settings
- [ ] DashboardWidget with position and configuration
- [ ] Report with scheduling
- [ ] ReportExecution for tracking
- [ ] StatusPage for public status
- [ ] StatusPageComponent for status page items
- [ ] StatusPageComponentGroup for grouping
- [ ] StatusPageSubscriber for notifications

## DTOs

Dashboard:
- [ ] DashboardDto, DashboardDetailDto
- [ ] DashboardWidgetDto
- [ ] CreateDashboardRequest, UpdateDashboardRequest
- [ ] CreateWidgetRequest, UpdateWidgetRequest
- [ ] BatchUpdateWidgetPositionsRequest

Widget Data:
- [ ] StatusOverviewData
- [ ] HostStatusGridData
- [ ] MetricChartData
- [ ] UptimeData
- [ ] AvailabilityHeatmapData
- [ ] CounterData
- [ ] TableWidgetData

Report:
- [ ] ReportDto, ReportDetailDto
- [ ] ReportExecutionDto
- [ ] CreateReportRequest, UpdateReportRequest
- [ ] GenerateReportRequest

Status Page:
- [ ] StatusPageDto, StatusPageDetailDto
- [ ] StatusPageComponentDto
- [ ] PublicStatusPageDto
- [ ] SubscribeRequest

## Services
- [ ] IDashboardService with widget management
- [ ] IReportService with scheduling
- [ ] IReportGeneratorService (PDF/Excel)
- [ ] IStatusPageService

## Controllers
- [ ] DashboardsController
- [ ] ReportsController
- [ ] StatusPagesController
- [ ] PublicStatusController

## API Endpoints

Dashboards:
- [ ] GET /api/dashboards
- [ ] GET /api/dashboards/{id}
- [ ] GET /api/dashboards/slug/{slug}
- [ ] GET /api/dashboards/default
- [ ] POST /api/dashboards
- [ ] PUT /api/dashboards/{id}
- [ ] DELETE /api/dashboards/{id}
- [ ] POST /api/dashboards/{id}/set-default
- [ ] POST /api/dashboards/{id}/widgets
- [ ] PUT /api/dashboards/widgets/{id}
- [ ] DELETE /api/dashboards/widgets/{id}
- [ ] PUT /api/dashboards/{id}/widgets/positions
- [ ] GET /api/dashboards/widgets/{id}/data
- [ ] GET /api/dashboards/status-overview
- [ ] GET /api/dashboards/host-status-grid

Reports:
- [ ] GET /api/reports
- [ ] GET /api/reports/{id}
- [ ] POST /api/reports
- [ ] PUT /api/reports/{id}
- [ ] DELETE /api/reports/{id}
- [ ] POST /api/reports/{id}/generate
- [ ] GET /api/reports/{id}/executions
- [ ] GET /api/reports/executions/{id}
- [ ] GET /api/reports/executions/{id}/download
- [ ] POST /api/reports/{id}/toggle-active

Status Pages:
- [ ] GET /api/status-pages
- [ ] GET /api/status-pages/{id}
- [ ] POST /api/status-pages
- [ ] PUT /api/status-pages/{id}
- [ ] DELETE /api/status-pages/{id}
- [ ] POST /api/status-pages/{id}/components
- [ ] PUT /api/status-pages/components/{id}
- [ ] DELETE /api/status-pages/components/{id}
- [ ] POST /api/status-pages/components/{id}/status

Public Status:
- [ ] GET /api/public/status/{slug}
- [ ] POST /api/public/status/{slug}/subscribe
- [ ] GET /api/public/status/verify/{token}
- [ ] GET /api/public/status/unsubscribe/{token}

## Features
- [ ] Customizable dashboard layouts
- [ ] Drag-and-drop widget positioning
- [ ] Multiple widget types (charts, tables, counters)
- [ ] Real-time data refresh
- [ ] Scheduled report generation
- [ ] PDF report export
- [ ] Excel report export
- [ ] Email report delivery
- [ ] Public status pages
- [ ] Status page subscriptions
- [ ] Uptime visualization

## Background Jobs
- [ ] ReportSchedulerJob
- [ ] ReportCleanupJob
- [ ] StatusPageUpdateJob

## Testing
- [ ] Dashboard service unit tests
- [ ] Widget data retrieval tests
- [ ] Report generation tests
- [ ] Status page public access tests
```