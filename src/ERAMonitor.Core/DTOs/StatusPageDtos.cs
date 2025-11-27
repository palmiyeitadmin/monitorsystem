using ERAMonitor.Core.Enums;

namespace ERAMonitor.Core.DTOs.StatusPages;

public class StatusPageDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? CustomDomain { get; set; }
    public bool IsPublic { get; set; }
    public bool IsActive { get; set; }
    public int ComponentCount { get; set; }
    public int SubscriberCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class StatusPageDetailDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? CustomDomain { get; set; }
    
    // Branding
    public string? LogoUrl { get; set; }
    public string? FaviconUrl { get; set; }
    public string? CompanyName { get; set; }
    public string PrimaryColor { get; set; } = "#29ABE2";
    public string? CustomCss { get; set; }
    
    // Content
    public string? HeaderText { get; set; }
    public string? FooterText { get; set; }
    public string? AboutText { get; set; }
    
    // Features
    public bool ShowUptime { get; set; }
    public int UptimeDays { get; set; }
    public bool ShowIncidents { get; set; }
    public bool ShowMaintenances { get; set; }
    public bool ShowSubscribe { get; set; }
    public bool ShowResponseTime { get; set; }
    
    public bool IsPublic { get; set; }
    public bool HasPassword { get; set; }
    public bool IsActive { get; set; }
    
    public List<StatusPageComponentDto> Components { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class StatusPageComponentDto
{
    public Guid Id { get; set; }
    public Guid StatusPageId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public StatusPageComponentType Type { get; set; }
    public Guid? HostId { get; set; }
    public Guid? CheckId { get; set; }
    public Guid? ServiceId { get; set; }
    public Guid? GroupId { get; set; }
    public int SortOrder { get; set; }
    public int Order { get; set; }
    public bool ShowUptime { get; set; }
    public bool ShowResponseTime { get; set; }
    public StatusPageComponentStatus? Status { get; set; }
}

public class CreateStatusPageDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Slug { get; set; } = string.Empty;
    public string? CustomDomain { get; set; }
    public string? CompanyName { get; set; }
    public string? LogoUrl { get; set; }
    public string? FaviconUrl { get; set; }
    public string? WebsiteUrl { get; set; }
    public string? SupportUrl { get; set; }
    public string? CustomCss { get; set; }
    public string? GoogleAnalyticsId { get; set; }
    
    public bool ShowUptime { get; set; }
    public bool ShowIncidents { get; set; }
    public bool ShowMaintenance { get; set; }
    public string? Theme { get; set; }
    
    public bool IsPublic { get; set; } = true;
    public string? Password { get; set; }
    public bool IsActive { get; set; } = true;
}

public class UpdateStatusPageDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Slug { get; set; } = string.Empty;
    public string? CustomDomain { get; set; }
    
    // Branding
    public string? LogoUrl { get; set; }
    public string? FaviconUrl { get; set; }
    public string? CompanyName { get; set; }
    public string PrimaryColor { get; set; } = "#29ABE2";
    public string? CustomCss { get; set; }
    public string? WebsiteUrl { get; set; }
    public string? SupportUrl { get; set; }
    
    // Content
    public string? HeaderText { get; set; }
    public string? FooterText { get; set; }
    public string? AboutText { get; set; }
    
    // Features
    public bool ShowUptime { get; set; }
    public int UptimeDays { get; set; }
    public bool ShowIncidents { get; set; }
    public bool ShowMaintenance { get; set; }
    public bool ShowSubscribe { get; set; }
    public bool ShowResponseTime { get; set; }
    public string? GoogleAnalyticsId { get; set; }
    public string? Theme { get; set; }
    
    public bool IsPublic { get; set; }
    public string? Password { get; set; }
    public bool IsActive { get; set; }
}

public class CreateStatusPageComponentDto
{
    public Guid? GroupId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public StatusPageComponentType Type { get; set; }
    public bool ShowUptime { get; set; }
    public int Order { get; set; }
    public Guid? ServiceId { get; set; }
    public Guid? HostId { get; set; }
}

public class UpdateStatusPageComponentDto
{
    public Guid? GroupId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public StatusPageComponentType Type { get; set; }
    public bool ShowUptime { get; set; }
    public int Order { get; set; }
    public Guid? ServiceId { get; set; }
    public Guid? HostId { get; set; }
}

public class UpdateComponentStatusDto
{
    public StatusPageComponentStatus Status { get; set; }
}

public class StatusPageComponentGroupDto
{
    public Guid Id { get; set; }
    public Guid StatusPageId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int Order { get; set; }
    public bool IsCollapsed { get; set; }
}

public class CreateStatusPageComponentGroupDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int Order { get; set; }
    public bool IsCollapsed { get; set; }
}

public class UpdateStatusPageComponentGroupDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int Order { get; set; }
    public bool IsCollapsed { get; set; }
}

public class StatusPageSubscriberDto
{
    public Guid Id { get; set; }
    public Guid StatusPageId { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public bool IsVerified { get; set; }
    public DateTime SubscribedAt { get; set; }
    public bool EmailSubscribed { get; set; }
    public bool SmsSubscribed { get; set; }
}

public class CreateStatusPageSubscriberDto
{
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public bool EmailSubscribed { get; set; }
    public bool SmsSubscribed { get; set; }
}
