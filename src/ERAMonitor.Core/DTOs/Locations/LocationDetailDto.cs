using ERAMonitor.Core.Enums;

namespace ERAMonitor.Core.DTOs.Locations;

public class LocationDetailDto
{
    public Guid Id { get; set; }
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
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    // Stats
    public int HostCount { get; set; }
    public int HostsUp { get; set; }
    public int HostsDown { get; set; }
    public int CheckCount { get; set; }
}
