using ERAMonitor.Core.Enums;

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
