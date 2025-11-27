using System.ComponentModel.DataAnnotations;
using ERAMonitor.Core.Enums;

namespace ERAMonitor.Core.DTOs;

public class LocationDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string? ProviderName { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }
    public bool IsActive { get; set; }
}

public class CreateLocationRequest
{
    [Required]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    public LocationCategory Category { get; set; }
    
    public string? ProviderName { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }
}

public class UpdateLocationRequest
{
    public string Name { get; set; } = string.Empty;
    public LocationCategory Category { get; set; }
    public string? ProviderName { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }
    public bool IsActive { get; set; }
}
