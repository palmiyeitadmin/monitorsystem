using Microsoft.EntityFrameworkCore;
using ERAMonitor.Core.DTOs.Common;
using ERAMonitor.Core.DTOs.Hosts;
using ERAMonitor.Core.Entities;
using ERAMonitor.Core.Enums;
using ERAMonitor.Core.Interfaces.Repositories;
using ERAMonitor.Infrastructure.Data;
using ERAMonitor.Infrastructure.Data.Repositories;

namespace ERAMonitor.Infrastructure.Repositories;

public class HostRepository : Repository<Host>, IHostRepository
{
    public HostRepository(ApplicationDbContext context) : base(context)
    {
    }

    public new IQueryable<Host> Query() => base.Query();

    public async Task<Host?> GetByApiKeyAsync(string apiKey)
    {
        return await _dbSet
            .Include(h => h.Disks)
            .Include(h => h.Services)
            .FirstOrDefaultAsync(h => h.ApiKey == apiKey);
    }

    public async Task<bool> ApiKeyExistsAsync(string apiKey, Guid? excludeId = null)
    {
        var query = _dbSet.AsQueryable();
        
        if (excludeId.HasValue)
        {
            query = query.Where(h => h.Id != excludeId.Value);
        }
        
        return await query.AnyAsync(h => h.ApiKey == apiKey);
    }

    public async Task<PagedResponse<HostListItemDto>> GetPagedAsync(
        Guid organizationId,
        PagedRequest request,
        StatusType? status = null,
        Guid? customerId = null,
        Guid? locationId = null,
        OsType? osType = null,
        string[]? tags = null,
        bool? monitoringEnabled = null,
        bool? isActive = null)
    {
        var query = _dbSet
            .Include(h => h.Customer)
            .Include(h => h.Location)
            .Include(h => h.Services)
            .Where(h => h.OrganizationId == organizationId);

        if (status.HasValue)
        {
            query = query.Where(h => h.CurrentStatus == status.Value);
        }

        if (customerId.HasValue)
        {
            query = query.Where(h => h.CustomerId == customerId.Value);
        }

        if (locationId.HasValue)
        {
            query = query.Where(h => h.LocationId == locationId.Value);
        }

        if (osType.HasValue)
        {
            query = query.Where(h => h.OsType == osType.Value);
        }

        if (monitoringEnabled.HasValue)
        {
            query = query.Where(h => h.MonitoringEnabled == monitoringEnabled.Value);
        }

        if (isActive.HasValue)
        {
            query = query.Where(h => h.IsActive == isActive.Value);
        }
        else
        {
            // Default to active only unless specified
            query = query.Where(h => h.IsActive);
        }

        if (tags != null && tags.Length > 0)
        {
            foreach (var tag in tags)
            {
                query = query.Where(h => h.Tags.Contains(tag));
            }
        }

        if (!string.IsNullOrEmpty(request.Search))
        {
            var search = request.Search.ToLower();
            query = query.Where(h => 
                h.Name.ToLower().Contains(search) || 
                (h.Hostname != null && h.Hostname.ToLower().Contains(search)) ||
                (h.PrimaryIp != null && h.PrimaryIp.Contains(search)) ||
                (h.PublicIp != null && h.PublicIp.Contains(search)));
        }

        // Sorting
        query = request.SortBy?.ToLower() switch
        {
            "name" => request.SortOrder == "desc" ? query.OrderByDescending(h => h.Name) : query.OrderBy(h => h.Name),
            "status" => request.SortOrder == "desc" ? query.OrderByDescending(h => h.CurrentStatus) : query.OrderBy(h => h.CurrentStatus),
            "cpu" => request.SortOrder == "desc" ? query.OrderByDescending(h => h.CpuPercent) : query.OrderBy(h => h.CpuPercent),
            "ram" => request.SortOrder == "desc" ? query.OrderByDescending(h => h.RamPercent) : query.OrderBy(h => h.RamPercent),
            "uptime" => request.SortOrder == "desc" ? query.OrderByDescending(h => h.UptimeSeconds) : query.OrderBy(h => h.UptimeSeconds),
            "lastseen" => request.SortOrder == "desc" ? query.OrderByDescending(h => h.LastSeenAt) : query.OrderBy(h => h.LastSeenAt),
            _ => query.OrderByDescending(h => h.CreatedAt)
        };

        var totalCount = await query.CountAsync();
        
        var items = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(h => new HostListItemDto
            {
                Id = h.Id,
                Name = h.Name,
                Hostname = h.Hostname,
                OsType = h.OsType,
                Category = h.Category,
                Tags = h.Tags,
                CurrentStatus = h.CurrentStatus,
                StatusDisplay = h.GetStatusDisplay(),
                LastSeenAt = h.LastSeenAt,
                MonitoringEnabled = h.MonitoringEnabled,
                MaintenanceMode = h.MaintenanceMode,
                CpuPercent = h.CpuPercent,
                RamPercent = h.RamPercent,
                UptimeSeconds = h.UptimeSeconds,
                Customer = h.Customer != null ? new CustomerSummaryDto 
                { 
                    Id = h.Customer.Id, 
                    Name = h.Customer.Name,
                    LogoUrl = h.Customer.LogoUrl
                } : null,
                Location = h.Location != null ? new LocationSummaryDto 
                { 
                    Id = h.Location.Id, 
                    Name = h.Location.Name,
                    City = h.Location.City,
                    Country = h.Location.Country
                } : null,
                ServiceCount = h.Services.Count,
                ServicesUp = h.Services.Count(s => s.CurrentStatus == StatusType.Up),
                ServicesDown = h.Services.Count(s => s.CurrentStatus == StatusType.Down),
                CreatedAt = h.CreatedAt
            })
            .ToListAsync();

        return new PagedResponse<HostListItemDto>(items, totalCount, request.Page, request.PageSize);
    }

