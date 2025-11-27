using ERAMonitor.Core.DTOs.Common;
using ERAMonitor.Core.DTOs.Dashboard;
using ERAMonitor.Core.Entities;

namespace ERAMonitor.Core.Interfaces.Repositories;

public interface IDashboardRepository : IRepository<Dashboard>
{
    Task<PagedResponse<DashboardDto>> GetPagedAsync(Guid organizationId, Guid userId, PagedRequest request);
    Task<DashboardDetailDto?> GetDetailAsync(Guid id, Guid organizationId, Guid userId);
    Task<DashboardDetailDto?> GetBySlugAsync(string slug, Guid organizationId, Guid userId);
    Task<bool> SlugExistsAsync(string slug, Guid organizationId);
    Task UnsetDefaultAsync(Guid organizationId, Guid userId, Guid? excludeDashboardId);
    Task<Dashboard?> GetDefaultAsync(Guid organizationId, Guid userId);
}

public interface IDashboardWidgetRepository : IRepository<DashboardWidget>
{
    Task<List<DashboardWidget>> GetByDashboardAsync(Guid dashboardId);
}
