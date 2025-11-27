using Microsoft.Extensions.Logging;
using ERAMonitor.Core.DTOs.Common;
using ERAMonitor.Core.DTOs.Hosts;
using ERAMonitor.Core.Entities;
using ERAMonitor.Core.Enums;
using ERAMonitor.Core.Exceptions;
using ERAMonitor.Core.Interfaces.Repositories;
using ERAMonitor.Core.Interfaces.Services;

namespace ERAMonitor.Infrastructure.Services;

public class HostService : IHostService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IHostMetricRepository _metricRepository;
    private readonly IAuditService _auditService;
    private readonly ILogger<HostService> _logger;
    
    public HostService(
        IUnitOfWork unitOfWork,
        IHostMetricRepository metricRepository,
        IAuditService auditService,
        ILogger<HostService> logger)
    {
        _unitOfWork = unitOfWork;
        _metricRepository = metricRepository;
        _auditService = auditService;
        _logger = logger;
    }
    
    public async Task<PagedResponse<HostListItemDto>> GetPagedAsync(
        Guid organizationId,
        PagedRequest request,
        StatusType? status = null,
        Guid? customerId = null,
        Guid? locationId = null,
        OsType? osType = null,
        string[]? tags = null)
    {
        return await _unitOfWork.Hosts.GetPagedAsync(
            organizationId,
            request,
            status,
            customerId,
            locationId,
            osType,
            tags
        );
    }
    
    public async Task<HostDetailDto> GetByIdAsync(Guid id, Guid organizationId)
    {
        var host = await _unitOfWork.Hosts.GetDetailAsync(id, organizationId);
        
        if (host == null)
        {
            throw new NotFoundException($"Host with ID {id} not found");
        }
        
        return host;
    }
    
    public async Task<HostDetailDto> CreateAsync(Guid organizationId, CreateHostRequest request)
    {
        // Validate customer if provided
        if (request.CustomerId.HasValue)
        {
            var customer = await _unitOfWork.Customers.GetByIdAsync(request.CustomerId.Value);
            if (customer == null || customer.OrganizationId != organizationId)
            {
                throw new BusinessException("Invalid customer ID", "INVALID_CUSTOMER");
            }
        }
        
        // Validate location if provided
        if (request.LocationId.HasValue)
        {
            var location = await _unitOfWork.Locations.GetByIdAsync(request.LocationId.Value);
            if (location == null || location.OrganizationId != organizationId)
            {
                throw new BusinessException("Invalid location ID", "INVALID_LOCATION");
            }
        }
        
        var host = new Host
        {
            OrganizationId = organizationId,
            Name = request.Name,
            Hostname = request.Hostname,
            Description = request.Description,
            OsType = request.OsType,
            OsVersion = request.OsVersion,
            Category = request.Category,
            Tags = request.Tags ?? Array.Empty<string>(),
            PrimaryIp = request.PrimaryIp,
            PublicIp = request.PublicIp,
            CustomerId = request.CustomerId,
            LocationId = request.LocationId,
            CheckIntervalSeconds = request.CheckIntervalSeconds,
            MonitoringEnabled = request.MonitoringEnabled,
            AlertOnDown = request.AlertOnDown,
            AlertDelaySeconds = request.AlertDelaySeconds,
            AlertOnHighCpu = request.AlertOnHighCpu,
            AlertOnHighRam = request.AlertOnHighRam,
            AlertOnHighDisk = request.AlertOnHighDisk,
            CpuWarningThreshold = request.CpuWarningThreshold,
            CpuCriticalThreshold = request.CpuCriticalThreshold,
            RamWarningThreshold = request.RamWarningThreshold,
            RamCriticalThreshold = request.RamCriticalThreshold,
            DiskWarningThreshold = request.DiskWarningThreshold,
            DiskCriticalThreshold = request.DiskCriticalThreshold,
            Notes = request.Notes,
            CurrentStatus = StatusType.Unknown
        };
        
        await _unitOfWork.Hosts.AddAsync(host);
        await _unitOfWork.SaveChangesAsync();
        
        await _auditService.LogCreateAsync(host);
        
        _logger.LogInformation("Host created: {HostName} ({HostId})", host.Name, host.Id);
        
        return await GetByIdAsync(host.Id, organizationId);
    }
    
    public async Task<HostDetailDto> UpdateAsync(Guid id, Guid organizationId, UpdateHostRequest request)
    {
        var host = await _unitOfWork.Hosts.GetByIdAsync(id);
        
        if (host == null || host.OrganizationId != organizationId)
        {
            throw new NotFoundException($"Host with ID {id} not found");
        }
        
        var oldValues = new
        {
            host.Name,
            host.Hostname,
            host.Description,
            host.OsType,
            host.Category,
            host.CustomerId,
            host.LocationId,
            host.MonitoringEnabled
        };
        
        // Validate customer if changed
        if (request.CustomerId != host.CustomerId && request.CustomerId.HasValue)
        {
            var customer = await _unitOfWork.Customers.GetByIdAsync(request.CustomerId.Value);
            if (customer == null || customer.OrganizationId != organizationId)
            {
                throw new BusinessException("Invalid customer ID", "INVALID_CUSTOMER");
            }
        }
        
        // Update host
        host.Name = request.Name;
        host.Hostname = request.Hostname;
        host.Description = request.Description;
        host.OsType = request.OsType;
        host.OsVersion = request.OsVersion;
        host.Category = request.Category;
        host.Tags = request.Tags ?? Array.Empty<string>();
        host.PrimaryIp = request.PrimaryIp;
        host.PublicIp = request.PublicIp;
        host.CustomerId = request.CustomerId;
        host.LocationId = request.LocationId;
        host.CheckIntervalSeconds = request.CheckIntervalSeconds;
        host.MonitoringEnabled = request.MonitoringEnabled;
        host.AlertOnDown = request.AlertOnDown;
        host.AlertDelaySeconds = request.AlertDelaySeconds;
        host.AlertOnHighCpu = request.AlertOnHighCpu;
        host.AlertOnHighRam = request.AlertOnHighRam;
        host.AlertOnHighDisk = request.AlertOnHighDisk;
        host.CpuWarningThreshold = request.CpuWarningThreshold;
        host.CpuCriticalThreshold = request.CpuCriticalThreshold;
        host.RamWarningThreshold = request.RamWarningThreshold;
        host.RamCriticalThreshold = request.RamCriticalThreshold;
        host.DiskWarningThreshold = request.DiskWarningThreshold;
        host.DiskCriticalThreshold = request.DiskCriticalThreshold;
        host.Notes = request.Notes;
        host.IsActive = request.IsActive;
        
        _unitOfWork.Hosts.Update(host);
        await _unitOfWork.SaveChangesAsync();
        
        await _auditService.LogUpdateAsync(host, oldValues);
        
        return await GetByIdAsync(id, organizationId);
    }
    
    public async Task DeleteAsync(Guid id, Guid organizationId)
    {
        var host = await _unitOfWork.Hosts.GetByIdAsync(id);
        
        if (host == null || host.OrganizationId != organizationId)
        {
            throw new NotFoundException($"Host with ID {id} not found");
        }
        
        // Soft delete
        host.IsActive = false;
        host.MonitoringEnabled = false;
        
        _unitOfWork.Hosts.Update(host);
        await _unitOfWork.SaveChangesAsync();
        
        await _auditService.LogDeleteAsync(host);
        
        _logger.LogInformation("Host deleted: {HostName} ({HostId})", host.Name, host.Id);
    }
    
    public async Task<string> RegenerateApiKeyAsync(Guid id, Guid organizationId)
    {
        var host = await _unitOfWork.Hosts.GetByIdAsync(id);
        
        if (host == null || host.OrganizationId != organizationId)
        {
            throw new NotFoundException($"Host with ID {id} not found");
        }
        
        var oldKey = host.ApiKey;
        host.ApiKey = Host.GenerateApiKey();
        
        _unitOfWork.Hosts.Update(host);
        await _unitOfWork.SaveChangesAsync();
        
        await _auditService.LogAsync(
            "RegenerateApiKey",
            "Host",
            host.Id,
            host.Name,
            new { OldKeyPrefix = oldKey[..8] + "..." },
            new { NewKeyPrefix = host.ApiKey[..8] + "..." },
            organizationId: organizationId
        );
        
        _logger.LogInformation("API key regenerated for host: {HostName}", host.Name);
        
        return host.ApiKey;
    }
    
    public async Task<HostDetailDto> SetMaintenanceAsync(Guid id, Guid organizationId, SetMaintenanceRequest request)
    {
        var host = await _unitOfWork.Hosts.GetByIdAsync(id);
        
        if (host == null || host.OrganizationId != organizationId)
        {
            throw new NotFoundException($"Host with ID {id} not found");
        }
        
        var oldValues = new
        {
            host.MaintenanceMode,
            host.MaintenanceStartAt,
            host.MaintenanceEndAt,
            host.MaintenanceReason
        };
        
        host.MaintenanceMode = request.Enable;
        host.MaintenanceStartAt = request.Enable ? (request.StartAt ?? DateTime.UtcNow) : null;
        host.MaintenanceEndAt = request.Enable ? request.EndAt : null;
        host.MaintenanceReason = request.Enable ? request.Reason : null;
        
        _unitOfWork.Hosts.Update(host);
        await _unitOfWork.SaveChangesAsync();
        
        await _auditService.LogAsync(
            request.Enable ? "MaintenanceEnabled" : "MaintenanceDisabled",
            "Host",
            host.Id,
            host.Name,
            oldValues,
            new { host.MaintenanceMode, host.MaintenanceStartAt, host.MaintenanceEndAt, host.MaintenanceReason },
            organizationId: organizationId
        );
        
        _logger.LogInformation("Maintenance mode {Action} for host: {HostName}",
            request.Enable ? "enabled" : "disabled", host.Name);
        
        return await GetByIdAsync(id, organizationId);
    }
    
    public async Task<bool> ToggleMonitoringAsync(Guid id, Guid organizationId)
    {
        var host = await _unitOfWork.Hosts.GetByIdAsync(id);
        
        if (host == null || host.OrganizationId != organizationId)
        {
            throw new NotFoundException($"Host with ID {id} not found");
        }
        
        host.MonitoringEnabled = !host.MonitoringEnabled;
        
        _unitOfWork.Hosts.Update(host);
        await _unitOfWork.SaveChangesAsync();
        
        await _auditService.LogAsync(
            "ToggleMonitoring",
            "Host",
            host.Id,
            host.Name,
            newValues: new { host.MonitoringEnabled },
            organizationId: organizationId
        );
        
        return host.MonitoringEnabled;
    }
    
    public async Task<HostMetricsDto> GetMetricsAsync(
        Guid id, 
        Guid organizationId, 
        DateTime from, 
        DateTime to, 
        string interval = "5m")
    {
        var host = await _unitOfWork.Hosts.GetByIdAsync(id);
        
        if (host == null || host.OrganizationId != organizationId)
        {
            throw new NotFoundException($"Host with ID {id} not found");
        }
        
        return await _metricRepository.GetMetricsHistoryAsync(id, from, to, interval);
    }
    
    public async Task<Dictionary<StatusType, int>> GetStatusCountsAsync(Guid organizationId)
    {
        return await _unitOfWork.Hosts.GetStatusCountsAsync(organizationId);
    }
    
    public async Task<List<HostListItemDto>> GetRecentlyDownAsync(Guid organizationId, int limit = 10)
    {
        return await _unitOfWork.Hosts.GetRecentlyDownAsync(organizationId, limit);
    }
    
    public async Task<List<HostListItemDto>> GetHighResourceUsageAsync(Guid organizationId, int limit = 10)
    {
        return await _unitOfWork.Hosts.GetHighResourceUsageAsync(organizationId, 90, 90, limit);
    }
}