    public async Task<HostDetailDto?> GetDetailAsync(Guid id, Guid organizationId)
    {
        var host = await _dbSet
            .Include(h => h.Customer)
            .Include(h => h.Location)
            .Include(h => h.Disks)
            .Include(h => h.Services)
            .FirstOrDefaultAsync(h => h.Id == id && h.OrganizationId == organizationId);

        if (host == null) return null;

        return new HostDetailDto
        {
            Id = host.Id,
            Name = host.Name,
            Hostname = host.Hostname,
            Description = host.Description,
            OsType = host.OsType,
            OsVersion = host.OsVersion,
            Category = host.Category,
            Tags = host.Tags,
            PrimaryIp = host.PrimaryIp,
            PublicIp = host.PublicIp,
            ApiKey = host.ApiKey,
            AgentVersion = host.AgentVersion,
            AgentInstalledAt = host.AgentInstalledAt,
            CheckIntervalSeconds = host.CheckIntervalSeconds,
            CurrentStatus = host.CurrentStatus,
            StatusDisplay = host.GetStatusDisplay(),
            LastSeenAt = host.LastSeenAt,
            StatusChangedAt = host.StatusChangedAt,
            Metrics = new HostCurrentMetricsDto
            {
                CpuPercent = host.CpuPercent,
                RamPercent = host.RamPercent,
                RamUsedMb = host.RamUsedMb,
                RamTotalMb = host.RamTotalMb,
                UptimeSeconds = host.UptimeSeconds,
                ProcessCount = host.ProcessCount
            },
            Disks = host.Disks.Select(d => new HostDiskDto
            {
                Id = d.Id,
                Name = d.Name,
                MountPoint = d.MountPoint,
                FileSystem = d.FileSystem,
                TotalGb = d.TotalGb,
                UsedGb = d.UsedGb,
                FreeGb = d.FreeGb,
                UsedPercent = d.UsedPercent,
                UpdatedAt = d.UpdatedAt
            }).ToList(),
            Thresholds = new HostThresholdsDto
            {
                CpuWarning = host.CpuWarningThreshold,
                CpuCritical = host.CpuCriticalThreshold,
                RamWarning = host.RamWarningThreshold,
                RamCritical = host.RamCriticalThreshold,
                DiskWarning = host.DiskWarningThreshold,
                DiskCritical = host.DiskCriticalThreshold
            },
            MonitoringSettings = new HostMonitoringSettingsDto
            {
                MonitoringEnabled = host.MonitoringEnabled,
                AlertOnDown = host.AlertOnDown,
                AlertDelaySeconds = host.AlertDelaySeconds,
                AlertOnHighCpu = host.AlertOnHighCpu,
                AlertOnHighRam = host.AlertOnHighRam,
                AlertOnHighDisk = host.AlertOnHighDisk
            },
            Maintenance = new HostMaintenanceDto
            {
                InMaintenance = host.MaintenanceMode,
                StartAt = host.MaintenanceStartAt,
                EndAt = host.MaintenanceEndAt,
                Reason = host.MaintenanceReason
            },
            Customer = host.Customer != null ? new CustomerSummaryDto
            {
                Id = host.Customer.Id,
                Name = host.Customer.Name,
                LogoUrl = host.Customer.LogoUrl
            } : null,
            Location = host.Location != null ? new LocationSummaryDto
            {
                Id = host.Location.Id,
                Name = host.Location.Name,
                City = host.Location.City,
                Country = host.Location.Country
            } : null,
            Statistics = new HostStatisticsDto
            {
                ServiceCount = host.Services.Count,
                ServicesUp = host.Services.Count(s => s.CurrentStatus == StatusType.Up),
                ServicesDown = host.Services.Count(s => s.CurrentStatus == StatusType.Down),
                CheckCount = host.Checks.Count
            },
            Notes = host.Notes,
            IsActive = host.IsActive,
            CreatedAt = host.CreatedAt,
            UpdatedAt = host.UpdatedAt
        };
    }

