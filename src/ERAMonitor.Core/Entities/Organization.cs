using ERAMonitor.Core.Enums;

namespace ERAMonitor.Core.Entities;

public class Organization : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? LogoUrl { get; set; }
    public string? Settings { get; set; } // JSON
    public bool IsActive { get; set; } = true;
    
    // Navigation
    public virtual ICollection<User> Users { get; set; } = new List<User>();
    public virtual ICollection<Customer> Customers { get; set; } = new List<Customer>();
    public virtual ICollection<Location> Locations { get; set; } = new List<Location>();
    public virtual ICollection<Host> Hosts { get; set; } = new List<Host>();
    public virtual ICollection<Check> Checks { get; set; } = new List<Check>();
    public virtual ICollection<Incident> Incidents { get; set; } = new List<Incident>();
    public virtual SystemSetting? SystemSetting { get; set; }
}
