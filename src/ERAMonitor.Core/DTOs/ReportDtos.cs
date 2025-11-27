using System.ComponentModel.DataAnnotations;
using ERAMonitor.Core.Enums;

namespace ERAMonitor.Core.DTOs.Reports;

public class ReportDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public ReportType Type { get; set; }
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
    public string Timezone { get; set; } = "Europe/Istanbul";
    public DateTime? NextRunAt { get; set; }
    public DateTime? LastRunAt { get; set; }
    
    // Filters
    public Guid? CustomerId { get; set; }
    public string? CustomerName { get; set; }
    public List<Guid> HostIds { get; set; } = new();
    
    // Time range
    public ReportTimeRange TimeRange { get; set; }
    public DateTime? CustomStartDate { get; set; }
    public DateTime? CustomEndDate { get; set; }
    
    // Output
    public ReportFormat Format { get; set; }
    public bool SendEmail { get; set; }
    public List<string> EmailRecipients { get; set; } = new();
    public bool SaveToStorage { get; set; }
    
    // Branding
    public string? LogoUrl { get; set; }
    public string? CompanyName { get; set; }
    
    public bool IsActive { get; set; }
    public List<ReportExecutionDto> RecentExecutions { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class ReportExecutionDto
{
    public Guid Id { get; set; }
    public Guid ReportId { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public ReportExecutionStatus Status { get; set; }
    public DateTime DataFromDate { get; set; }
    public DateTime DataToDate { get; set; }
    public ReportFormat Format { get; set; }
    public string? FileUrl { get; set; }
    public long? FileSizeBytes { get; set; }
    public string? ErrorMessage { get; set; }
    public bool EmailSent { get; set; }
    public bool IsManual { get; set; }
    public Guid? TriggeredByUserId { get; set; }
    public string? TriggeredByUserName { get; set; }
}

public class CreateReportDto
{
    [Required]
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public ReportType Type { get; set; }
    
    public bool IsScheduled { get; set; }
    public ReportSchedule? Schedule { get; set; }
    public string? CronExpression { get; set; }
    public string? Timezone { get; set; }
    
    public Guid? CustomerId { get; set; }
    public string? HostIds { get; set; } // JSON
    public string? CheckIds { get; set; } // JSON
    public string? Tags { get; set; } // JSON
    
    public ReportTimeRange TimeRange { get; set; }
    public DateTime? CustomStartDate { get; set; }
    public DateTime? CustomEndDate { get; set; }
    
    public ReportFormat Format { get; set; }
    public string? Sections { get; set; } // JSON
    public bool SendEmail { get; set; }
    public string? EmailRecipients { get; set; } // JSON
    public bool SaveToStorage { get; set; }
    
    public string? LogoUrl { get; set; }
    public string? CompanyName { get; set; }
    public string? PrimaryColor { get; set; }
    public bool IsActive { get; set; }
}

public class UpdateReportDto : CreateReportDto
{
}