    public async Task<List<Host>> GetByCustomerAsync(Guid customerId)
    {
        return await _dbSet
            .Where(h => h.CustomerId == customerId && h.IsActive)
            .ToListAsync();
    }

    public async Task<List<Host>> GetByLocationAsync(Guid locationId)
    {
        return await _dbSet
            .Where(h => h.LocationId == locationId && h.IsActive)
            .ToListAsync();
    }

    public async Task<List<Host>> GetHostsNotSeenSinceAsync(DateTime threshold)
    {
        return await _dbSet
            .Where(h => h.IsActive && 
                        h.MonitoringEnabled && 
                        !h.MaintenanceMode &&
                        h.LastSeenAt.HasValue && 
                        h.LastSeenAt.Value < threshold &&
                        h.CurrentStatus != StatusType.Down)
            .ToListAsync();
    }

    public async Task<List<Host>> GetHostsInMaintenanceEndingAsync(DateTime before)
    {
        return await _dbSet
            .Where(h => h.IsActive && 
                        h.MaintenanceMode && 
                        h.MaintenanceEndAt.HasValue && 
                        h.MaintenanceEndAt.Value < before)
            .ToListAsync();
    }

    public async Task<int> GetCountByStatusAsync(Guid organizationId, StatusType status)
    {
        return await _dbSet
            .CountAsync(h => h.OrganizationId == organizationId && 
                             h.IsActive && 
                             h.CurrentStatus == status);
    }

    public async Task<Dictionary<StatusType, int>> GetStatusCountsAsync(Guid organizationId)
    {
        return await _dbSet
            .Where(h => h.OrganizationId == organizationId && h.IsActive)
            .GroupBy(h => h.CurrentStatus)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Status, x => x.Count);
    }

    public async Task<List<HostListItemDto>> GetRecentlyDownAsync(Guid organizationId, int limit = 10)
    {
        return await _dbSet
            .Where(h => h.OrganizationId == organizationId && 
                        h.IsActive && 
                        h.CurrentStatus == StatusType.Down)
            .OrderByDescending(h => h.StatusChangedAt)
            .Take(limit)
            .Select(h => new HostListItemDto
            {
                Id = h.Id,
                Name = h.Name,
                Hostname = h.Hostname,
                OsType = h.OsType,
                Category = h.Category,
                CurrentStatus = h.CurrentStatus,
                StatusDisplay = h.GetStatusDisplay(),
                LastSeenAt = h.LastSeenAt,
                MonitoringEnabled = h.MonitoringEnabled,
                MaintenanceMode = h.MaintenanceMode,
                CreatedAt = h.CreatedAt
            })
            .ToListAsync();
    }

    public async Task<List<HostListItemDto>> GetHighResourceUsageAsync(Guid organizationId, int cpuThreshold = 90, int ramThreshold = 90, int limit = 10)
    {
        return await _dbSet
            .Where(h => h.OrganizationId == organizationId && 
                        h.IsActive && 
                        h.CurrentStatus == StatusType.Up &&
                        (h.CpuPercent >= cpuThreshold || h.RamPercent >= ramThreshold))
            .OrderByDescending(h => h.CpuPercent)
            .Take(limit)
            .Select(h => new HostListItemDto
            {
                Id = h.Id,
                Name = h.Name,
                Hostname = h.Hostname,
                OsType = h.OsType,
                Category = h.Category,
                CurrentStatus = h.CurrentStatus,
                StatusDisplay = h.GetStatusDisplay(),
                LastSeenAt = h.LastSeenAt,
                CpuPercent = h.CpuPercent,
                RamPercent = h.RamPercent,
                MonitoringEnabled = h.MonitoringEnabled,
                MaintenanceMode = h.MaintenanceMode,
                CreatedAt = h.CreatedAt
            })
            .ToListAsync();
    }
}
