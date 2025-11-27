using Microsoft.EntityFrameworkCore;
using ERAMonitor.Core.DTOs.Common;
using ERAMonitor.Core.DTOs.Locations;
using ERAMonitor.Core.Entities;
using ERAMonitor.Core.Enums;
using ERAMonitor.Core.Interfaces.Repositories;

namespace ERAMonitor.Infrastructure.Data.Repositories;

public class LocationRepository : Repository<Location>, ILocationRepository
{
    public LocationRepository(ApplicationDbContext context) : base(context)
    {
    }
    
    public async Task<PagedResponse<LocationDto>> GetPagedAsync(
        Guid organizationId, 
        PagedRequest request, 
        LocationCategory? category = null,
        bool? isActive = null)
    {
        var query = _dbSet.AsQueryable()
            .Where(l => l.OrganizationId == organizationId);
            
        if (category.HasValue)
        {
            query = query.Where(l => l.Category == category.Value);
        }
        
        if (isActive.HasValue)
        {
            query = query.Where(l => l.IsActive == isActive.Value);
        }
        
        if (!string.IsNullOrEmpty(request.Search))
        {
            var search = request.Search.ToLower();
            query = query.Where(l => l.Name.ToLower().Contains(search) || 
                                     (l.City != null && l.City.ToLower().Contains(search)) ||
                                     (l.Country != null && l.Country.ToLower().Contains(search)));
        }
        
        // Sorting
        query = request.SortBy?.ToLower() switch
        {
            "name" => request.SortOrder == "desc" ? query.OrderByDescending(l => l.Name) : query.OrderBy(l => l.Name),
            "createdat" => request.SortOrder == "desc" ? query.OrderByDescending(l => l.CreatedAt) : query.OrderBy(l => l.CreatedAt),
            _ => query.OrderByDescending(l => l.CreatedAt)
        };
        
        var totalCount = await query.CountAsync();
        
        var items = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(l => new LocationDto
            {
                Id = l.Id,
                Name = l.Name,
                Category = l.Category,
                ProviderName = l.ProviderName,
                City = l.City,
                Country = l.Country,
                Latitude = l.Latitude,
                Longitude = l.Longitude,
                HostCount = 0, // TODO: Implement host count when Host entity is fully linked
                IsActive = l.IsActive,
                CreatedAt = l.CreatedAt
            })
            .ToListAsync();
            
        return new PagedResponse<LocationDto>(items, totalCount, request.Page, request.PageSize);
    }
    
    public async Task<LocationDetailDto?> GetDetailAsync(Guid id)
    {
        var location = await _dbSet.FindAsync(id);
        
        if (location == null) return null;
        
        return new LocationDetailDto
        {
            Id = location.Id,
            Name = location.Name,
            Category = location.Category,
            ProviderName = location.ProviderName,
            City = location.City,
            Country = location.Country,
            Address = location.Address,
            ContactInfo = location.ContactInfo,
            Latitude = location.Latitude,
            Longitude = location.Longitude,
            Notes = location.Notes,
            IsActive = location.IsActive,
            CreatedAt = location.CreatedAt,
            UpdatedAt = location.UpdatedAt,
            HostCount = 0, // TODO
            HostsUp = 0, // TODO
            HostsDown = 0, // TODO
            CheckCount = 0 // TODO
        };
    }
    
    public async Task<List<Location>> GetByOrganizationAsync(Guid organizationId)
    {
        return await _dbSet.Where(l => l.OrganizationId == organizationId).ToListAsync();
    }
    
    public async Task<List<LocationDto>> GetForMapAsync(Guid organizationId)
    {
        return await _dbSet
            .Where(l => l.OrganizationId == organizationId && l.Latitude.HasValue && l.Longitude.HasValue)
            .Select(l => new LocationDto
            {
                Id = l.Id,
                Name = l.Name,
                Category = l.Category,
                ProviderName = l.ProviderName,
                City = l.City,
                Country = l.Country,
                Latitude = l.Latitude,
                Longitude = l.Longitude,
                IsActive = l.IsActive,
                CreatedAt = l.CreatedAt
            })
            .ToListAsync();
    }
}
