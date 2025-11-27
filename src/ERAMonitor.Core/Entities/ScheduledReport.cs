using ERAMonitor.Core.Enums;

namespace ERAMonitor.Core.Entities;

public class ScheduledReport : BaseEntity
{
    public Guid OrganizationId { get; set; }
    public Organization Organization { get; set; } = null!;
    public Guid? CustomerId { get; set; }
    public Customer? Customer { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ReportType { get; set; } = string.Empty;
    public string? Parameters { get; set; } // JSONB
    public string? ScheduleCron { get; set; }
    public string ScheduleTimezone { get; set; } = "UTC";
    public string[] DeliveryEmails { get; set; } = Array.Empty<string>();
    public string? DeliveryWebhookUrl { get; set; }
    public DateTime? LastRunAt { get; set; }
    public DateTime? NextRunAt { get; set; }
    public bool IsActive { get; set; } = true;
}
