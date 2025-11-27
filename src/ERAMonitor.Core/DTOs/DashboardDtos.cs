using ERAMonitor.Core.Enums;

namespace ERAMonitor.Core.DTOs.Dashboard;

public class DashboardDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Slug { get; set; } = string.Empty;
    public DashboardVisibility Visibility { get; set; }
    public bool IsDefault { get; set; }
    public int WidgetCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class DashboardDetailDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Slug { get; set; } = string.Empty;
    public int Columns { get; set; }
    public DashboardVisibility Visibility { get; set; }
    public bool IsDefault { get; set; }
    public int RefreshIntervalSeconds { get; set; }
    public string? Theme { get; set; }
    public Guid? DefaultCustomerId { get; set; }
    public List<DashboardWidgetDto> Widgets { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class DashboardWidgetDto
{
    public Guid Id { get; set; }
    public Guid DashboardId { get; set; }
    public string Name { get; set; } = string.Empty;
    public WidgetType Type { get; set; }
    public int SizeX { get; set; }
    public int SizeY { get; set; }
    public int Col { get; set; }
    public int Row { get; set; }
    public string? Config { get; set; }
    public int RefreshIntervalSeconds { get; set; }
}

public class CreateDashboardDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Slug { get; set; } = string.Empty;
    public int Columns { get; set; } = 12;
    public DashboardVisibility Visibility { get; set; } = DashboardVisibility.Private;
    public bool IsDefault { get; set; }
    public int RefreshIntervalSeconds { get; set; } = 30;
    public string? Theme { get; set; }
}

public class UpdateDashboardDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Slug { get; set; } = string.Empty;
    public int Columns { get; set; }
    public DashboardVisibility Visibility { get; set; }
    public bool IsDefault { get; set; }
    public int RefreshIntervalSeconds { get; set; }
    public string? Theme { get; set; }
    public Guid? DefaultCustomerId { get; set; }
}

public class CreateWidgetDto
{
    public Guid DashboardId { get; set; }
    public string Name { get; set; } = string.Empty;
    public WidgetType Type { get; set; }
    public int SizeX { get; set; }
    public int SizeY { get; set; }
    public int Col { get; set; }
    public int Row { get; set; }
    public string? Config { get; set; }
    public int RefreshIntervalSeconds { get; set; }
}

public class UpdateWidgetDto
{
    public string Name { get; set; } = string.Empty;
    public int SizeX { get; set; }
    public int SizeY { get; set; }
    public int Col { get; set; }
    public int Row { get; set; }
    public string? Config { get; set; }
    public int RefreshIntervalSeconds { get; set; }
}
