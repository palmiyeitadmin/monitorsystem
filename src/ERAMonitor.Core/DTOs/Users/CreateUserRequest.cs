using System.ComponentModel.DataAnnotations;
using ERAMonitor.Core.Enums;

namespace ERAMonitor.Core.DTOs.Users;

public class CreateUserRequest
{
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    [MaxLength(255)]
    public string Email { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Full name is required")]
    [MaxLength(200)]
    public string FullName { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Password is required")]
    [MinLength(8, ErrorMessage = "Password must be at least 8 characters")]
    public string Password { get; set; } = string.Empty;
    
    [Phone]
    public string? Phone { get; set; }
    
    [MaxLength(100)]
    public string? JobTitle { get; set; }
    
    [Required(ErrorMessage = "Role is required")]
    public UserRole Role { get; set; } = UserRole.Viewer;
    
    public string Timezone { get; set; } = "UTC";
    
    public List<Guid>? AssignedCustomerIds { get; set; }
    
    public bool SendWelcomeEmail { get; set; } = true;
}
