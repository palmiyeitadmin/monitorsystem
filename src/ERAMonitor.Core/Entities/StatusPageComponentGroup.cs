namespace ERAMonitor.Core.Entities;

public class StatusPageComponentGroup : BaseEntity
{
    public Guid StatusPageId { get; set; }
    
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int SortOrder { get; set; } = 0;
    public bool IsExpanded { get; set; } = true;
    
    // Navigation
    public virtual StatusPage StatusPage { get; set; } = null!;
    public virtual ICollection<StatusPageComponent> Components { get; set; } = new List<StatusPageComponent>();
}
