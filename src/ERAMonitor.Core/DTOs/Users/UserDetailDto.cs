namespace ERAMonitor.Core.DTOs.Users;

public class UserDetailDto
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public string? Phone { get; set; }
    public string? JobTitle { get; set; }
    public string Role { get; set; } = string.Empty;
    public string Timezone { get; set; } = "UTC";
    public string Locale { get; set; } = "en";
    public bool IsActive { get; set; }
    public bool EmailVerified { get; set; }
    public bool TwoFactorEnabled { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public string? LastLoginIp { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    // Assigned customers (for Admin/Operator roles)
    public List<AssignedCustomerDto> AssignedCustomers { get; set; } = new();
    
    // Permissions
    public UserPermissionsDto? Permissions { get; set; }
}

public class AssignedCustomerDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? LogoUrl { get; set; }
}
