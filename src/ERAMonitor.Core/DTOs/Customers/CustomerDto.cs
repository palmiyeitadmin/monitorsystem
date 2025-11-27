namespace ERAMonitor.Core.DTOs.Customers;

public class CustomerDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? LogoUrl { get; set; }
    public string? Industry { get; set; }
    public string? ContactName { get; set; }
    public string? ContactEmail { get; set; }
    public bool IsActive { get; set; }
    public bool PortalEnabled { get; set; }
    public DateTime CreatedAt { get; set; }
}
