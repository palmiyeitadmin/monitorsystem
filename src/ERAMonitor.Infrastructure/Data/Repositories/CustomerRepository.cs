using Microsoft.EntityFrameworkCore;
using ERAMonitor.Core.DTOs.Common;
using ERAMonitor.Core.DTOs.Customers;
using ERAMonitor.Core.Entities;
using ERAMonitor.Core.Interfaces.Repositories;

namespace ERAMonitor.Infrastructure.Data.Repositories;

public class CustomerRepository : Repository<Customer>, ICustomerRepository
{
    public CustomerRepository(ApplicationDbContext context) : base(context)
    {
    }
    
    public async Task<Customer?> GetBySlugAsync(Guid organizationId, string slug)
    {
        return await _dbSet.FirstOrDefaultAsync(c => c.OrganizationId == organizationId && c.Slug == slug);
    }
    
    public async Task<Customer?> GetByApiKeyAsync(string apiKey)
    {
        return await _dbSet.FirstOrDefaultAsync(c => c.ApiKey == apiKey);
    }
    
    public async Task<bool> SlugExistsAsync(Guid organizationId, string slug, Guid? excludeId = null)
    {
        var query = _dbSet.Where(c => c.OrganizationId == organizationId && c.Slug == slug);
        
        if (excludeId.HasValue)
        {
            query = query.Where(c => c.Id != excludeId.Value);
        }
        
        return await query.AnyAsync();
    }
    
    public async Task<PagedResponse<CustomerListItemDto>> GetPagedAsync(
        Guid organizationId, 
        PagedRequest request, 
        Guid? assignedAdminId = null, 
        bool? isActive = null)
    {
        var query = _dbSet.AsQueryable()
            .Where(c => c.OrganizationId == organizationId);
            
        if (assignedAdminId.HasValue)
        {
            query = query.Where(c => c.AssignedAdminId == assignedAdminId.Value || 
                                     c.UserAssignments.Any(ua => ua.UserId == assignedAdminId.Value));
        }
        
        if (isActive.HasValue)
        {
            query = query.Where(c => c.IsActive == isActive.Value);
        }
        
        if (!string.IsNullOrEmpty(request.Search))
        {
            var search = request.Search.ToLower();
            query = query.Where(c => c.Name.ToLower().Contains(search) || 
                                     (c.ContactName != null && c.ContactName.ToLower().Contains(search)) ||
                                     (c.ContactEmail != null && c.ContactEmail.ToLower().Contains(search)));
        }
        
        // Sorting
        query = request.SortBy?.ToLower() switch
        {
            "name" => request.SortOrder == "desc" ? query.OrderByDescending(c => c.Name) : query.OrderBy(c => c.Name),
            "createdat" => request.SortOrder == "desc" ? query.OrderByDescending(c => c.CreatedAt) : query.OrderBy(c => c.CreatedAt),
            "hostcount" => request.SortOrder == "desc" ? query.OrderByDescending(c => c.Hosts.Count) : query.OrderBy(c => c.Hosts.Count),
            _ => query.OrderByDescending(c => c.CreatedAt)
        };
        
        var totalCount = await query.CountAsync();
        
        var items = await query
            .Include(c => c.AssignedAdmin)
            .Include(c => c.Hosts)
            .Include(c => c.Incidents)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(c => new CustomerListItemDto
            {
                Id = c.Id,
                Name = c.Name,
                Slug = c.Slug,
                LogoUrl = c.LogoUrl,
                Industry = c.Industry,
                ContactName = c.ContactName,
                ContactEmail = c.ContactEmail,
                AssignedAdmin = c.AssignedAdmin != null ? new AssignedAdminDto 
                { 
                    Id = c.AssignedAdmin.Id, 
                    FullName = c.AssignedAdmin.FullName,
                    Email = c.AssignedAdmin.Email,
                    AvatarUrl = c.AssignedAdmin.AvatarUrl
                } : null,
                HostCount = c.Hosts.Count,
                WebsiteCount = c.Hosts.Count(h => h.Category == Core.Enums.HostCategory.Website),
                ActiveIncidentCount = c.Incidents.Count(i => i.ResolvedAt == null),
                IsActive = c.IsActive,
                PortalEnabled = c.PortalEnabled,
                CreatedAt = c.CreatedAt
            })
            .ToListAsync();
            
        return new PagedResponse<CustomerListItemDto>(items, totalCount, request.Page, request.PageSize);
    }
    
    public async Task<List<Customer>> GetByAssignedAdminAsync(Guid adminId)
    {
        return await _dbSet
            .Where(c => c.AssignedAdminId == adminId || c.UserAssignments.Any(ua => ua.UserId == adminId))
            .ToListAsync();
    }
    
    public async Task<CustomerDetailDto?> GetDetailAsync(Guid id)
    {
        var customer = await _dbSet
            .Include(c => c.AssignedAdmin)
            .Include(c => c.Hosts)
            .Include(c => c.Incidents)
            .Include(c => c.CustomerUsers)
            .FirstOrDefaultAsync(c => c.Id == id);
            
        if (customer == null) return null;
        
        return new CustomerDetailDto
        {
            Id = customer.Id,
            Name = customer.Name,
            Slug = customer.Slug,
            LogoUrl = customer.LogoUrl,
            Industry = customer.Industry,
            ContactName = customer.ContactName,
            ContactEmail = customer.ContactEmail,
            ContactPhone = customer.ContactPhone,
            ContactMobile = customer.ContactMobile,
            ContactJobTitle = customer.ContactJobTitle,
            SecondaryContactName = customer.SecondaryContactName,
            SecondaryContactEmail = customer.SecondaryContactEmail,
            SecondaryContactPhone = customer.SecondaryContactPhone,
            EmergencyContactName = customer.EmergencyContactName,
            EmergencyContactPhone = customer.EmergencyContactPhone,
            EmergencyAvailableHours = customer.EmergencyAvailableHours,
            AddressLine1 = customer.AddressLine1,
            AddressLine2 = customer.AddressLine2,
            City = customer.City,
            Country = customer.Country,
            PostalCode = customer.PostalCode,
            PortalEnabled = customer.PortalEnabled,
            ApiEnabled = customer.ApiEnabled,
            ApiKey = customer.ApiKey,
            ApiRateLimit = customer.ApiRateLimit,
            AssignedAdmin = customer.AssignedAdmin != null ? new AssignedAdminDto 
            { 
                Id = customer.AssignedAdmin.Id, 
                FullName = customer.AssignedAdmin.FullName,
                Email = customer.AssignedAdmin.Email,
                AvatarUrl = customer.AssignedAdmin.AvatarUrl
            } : null,
            HostCount = customer.Hosts.Count,
            WebsiteCount = customer.Hosts.Count(h => h.Category == Core.Enums.HostCategory.Website),
            ActiveIncidentCount = customer.Incidents.Count(i => i.ResolvedAt == null),
            PortalUserCount = customer.CustomerUsers.Count,
            Notes = customer.Notes,
            IsActive = customer.IsActive,
            CreatedAt = customer.CreatedAt,
            UpdatedAt = customer.UpdatedAt
        };
    }
}
