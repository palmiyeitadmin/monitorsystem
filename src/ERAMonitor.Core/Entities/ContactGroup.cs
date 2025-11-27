namespace ERAMonitor.Core.Entities;

public class ContactGroup : BaseEntityWithOrganization
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    
    // Members
    public virtual ICollection<ContactGroupMember> Members { get; set; } = new List<ContactGroupMember>();
    
    // Settings
    public bool IsActive { get; set; } = true;
}

public class ContactGroupMember : BaseEntity
{
    public Guid ContactGroupId { get; set; }
    
    // Either a user or external contact
    public Guid? UserId { get; set; }
    public string? ExternalEmail { get; set; }
    public string? ExternalPhone { get; set; }
    public string? ExternalName { get; set; }
    
    // Preferences for this member
    public bool ReceiveEmail { get; set; } = true;
    public bool ReceiveSms { get; set; } = false;
    public bool ReceivePush { get; set; } = true;
    
    // Navigation
    public virtual ContactGroup ContactGroup { get; set; } = null!;
    public virtual User? User { get; set; }
}
