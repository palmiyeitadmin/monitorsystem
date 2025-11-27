using ERAMonitor.Core.Enums;

namespace ERAMonitor.Core.Entities;

public class SystemSetting : BaseEntity
{
    public Guid OrganizationId { get; set; }
    public Organization Organization { get; set; } = null!;
    public string? SmtpHost { get; set; }
    public int SmtpPort { get; set; } = 587;
    public string? SmtpUsername { get; set; }
    public string? SmtpPasswordEncrypted { get; set; }
    public string? SmtpFromEmail { get; set; }
    public string? SmtpFromName { get; set; }
    public bool SmtpUseSsl { get; set; } = true;
    public string? TelegramBotTokenEncrypted { get; set; }
    public int DefaultCheckIntervalSeconds { get; set; } = 60;
    public int DefaultAlertDelaySeconds { get; set; } = 60;
    public int RetentionDaysMetrics { get; set; } = 30;
    public int RetentionDaysLogs { get; set; } = 90;
}
