using ERAMonitor.Core.Enums;

namespace ERAMonitor.Core.Entities;

public class IncidentTimeline : BaseEntity
{
    public Guid IncidentId { get; set; }
    public Incident Incident { get; set; } = null!;
    public Guid? UserId { get; set; }
    public User? User { get; set; }
    public string EventType { get; set; } = string.Empty;
    public string? Content { get; set; }
    public bool IsInternal { get; set; } = true;
    public string? Metadata { get; set; } // JSONB
}
