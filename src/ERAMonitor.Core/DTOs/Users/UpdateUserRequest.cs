using System.ComponentModel.DataAnnotations;
using ERAMonitor.Core.Enums;

namespace ERAMonitor.Core.DTOs.Users;

public class UpdateUserRequest
{
    [Required(ErrorMessage = "Full name is required")]
    [MaxLength(200)]
    public string FullName { get; set; } = string.Empty;
    
    [Phone]
    public string? Phone { get; set; }
    
    [MaxLength(100)]
    public string? JobTitle { get; set; }
    
    [Required(ErrorMessage = "Role is required")]
    public UserRole Role { get; set; }
    
    public string Timezone { get; set; } = "UTC";
    
    public bool IsActive { get; set; } = true;
    
    public List<Guid>? AssignedCustomerIds { get; set; }
}
