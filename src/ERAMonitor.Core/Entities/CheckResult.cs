using ERAMonitor.Core.Enums;

namespace ERAMonitor.Core.Entities;

public class CheckResult : BaseEntity
{
    public Guid CheckId { get; set; }
    public Check Check { get; set; } = null!;
    public StatusType Status { get; set; }
    public int? ResponseTimeMs { get; set; }
    public int? StatusCode { get; set; }
    public string? ErrorMessage { get; set; }
    public string? ResponseBodyPreview { get; set; }
    public DateTime? SslExpiryDate { get; set; }
    public int? SslDaysRemaining { get; set; }
    public string? Headers { get; set; } // JSONB
    public DateTime CheckedAt { get; set; } = DateTime.UtcNow;
}
