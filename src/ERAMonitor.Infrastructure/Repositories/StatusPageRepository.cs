using Microsoft.EntityFrameworkCore;
using ERAMonitor.Core.DTOs.Common;
using ERAMonitor.Core.DTOs.StatusPages;
using ERAMonitor.Core.Entities;
using ERAMonitor.Core.Interfaces.Repositories;
using ERAMonitor.Infrastructure.Data;
using ERAMonitor.Infrastructure.Data.Repositories;

namespace ERAMonitor.Infrastructure.Repositories;

public class StatusPageRepository : Repository<StatusPage>, IStatusPageRepository
{
    public StatusPageRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<PagedResponse<StatusPageDto>> GetPagedAsync(Guid organizationId, PagedRequest request)
    {
        var query = _dbSet.Where(sp => sp.OrganizationId == organizationId);

        if (!string.IsNullOrEmpty(request.Search))
        {
            var search = request.Search.ToLower();
            query = query.Where(sp => sp.Name.ToLower().Contains(search) || sp.Slug.ToLower().Contains(search));
        }

        // Sorting
        query = request.SortBy?.ToLower() switch
        {
            "name" => request.SortOrder == "desc" ? query.OrderByDescending(sp => sp.Name) : query.OrderBy(sp => sp.Name),
            "createdat" => request.SortOrder == "desc" ? query.OrderByDescending(sp => sp.CreatedAt) : query.OrderBy(sp => sp.CreatedAt),
            _ => query.OrderByDescending(sp => sp.CreatedAt)
        };

        var totalCount = await query.CountAsync();
        
        var items = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(sp => new StatusPageDto
            {
                Id = sp.Id,
                Name = sp.Name,
                Slug = sp.Slug,
                CustomDomain = sp.CustomDomain,
                IsPublic = sp.IsPublic,
                IsActive = sp.IsActive,
                ComponentCount = sp.Components.Count,
                SubscriberCount = sp.Subscribers.Count,
                CreatedAt = sp.CreatedAt,
                UpdatedAt = sp.UpdatedAt
            })
            .ToListAsync();

        return new PagedResponse<StatusPageDto>(items, totalCount, request.Page, request.PageSize);
    }

    public async Task<StatusPageDetailDto?> GetDetailAsync(Guid id, Guid organizationId)
    {
        var statusPage = await _dbSet
            .Include(sp => sp.Components)
            .Include(sp => sp.Subscribers)
            .FirstOrDefaultAsync(sp => sp.Id == id && sp.OrganizationId == organizationId);

        if (statusPage == null) return null;

        return MapToDetailDto(statusPage);
    }

    public async Task<StatusPage?> GetBySlugAsync(string slug)
    {
        return await _dbSet
            .Include(sp => sp.Components)
            .Include(sp => sp.Subscribers)
            .FirstOrDefaultAsync(sp => sp.Slug == slug || sp.CustomDomain == slug);
    }

    public async Task<bool> SlugExistsAsync(string slug)
    {
        return await _dbSet.AnyAsync(sp => sp.Slug == slug || sp.CustomDomain == slug);
    }
    
    private static StatusPageDetailDto MapToDetailDto(StatusPage sp)
    {
        return new StatusPageDetailDto
        {
            Id = sp.Id,
            Name = sp.Name,
            Slug = sp.Slug,
            CustomDomain = sp.CustomDomain,
            LogoUrl = sp.LogoUrl,
            FaviconUrl = sp.FaviconUrl,
            CompanyName = sp.CompanyName,
            PrimaryColor = sp.PrimaryColor,
            CustomCss = sp.CustomCss,
            HeaderText = sp.HeaderText,
            FooterText = sp.FooterText,
            AboutText = sp.AboutText,
            ShowUptime = sp.ShowUptime,
            UptimeDays = sp.UptimeDays,
            ShowIncidents = sp.ShowIncidents,
            ShowMaintenances = sp.ShowMaintenances,
            ShowSubscribe = sp.ShowSubscribe,
            ShowResponseTime = sp.ShowResponseTime,
            IsPublic = sp.IsPublic,
            HasPassword = !string.IsNullOrEmpty(sp.Password),
            IsActive = sp.IsActive,
            Components = sp.Components.Select(c => new StatusPageComponentDto
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                Type = c.Type,
                HostId = c.HostId,
                CheckId = c.CheckId,
                ServiceId = c.ServiceId,
                GroupId = c.GroupId,
                SortOrder = c.SortOrder,
                ShowUptime = c.ShowUptime,
                ShowResponseTime = c.ShowResponseTime,
                Status = c.OverrideStatus ? c.ManualStatus : null
            }).ToList(),
            CreatedAt = sp.CreatedAt,
            UpdatedAt = sp.UpdatedAt
        };
    }
}

public class StatusPageComponentRepository : Repository<StatusPageComponent>, IStatusPageComponentRepository
{
    public StatusPageComponentRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<List<StatusPageComponent>> GetByStatusPageAsync(Guid statusPageId)
    {
        return await _dbSet
            .Where(c => c.StatusPageId == statusPageId)
            .OrderBy(c => c.SortOrder)
            .ToListAsync();
    }
}

public class StatusPageComponentGroupRepository : Repository<StatusPageComponentGroup>, IStatusPageComponentGroupRepository
{
    public StatusPageComponentGroupRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<List<StatusPageComponentGroup>> GetByStatusPageAsync(Guid statusPageId)
    {
        return await _dbSet
            .Where(g => g.StatusPageId == statusPageId)
            .OrderBy(g => g.SortOrder)
            .ToListAsync();
    }
}

public class StatusPageSubscriberRepository : Repository<StatusPageSubscriber>, IStatusPageSubscriberRepository
{
    public StatusPageSubscriberRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<List<StatusPageSubscriber>> GetByStatusPageAsync(Guid statusPageId)
    {
        return await _dbSet
            .Where(s => s.StatusPageId == statusPageId)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync();
    }

    public async Task<StatusPageSubscriber?> GetByTokenAsync(string token)
    {
        return await _dbSet.FirstOrDefaultAsync(s => s.VerificationToken == token);
    }

    public async Task<StatusPageSubscriber?> GetByEmailAsync(Guid statusPageId, string email)
    {
        return await _dbSet.FirstOrDefaultAsync(s => s.StatusPageId == statusPageId && s.Email == email);
    }
}
