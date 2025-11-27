using Microsoft.EntityFrameworkCore;
using ERAMonitor.Core.DTOs.Common;
using ERAMonitor.Core.DTOs.Services;
using ERAMonitor.Core.Entities;
using ERAMonitor.Core.Enums;
using ERAMonitor.Core.Interfaces.Repositories;
using ERAMonitor.Infrastructure.Data;
using ERAMonitor.Infrastructure.Data.Repositories;

namespace ERAMonitor.Infrastructure.Repositories;

public class ServiceRepository : Repository<Service>, IServiceRepository
{
    public ServiceRepository(ApplicationDbContext context) : base(context)
    {
    }

    public new IQueryable<Service> Query() => base.Query();

    public async Task<Service?> GetByHostAndNameAsync(Guid hostId, ServiceType type, string serviceName)
    {
        return await _dbSet
            .FirstOrDefaultAsync(s => s.HostId == hostId && s.ServiceType == type && s.ServiceName == serviceName);
    }

    public async Task<PagedResponse<ServiceListItemDto>> GetPagedAsync(
        Guid organizationId,
        PagedRequest request,
        Guid? hostId = null,
        Guid? customerId = null,
        ServiceType? serviceType = null,
        StatusType? status = null,
        bool? monitoringEnabled = null)
    {
        var query = _dbSet
            .Include(s => s.Host)
            .ThenInclude(h => h.Customer)
            .Where(s => s.Host.OrganizationId == organizationId);

        if (hostId.HasValue)
        {
            query = query.Where(s => s.HostId == hostId.Value);
        }

        if (customerId.HasValue)
        {
            query = query.Where(s => s.Host.CustomerId == customerId.Value);
        }

        if (serviceType.HasValue)
        {
            query = query.Where(s => s.ServiceType == serviceType.Value);
        }

        if (status.HasValue)
        {
            query = query.Where(s => s.CurrentStatus == status.Value);
        }

        if (monitoringEnabled.HasValue)
        {
            query = query.Where(s => s.MonitoringEnabled == monitoringEnabled.Value);
        }

        if (!string.IsNullOrEmpty(request.Search))
        {
            var search = request.Search.ToLower();
            query = query.Where(s => 
                s.ServiceName.ToLower().Contains(search) || 
                (s.DisplayName != null && s.DisplayName.ToLower().Contains(search)) ||
                s.Host.Name.ToLower().Contains(search));
        }

        // Sorting
        query = request.SortBy?.ToLower() switch
        {
            "name" => request.SortOrder == "desc" ? query.OrderByDescending(s => s.ServiceName) : query.OrderBy(s => s.ServiceName),
            "status" => request.SortOrder == "desc" ? query.OrderByDescending(s => s.CurrentStatus) : query.OrderBy(s => s.CurrentStatus),
            "host" => request.SortOrder == "desc" ? query.OrderByDescending(s => s.Host.Name) : query.OrderBy(s => s.Host.Name),
            "type" => request.SortOrder == "desc" ? query.OrderByDescending(s => s.ServiceType) : query.OrderBy(s => s.ServiceType),
            "uptime" => request.SortOrder == "desc" ? query.OrderByDescending(s => s.LastHealthyAt) : query.OrderBy(s => s.LastHealthyAt),
            "lastseen" => request.SortOrder == "desc" ? query.OrderByDescending(s => s.UpdatedAt) : query.OrderBy(s => s.UpdatedAt),
            _ => query.OrderByDescending(s => s.LastStatusChange)
        };

        var totalCount = await query.CountAsync();
        
        var items = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(s => new ServiceListItemDto
            {
                Id = s.Id,
                HostId = s.HostId,
                HostName = s.Host.Name,
                HostOsType = s.Host.OsType,
                ServiceType = s.ServiceType,
                ServiceName = s.ServiceName,
                DisplayName = s.DisplayName,
                CurrentStatus = s.CurrentStatus,
                LastStatusChange = s.LastStatusChange,
                MonitoringEnabled = s.MonitoringEnabled,
                CustomerId = s.Host.CustomerId,
                CustomerName = s.Host.Customer != null ? s.Host.Customer.Name : null
            })
            .ToListAsync();

        return new PagedResponse<ServiceListItemDto>(items, totalCount, request.Page, request.PageSize);
    }

    public async Task<ServiceDetailDto?> GetDetailAsync(Guid id)
    {
        var service = await _dbSet
            .Include(s => s.Host)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (service == null) return null;

        return new ServiceDetailDto
        {
            Id = service.Id,
            HostId = service.HostId,
            HostName = service.Host.Name,
            ServiceType = service.ServiceType,
            ServiceName = service.ServiceName,
            DisplayName = service.DisplayName,
            Description = service.Description,
            CurrentStatus = service.CurrentStatus,
            LastStatusChange = service.LastStatusChange,
            PreviousStatus = service.PreviousStatus,
            Config = service.Config != null ? System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(service.Config) : null,
            MonitoringEnabled = service.MonitoringEnabled,
            AlertOnStop = service.AlertOnStop,
            RestartCount = service.RestartCount,
            LastRestartAt = service.LastRestartAt,
            LastHealthyAt = service.LastHealthyAt,
            CreatedAt = service.CreatedAt,
            UpdatedAt = service.UpdatedAt
        };
    }

    public async Task<List<Service>> GetByHostAsync(Guid hostId)
    {
        return await _dbSet
            .Where(s => s.HostId == hostId)
            .ToListAsync();
    }

    public async Task<List<Service>> GetByHostAsync(Guid hostId, ServiceType type)
    {
        return await _dbSet
            .Where(s => s.HostId == hostId && s.ServiceType == type)
            .ToListAsync();
    }

    public async Task<int> GetCountByStatusAsync(Guid organizationId, StatusType status)
    {
        return await _dbSet
            .CountAsync(s => s.Host.OrganizationId == organizationId && s.CurrentStatus == status);
    }

    public async Task<Dictionary<StatusType, int>> GetStatusCountsAsync(Guid organizationId)
    {
        return await _dbSet
            .Where(s => s.Host.OrganizationId == organizationId)
            .GroupBy(s => s.CurrentStatus)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Status, x => x.Count);
    }

    public async Task<Dictionary<ServiceType, int>> GetCountsByTypeAsync(Guid organizationId)
    {
        return await _dbSet
            .Where(s => s.Host.OrganizationId == organizationId)
            .GroupBy(s => s.ServiceType)
            .Select(g => new { Type = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Type, x => x.Count);
    }

    public async Task UpsertServicesAsync(Guid hostId, List<Service> services)
    {
        // Similar to disks, simplified upsert
        var existingServices = await GetByHostAsync(hostId);
        
        foreach (var service in services)
        {
            var existing = existingServices.FirstOrDefault(s => 
                s.ServiceType == service.ServiceType && 
                s.ServiceName == service.ServiceName);
            
            if (existing != null)
            {
                // Update
                existing.DisplayName = service.DisplayName ?? existing.DisplayName;
                existing.CurrentStatus = service.CurrentStatus;
                existing.Config = service.Config;
                existing.UpdatedAt = DateTime.UtcNow;
                
                if (existing.CurrentStatus != service.CurrentStatus)
                {
                    existing.PreviousStatus = existing.CurrentStatus;
                    existing.LastStatusChange = DateTime.UtcNow;
                }
                
                Update(existing);
            }
            else
            {
                // Add
                service.HostId = hostId;
                await AddAsync(service);
            }
        }
    }
}
