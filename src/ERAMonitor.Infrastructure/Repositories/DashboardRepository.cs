using Microsoft.EntityFrameworkCore;
using ERAMonitor.Core.DTOs.Common;
using ERAMonitor.Core.DTOs.Dashboard;
using ERAMonitor.Core.Entities;
using ERAMonitor.Core.Enums;
using ERAMonitor.Core.Interfaces.Repositories;
using ERAMonitor.Infrastructure.Data;
using ERAMonitor.Infrastructure.Data.Repositories;

namespace ERAMonitor.Infrastructure.Repositories;

public class DashboardRepository : Repository<Dashboard>, IDashboardRepository
{
    public DashboardRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<PagedResponse<DashboardDto>> GetPagedAsync(Guid organizationId, Guid userId, PagedRequest request)
    {
        var query = _dbSet
            .Where(d => d.OrganizationId == organizationId && 
                        (d.Visibility == DashboardVisibility.Public || 
                         d.Visibility == DashboardVisibility.Team || 
                         d.UserId == userId));

        if (!string.IsNullOrEmpty(request.Search))
        {
            var search = request.Search.ToLower();
            query = query.Where(d => d.Name.ToLower().Contains(search));
        }

        // Sorting
        query = request.SortBy?.ToLower() switch
        {
            "name" => request.SortOrder == "desc" ? query.OrderByDescending(d => d.Name) : query.OrderBy(d => d.Name),
            "createdat" => request.SortOrder == "desc" ? query.OrderByDescending(d => d.CreatedAt) : query.OrderBy(d => d.CreatedAt),
            _ => query.OrderByDescending(d => d.CreatedAt)
        };

        var totalCount = await query.CountAsync();
        
        var items = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(d => new DashboardDto
            {
                Id = d.Id,
                Name = d.Name,
                Description = d.Description,
                Slug = d.Slug,
                Visibility = d.Visibility,
                IsDefault = d.IsDefault,
                WidgetCount = d.Widgets.Count,
                CreatedAt = d.CreatedAt,
                UpdatedAt = d.UpdatedAt
            })
            .ToListAsync();

        return new PagedResponse<DashboardDto>(items, totalCount, request.Page, request.PageSize);
    }

    public async Task<DashboardDetailDto?> GetDetailAsync(Guid id, Guid organizationId, Guid userId)
    {
        var dashboard = await _dbSet
            .Include(d => d.Widgets)
            .FirstOrDefaultAsync(d => d.Id == id && d.OrganizationId == organizationId);

        if (dashboard == null) return null;
        
        // Check visibility
        if (dashboard.Visibility == DashboardVisibility.Private && dashboard.UserId != userId)
            return null;

        return MapToDetailDto(dashboard);
    }

    public async Task<DashboardDetailDto?> GetBySlugAsync(string slug, Guid organizationId, Guid userId)
    {
        var dashboard = await _dbSet
            .Include(d => d.Widgets)
            .FirstOrDefaultAsync(d => d.Slug == slug && d.OrganizationId == organizationId);

        if (dashboard == null) return null;
        
        // Check visibility
        if (dashboard.Visibility == DashboardVisibility.Private && dashboard.UserId != userId)
            return null;

        return MapToDetailDto(dashboard);
    }

    public async Task<bool> SlugExistsAsync(string slug, Guid organizationId)
    {
        return await _dbSet.AnyAsync(d => d.Slug == slug && d.OrganizationId == organizationId);
    }

    public async Task UnsetDefaultAsync(Guid organizationId, Guid userId, Guid? excludeDashboardId)
    {
        var query = _dbSet.Where(d => d.OrganizationId == organizationId && d.UserId == userId && d.IsDefault);
        
        if (excludeDashboardId.HasValue)
        {
            query = query.Where(d => d.Id != excludeDashboardId.Value);
        }
        
        var defaults = await query.ToListAsync();
        foreach (var d in defaults)
        {
            d.IsDefault = false;
        }
    }

    public async Task<Dashboard?> GetDefaultAsync(Guid organizationId, Guid userId)
    {
        return await _dbSet
            .FirstOrDefaultAsync(d => d.OrganizationId == organizationId && d.UserId == userId && d.IsDefault);
    }
    
    private static DashboardDetailDto MapToDetailDto(Dashboard d)
    {
        return new DashboardDetailDto
        {
            Id = d.Id,
            Name = d.Name,
            Description = d.Description,
            Slug = d.Slug,
            Columns = d.Columns,
            Visibility = d.Visibility,
            IsDefault = d.IsDefault,
            RefreshIntervalSeconds = d.RefreshIntervalSeconds,
            Theme = d.Theme,
            DefaultCustomerId = d.DefaultCustomerId,
            Widgets = d.Widgets.Select(w => new DashboardWidgetDto
            {
                Id = w.Id,
                DashboardId = w.DashboardId,
                Name = w.Title,
                Type = w.Type,
                SizeX = w.Width,
                SizeY = w.Height,
                Col = w.PositionX,
                Row = w.PositionY,
                Config = w.Configuration,
                RefreshIntervalSeconds = 0 // Not supported in entity
            }).ToList(),
            CreatedAt = d.CreatedAt,
            UpdatedAt = d.UpdatedAt
        };
    }
}

public class DashboardWidgetRepository : Repository<DashboardWidget>, IDashboardWidgetRepository
{
    public DashboardWidgetRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<List<DashboardWidget>> GetByDashboardAsync(Guid dashboardId)
    {
        return await _dbSet
            .Where(w => w.DashboardId == dashboardId)
            .OrderBy(w => w.SortOrder)
            .ToListAsync();
    }
}
