using System.ComponentModel.DataAnnotations;

namespace ERAMonitor.Core.DTOs.Auth;

public class UpdateProfileRequest
{
    [Required(ErrorMessage = "Full name is required")]
    [MaxLength(200, ErrorMessage = "Full name cannot exceed 200 characters")]
    public string FullName { get; set; } = string.Empty;
    
    [Phone(ErrorMessage = "Invalid phone number")]
    public string? Phone { get; set; }
    
    [MaxLength(100)]
    public string? JobTitle { get; set; }
    
    [MaxLength(50)]
    public string Timezone { get; set; } = "UTC";
    
    [MaxLength(10)]
    public string Locale { get; set; } = "en";
}
