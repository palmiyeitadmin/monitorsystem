using System.ComponentModel.DataAnnotations;
using ERAMonitor.Core.Enums;

namespace ERAMonitor.Core.DTOs.Locations;

public class CreateLocationRequest
{
    [Required(ErrorMessage = "Name is required")]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Category is required")]
    public LocationCategory Category { get; set; }
    
    [MaxLength(200)]
    public string? ProviderName { get; set; }
    
    [MaxLength(100)]
    public string? City { get; set; }
    
    [MaxLength(100)]
    public string? Country { get; set; }
    
    public string? Address { get; set; }
    
    public string? ContactInfo { get; set; }
    
    [Range(-90, 90)]
    public decimal? Latitude { get; set; }
    
    [Range(-180, 180)]
    public decimal? Longitude { get; set; }
    
    public string? Notes { get; set; }
}
