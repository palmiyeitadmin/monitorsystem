using System.ComponentModel.DataAnnotations;

namespace ERAMonitor.Core.DTOs;

public class CustomerDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? LogoUrl { get; set; }
    public string? Industry { get; set; }
    public bool IsActive { get; set; }
    public string? ContactName { get; set; }
    public string? ContactEmail { get; set; }
}

public class CreateCustomerRequest
{
    [Required]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    public string Slug { get; set; } = string.Empty;
    
    public string? Industry { get; set; }
    public string? ContactName { get; set; }
    
    [EmailAddress]
    public string? ContactEmail { get; set; }
}

public class UpdateCustomerRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Industry { get; set; }
    public string? ContactName { get; set; }
    public string? ContactEmail { get; set; }
    public bool IsActive { get; set; }
}
