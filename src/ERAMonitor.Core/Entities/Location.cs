using ERAMonitor.Core.Enums;

namespace ERAMonitor.Core.Entities;

public class Location : BaseEntity
{
    public Guid OrganizationId { get; set; }
    public Organization Organization { get; set; } = null!;
    
    public string Name { get; set; } = string.Empty;
    public LocationCategory Category { get; set; }
    public string? ProviderName { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }
    public string? Address { get; set; }
    public string? ContactInfo { get; set; }
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    public string? Notes { get; set; }
    public bool IsActive { get; set; } = true;
}
