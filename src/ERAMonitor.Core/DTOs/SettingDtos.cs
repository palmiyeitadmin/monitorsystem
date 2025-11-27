using System.ComponentModel.DataAnnotations;

namespace ERAMonitor.Core.DTOs;

public class SystemSettingDto
{
    public Guid Id { get; set; }
    public string? SmtpHost { get; set; }
    public int SmtpPort { get; set; }
    public string? SmtpUsername { get; set; }
    public string? SmtpFromEmail { get; set; }
    public string? SmtpFromName { get; set; }
    public bool SmtpUseSsl { get; set; }
    public bool HasSmtpPassword { get; set; }
    public bool HasTelegramBotToken { get; set; }
    public int DefaultCheckIntervalSeconds { get; set; }
    public int DefaultAlertDelaySeconds { get; set; }
    public int RetentionDaysMetrics { get; set; }
    public int RetentionDaysLogs { get; set; }
}

public class UpdateSystemSettingRequest
{
    public string? SmtpHost { get; set; }
    public int SmtpPort { get; set; }
    public string? SmtpUsername { get; set; }
    public string? SmtpPassword { get; set; } // Optional, only if changing
    public string? SmtpFromEmail { get; set; }
    public string? SmtpFromName { get; set; }
    public bool SmtpUseSsl { get; set; }
    public string? TelegramBotToken { get; set; } // Optional, only if changing
    public int DefaultCheckIntervalSeconds { get; set; }
    public int DefaultAlertDelaySeconds { get; set; }
    public int RetentionDaysMetrics { get; set; }
    public int RetentionDaysLogs { get; set; }
}
