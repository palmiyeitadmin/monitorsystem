using ERAMonitor.Core.Enums;

namespace ERAMonitor.Core.Entities;

public class EventLog : BaseEntity
{
    public Guid HostId { get; set; }
    
    // Event Log Details
    public string LogName { get; set; } = string.Empty; // System, Application, Security, etc.
    public int EventId { get; set; }
    public string Level { get; set; } = string.Empty; // Critical, Error, Warning
    public string Source { get; set; } = string.Empty; // MSSQLSERVER, IIS, etc.
    public string Category { get; set; } = string.Empty; // System, SQL, IIS, Veeam, etc.
    public string Message { get; set; } = string.Empty;
    
    public DateTime TimeCreated { get; set; } // When the event was created on the host
    public DateTime RecordedAt { get; set; } = DateTime.UtcNow; // When it was recorded in our system
    
    // Navigation
    public virtual Host Host { get; set; } = null!;
}
