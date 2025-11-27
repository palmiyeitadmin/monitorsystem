using ERAMonitor.Core.Enums;

namespace ERAMonitor.Core.Entities;

public class ServiceStatusHistory
{
    public long Id { get; set; } // BIGSERIAL
    public Guid ServiceId { get; set; }
    
    public StatusType Status { get; set; }
    public string? Message { get; set; }
    public string? Details { get; set; } // JSON for additional info
    
    public DateTime RecordedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation
    public virtual Service Service { get; set; } = null!;
}
