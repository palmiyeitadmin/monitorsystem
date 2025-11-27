namespace ERAMonitor.Core.DTOs.EventLog;

public class EventLogListItemDto
{
    public Guid Id { get; set; }
    public Guid HostId { get; set; }
    
    public string LogName { get; set; } = string.Empty;
    public int EventId { get; set; }
    public string Level { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    
    public DateTime TimeCreated { get; set; }
    public DateTime RecordedAt { get; set; }
}
