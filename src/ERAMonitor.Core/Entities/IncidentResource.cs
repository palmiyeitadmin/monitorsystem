using ERAMonitor.Core.Enums;

namespace ERAMonitor.Core.Entities;

public class IncidentResource : BaseEntity
{
    public Guid IncidentId { get; set; }
    public Incident Incident { get; set; } = null!;
    public string ResourceType { get; set; } = string.Empty;
    public Guid ResourceId { get; set; }
    public bool IsPrimary { get; set; } = false;
}
