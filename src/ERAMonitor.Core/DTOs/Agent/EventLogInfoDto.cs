namespace ERAMonitor.Core.DTOs.Agent;

public class EventLogInfoDto
{
    public string LogName { get; set; } = string.Empty;
    public int EventId { get; set; }
    public string Level { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public DateTime TimeCreated { get; set; }
    public string Category { get; set; } = string.Empty;
}
