using ERAMonitor.Core.DTOs.Common;
using ERAMonitor.Core.DTOs.Dashboard;

namespace ERAMonitor.Core.Interfaces;

public interface IDashboardService
{
    Task<PagedResponse<DashboardDto>> GetPagedAsync(Guid organizationId, Guid userId, PagedRequest request);
    Task<DashboardDetailDto?> GetDetailAsync(Guid id, Guid organizationId, Guid userId);
    Task<DashboardDetailDto?> GetBySlugAsync(string slug, Guid organizationId, Guid userId);
    Task<DashboardDetailDto> CreateAsync(Guid organizationId, Guid userId, CreateDashboardDto dto);
    Task<DashboardDetailDto?> UpdateAsync(Guid id, Guid organizationId, Guid userId, UpdateDashboardDto dto);
    Task<bool> DeleteAsync(Guid id, Guid organizationId, Guid userId);
    
    // Widget management
    Task<DashboardWidgetDto> AddWidgetAsync(Guid dashboardId, Guid organizationId, Guid userId, CreateWidgetDto dto);
    Task<DashboardWidgetDto?> UpdateWidgetAsync(Guid widgetId, Guid organizationId, Guid userId, UpdateWidgetDto dto);
    Task<bool> DeleteWidgetAsync(Guid widgetId, Guid organizationId, Guid userId);
    Task UpdateWidgetPositionsAsync(Guid dashboardId, Guid organizationId, Guid userId, List<Guid> widgetIds);
}
