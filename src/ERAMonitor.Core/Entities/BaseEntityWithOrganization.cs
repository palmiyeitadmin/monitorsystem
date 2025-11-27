namespace ERAMonitor.Core.Entities;

public abstract class BaseEntityWithOrganization : BaseEntity
{
    public Guid OrganizationId { get; set; }
    public virtual Organization Organization { get; set; } = null!;
}
