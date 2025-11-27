namespace ERAMonitor.Core.Entities;

public class CustomerUser : BaseEntity
{
    public Guid CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    public bool IsPrimary { get; set; } = false;
}
