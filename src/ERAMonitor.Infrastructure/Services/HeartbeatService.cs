using System.Text.Json;
using Microsoft.Extensions.Logging;
using ERAMonitor.Core.DTOs.Agent;
using ERAMonitor.Core.Entities;
using ERAMonitor.Core.Enums;
using ERAMonitor.Core.Exceptions;
using ERAMonitor.Core.Interfaces.Repositories;
using ERAMonitor.Core.Interfaces.Services;
using ERAMonitor.Core.Interfaces;

namespace ERAMonitor.Infrastructure.Services;

public class HeartbeatService : IHeartbeatService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IHostMetricRepository _metricRepository;
    private readonly IHostDiskRepository _diskRepository;
    private readonly IServiceRepository _serviceRepository;
    private readonly IServiceStatusHistoryRepository _statusHistoryRepository;
    private readonly IIncidentService _incidentService;
    private readonly INotificationService _notificationService;
    private readonly IRealTimeService _realTimeService;
    private readonly IAuditService _auditService;
    private readonly ILogger<HeartbeatService> _logger;
    
    // Threshold for considering a host as down (no heartbeat)
    private const int HostDownThresholdSeconds = 90;
    
    public HeartbeatService(
        IUnitOfWork unitOfWork,
        IHostMetricRepository metricRepository,
        IHostDiskRepository diskRepository,
        IServiceRepository serviceRepository,
        IServiceStatusHistoryRepository statusHistoryRepository,
        IIncidentService incidentService,
        INotificationService notificationService,
        IRealTimeService realTimeService,
        IAuditService auditService,
        ILogger<HeartbeatService> logger)
    {
        _unitOfWork = unitOfWork;
        _metricRepository = metricRepository;
        _diskRepository = diskRepository;
        _serviceRepository = serviceRepository;
        _statusHistoryRepository = statusHistoryRepository;
        _incidentService = incidentService;
        _notificationService = notificationService;
        _realTimeService = realTimeService;
        _auditService = auditService;
        _logger = logger;
    }
    
    public async Task<HeartbeatResponse> ProcessHeartbeatAsync(string apiKey, HeartbeatRequest request)
    {
        // 1. Validate API key and get host
        var host = await _unitOfWork.Hosts.GetByApiKeyAsync(apiKey);
        
        if (host == null)
        {
            _logger.LogWarning("Heartbeat received with invalid API key: {ApiKey}", apiKey[..8] + "...");
            throw new UnauthorizedException("Invalid API key");
        }
        
        if (!host.IsActive)
        {
            _logger.LogWarning("Heartbeat received for inactive host: {HostId}", host.Id);
            throw new UnauthorizedException("Host is deactivated");
        }
        
        _logger.LogDebug("Processing heartbeat for host {HostName} ({HostId})", host.Name, host.Id);
        
        // 2. Determine host status based on metrics
        var previousStatus = host.CurrentStatus;
        var newStatus = DetermineHostStatus(host, request);
        
        // 3. Update host record
        host.LastSeenAt = DateTime.UtcNow;
        host.CurrentStatus = newStatus;
        host.AgentVersion = request.AgentVersion;
        host.LastHeartbeat = JsonSerializer.Serialize(request);
        
        // Update metrics
        host.CpuPercent = (decimal)request.System.CpuPercent;
        host.RamPercent = (decimal)request.System.RamPercent;
        host.RamUsedMb = request.System.RamUsedMb;
        host.RamTotalMb = request.System.RamTotalMb;
        host.UptimeSeconds = request.System.UptimeSeconds;
        host.ProcessCount = request.System.ProcessCount;
        
        // Update network info
        if (request.Network != null)
        {
            host.PrimaryIp = request.Network.PrimaryIp;
            host.PublicIp = request.Network.PublicIp;
        }
        
        // Update OS info if changed
        if (!string.IsNullOrEmpty(request.System.OsVersion) && host.OsVersion != request.System.OsVersion)
        {
            host.OsVersion = request.System.OsVersion;
        }
        
        // Handle status change
        if (previousStatus != newStatus)
        {
            host.PreviousStatus = previousStatus;
            host.StatusChangedAt = DateTime.UtcNow;
            
            _logger.LogInformation("Host {HostName} status changed from {OldStatus} to {NewStatus}",
                host.Name, previousStatus, newStatus);
        }
        
        _unitOfWork.Hosts.Update(host);
        
        // 4. Update/Insert disks
        await UpdateHostDisksAsync(host.Id, request.Disks);
        
        // 5. Update/Insert services
        await UpdateHostServicesAsync(host.Id, request.Services);
        
        // 6. Store metrics (time series)
        await StoreMetricsAsync(host.Id, request);
        
        // 7. Save all changes
        await _unitOfWork.SaveChangesAsync();
        
        // 8. Handle status change events (after save)
        if (previousStatus != newStatus)
        {
            await HandleHostStatusChangeAsync(host, previousStatus, newStatus);
        }
        
        // 9. Check threshold alerts
        await CheckThresholdAlertsAsync(host, request);
        
        // 10. Broadcast update via SignalR
        await BroadcastHostUpdateAsync(host);
        
        return new HeartbeatResponse
        {
            Success = true,
            HostId = host.Id.ToString(),
            NextCheckIn = host.CheckIntervalSeconds,
            Message = "Heartbeat processed successfully"
        };
    }
    
    public async Task ProcessHostDownDetectionAsync()
    {
        var threshold = DateTime.UtcNow.AddSeconds(-HostDownThresholdSeconds);
        var hosts = await _unitOfWork.Hosts.GetHostsNotSeenSinceAsync(threshold);
        
        foreach (var host in hosts)
        {
            // Skip hosts in maintenance or with monitoring disabled
            if (!host.ShouldAlert())
            {
                continue;
            }
            
            // Skip hosts already marked as down
            if (host.CurrentStatus == StatusType.Down)
            {
                continue;
            }
            
            _logger.LogWarning("Host {HostName} ({HostId}) detected as DOWN - no heartbeat since {LastSeen}",
                host.Name, host.Id, host.LastSeenAt);
            
            var previousStatus = host.CurrentStatus;
            host.PreviousStatus = previousStatus;
            host.CurrentStatus = StatusType.Down;
            host.StatusChangedAt = DateTime.UtcNow;
            
            _unitOfWork.Hosts.Update(host);
            await _unitOfWork.SaveChangesAsync();
            
            // Create incident
            await _incidentService.CreateAutoIncidentAsync(
                "Host",
                host.Id,
                host.Name,
                $"Host DOWN: {host.Name}",
                $"No heartbeat received since {host.LastSeenAt:yyyy-MM-dd HH:mm:ss} UTC",
                IncidentSeverity.Critical,
                host.CustomerId,
                host.OrganizationId
            );
            
            // Send notifications
            await _notificationService.SendHostDownNotificationAsync(host);
            
            // Broadcast update
            await BroadcastHostUpdateAsync(host);
        }
    }
    
    public async Task ProcessMaintenanceExpirationAsync()
    {
        var hosts = await _unitOfWork.Hosts.GetHostsInMaintenanceEndingAsync(DateTime.UtcNow);
        
        foreach (var host in hosts)
        {
            _logger.LogInformation("Maintenance period ended for host {HostName}", host.Name);
            
            host.MaintenanceMode = false;
            host.MaintenanceEndAt = null;
            host.MaintenanceStartAt = null;
            host.MaintenanceReason = null;
            
            _unitOfWork.Hosts.Update(host);
            
            await _auditService.LogAsync(
                "MaintenanceEnded",
                "Host",
                host.Id,
                host.Name,
                organizationId: host.OrganizationId
            );
        }
        
        await _unitOfWork.SaveChangesAsync();
    }
    
    #region Private Helper Methods
    
    private StatusType DetermineHostStatus(Host host, HeartbeatRequest request)
    {
        // Host is responding, so at minimum it's Up
        // Check for warning/degraded conditions
        
        var warnings = new List<string>();
        
        // CPU check
        if (request.System.CpuPercent >= host.CpuCriticalThreshold)
        {
            return StatusType.Warning; // Critical CPU
        }
        if (request.System.CpuPercent >= host.CpuWarningThreshold)
        {
            warnings.Add("High CPU");
        }
        
        // RAM check
        if (request.System.RamPercent >= host.RamCriticalThreshold)
        {
            return StatusType.Warning; // Critical RAM
        }
        if (request.System.RamPercent >= host.RamWarningThreshold)
        {
            warnings.Add("High RAM");
        }
        
        // Disk check
        foreach (var disk in request.Disks)
        {
            if ((decimal)disk.UsedPercent >= host.DiskCriticalThreshold)
            {
                return StatusType.Warning; // Critical disk
            }
            if ((decimal)disk.UsedPercent >= host.DiskWarningThreshold)
            {
                warnings.Add($"High disk usage on {disk.Name}");
            }
        }
        
        // Check for any services down
        var servicesDown = request.Services.Count(s => 
            s.Status.Equals("Stopped", StringComparison.OrdinalIgnoreCase) ||
            s.Status.Equals("Failed", StringComparison.OrdinalIgnoreCase));
        
        if (servicesDown > 0)
        {
            return StatusType.Degraded;
        }
        
        if (warnings.Count > 0)
        {
            return StatusType.Warning;
        }
        
        return StatusType.Up;
    }
    
    private async Task UpdateHostDisksAsync(Guid hostId, List<DiskInfoDto> disks)
    {
        var existingDisks = await _diskRepository.GetByHostAsync(hostId);
        var existingDiskNames = existingDisks.Select(d => d.Name).ToHashSet();
        var reportedDiskNames = disks.Select(d => d.Name).ToHashSet();
        
        foreach (var diskDto in disks)
        {
            var existing = existingDisks.FirstOrDefault(d => d.Name == diskDto.Name);
            
            if (existing != null)
            {
                // Update existing
                existing.MountPoint = diskDto.MountPoint;
                existing.FileSystem = diskDto.FileSystem;
                existing.TotalGb = diskDto.TotalGb;
                existing.UsedGb = diskDto.UsedGb;
                existing.UsedPercent = diskDto.UsedPercent;
                existing.UpdatedAt = DateTime.UtcNow;
                
                _diskRepository.Update(existing);
            }
            else
            {
                // Add new disk
                var newDisk = new HostDisk
                {
                    HostId = hostId,
                    Name = diskDto.Name,
                    MountPoint = diskDto.MountPoint,
                    FileSystem = diskDto.FileSystem,
                    TotalGb = diskDto.TotalGb,
                    UsedGb = diskDto.UsedGb,
                    UsedPercent = diskDto.UsedPercent
                };
                
                await _diskRepository.AddAsync(newDisk);
            }
        }
        
        // Remove disks no longer reported
        var removedDisks = existingDisks.Where(d => !reportedDiskNames.Contains(d.Name));
        foreach (var disk in removedDisks)
        {
            _diskRepository.Remove(disk);
        }
        
        await _diskRepository.SaveChangesAsync();
    }
    
    private async Task UpdateHostServicesAsync(Guid hostId, List<ServiceInfoDto> services)
    {
        var existingServices = await _serviceRepository.GetByHostAsync(hostId);
        
        foreach (var serviceDto in services)
        {
            if (!Enum.TryParse<ServiceType>(serviceDto.Type, true, out var serviceType))
            {
                _logger.LogWarning("Unknown service type: {Type}", serviceDto.Type);
                continue;
            }
            
            var existing = existingServices.FirstOrDefault(s => 
                s.ServiceType == serviceType && s.ServiceName == serviceDto.Name);
            
            var newStatus = MapServiceStatus(serviceDto.Status);
            
            if (existing != null)
            {
                var previousStatus = existing.CurrentStatus;
                
                // Update existing
                existing.DisplayName = serviceDto.DisplayName ?? existing.DisplayName;
                existing.CurrentStatus = newStatus;
                existing.Config = serviceDto.Config != null 
                    ? JsonSerializer.Serialize(serviceDto.Config) 
                    : existing.Config;
                existing.UpdatedAt = DateTime.UtcNow;
                
                // Track status change
                if (previousStatus != newStatus)
                {
                    existing.PreviousStatus = previousStatus;
                    existing.LastStatusChange = DateTime.UtcNow;
                    
                    if (newStatus == StatusType.Up)
                    {
                        existing.LastHealthyAt = DateTime.UtcNow;
                    }
                    
                    // Record status history
                    await _statusHistoryRepository.AddAsync(new ServiceStatusHistory
                    {
                        ServiceId = existing.Id,
                        Status = newStatus,
                        Message = $"Status changed from {previousStatus} to {newStatus}"
                    });
                    
                    // Handle service down
                    if (previousStatus != StatusType.Down && newStatus == StatusType.Down)
                    {
                        await HandleServiceDownAsync(existing);
                    }
                    else if (previousStatus == StatusType.Down && newStatus == StatusType.Up)
                    {
                        await HandleServiceRecoveredAsync(existing);
                    }
                }
                
                _serviceRepository.Update(existing);
            }
            else
            {
                // Add new service
                var newService = new Service
                {
                    HostId = hostId,
                    ServiceType = serviceType,
                    ServiceName = serviceDto.Name,
                    DisplayName = serviceDto.DisplayName,
                    CurrentStatus = newStatus,
                    LastStatusChange = DateTime.UtcNow,
                    Config = serviceDto.Config != null ? JsonSerializer.Serialize(serviceDto.Config) : null,
                    MonitoringEnabled = true,
                    AlertOnStop = true
                };
                
                if (newStatus == StatusType.Up)
                {
                    newService.LastHealthyAt = DateTime.UtcNow;
                }
                
                await _serviceRepository.AddAsync(newService);
                
                // Record initial status
                await _statusHistoryRepository.AddAsync(new ServiceStatusHistory
                {
                    ServiceId = newService.Id,
                    Status = newStatus,
                    Message = "Service discovered"
                });
            }
        }
        
        await _statusHistoryRepository.SaveChangesAsync();
    }
    
    private async Task StoreMetricsAsync(Guid hostId, HeartbeatRequest request)
    {
        var metric = new HostMetric
        {
            HostId = hostId,
            CpuPercent = (decimal)request.System.CpuPercent,
            RamPercent = (decimal)request.System.RamPercent,
            RamUsedMb = request.System.RamUsedMb,
            RamTotalMb = request.System.RamTotalMb,
            UptimeSeconds = request.System.UptimeSeconds,
            ProcessCount = request.System.ProcessCount,
            DiskInfo = JsonSerializer.Serialize(request.Disks),
            NetworkInBytes = request.Network?.InBytes,
            NetworkOutBytes = request.Network?.OutBytes,
            RecordedAt = DateTime.UtcNow
        };
        
        await _metricRepository.AddAsync(metric);
        await _metricRepository.SaveChangesAsync();
    }
    
    private async Task HandleHostStatusChangeAsync(Host host, StatusType previousStatus, StatusType newStatus)
    {
        _logger.LogInformation("Host {HostName} status changed: {OldStatus} -> {NewStatus}",
            host.Name, previousStatus, newStatus);
        
        // Host came back online
        if (previousStatus == StatusType.Down && newStatus == StatusType.Up)
        {
            // Auto-resolve related incidents
            await _incidentService.AutoResolveIncidentsAsync("Host", host.Id, "Host is back online");
            
            // Send recovery notification
            await _notificationService.SendHostRecoveredNotificationAsync(host);
        }
        // Host went down (shouldn't happen here as we get heartbeat, but for status change tracking)
        else if (previousStatus != StatusType.Down && newStatus == StatusType.Down)
        {
            if (host.ShouldAlert())
            {
                await _incidentService.CreateAutoIncidentAsync(
                    "Host",
                    host.Id,
                    host.Name,
                    $"Host DOWN: {host.Name}",
                    "Host stopped responding",
                    IncidentSeverity.Critical,
                    host.CustomerId,
                    host.OrganizationId
                );
            }
        }
        // Status degradation
        else if (newStatus == StatusType.Warning || newStatus == StatusType.Degraded)
        {
            // Could create warning-level incident if configured
        }
    }
    
    private async Task HandleServiceDownAsync(Service service)
    {
        if (!service.MonitoringEnabled || !service.AlertOnStop)
        {
            return;
        }
        
        var host = await _unitOfWork.Hosts.GetByIdAsync(service.HostId);
        if (host == null || host.IsInMaintenance())
        {
            return;
        }
        
        _logger.LogWarning("Service {ServiceName} on host {HostId} is DOWN", 
            service.DisplayName ?? service.ServiceName, service.HostId);
        
        await _incidentService.CreateAutoIncidentAsync(
            "Service",
            service.Id,
            service.DisplayName ?? service.ServiceName,
            $"Service Stopped: {service.DisplayName ?? service.ServiceName}",
            $"Service {service.ServiceName} on host {host.Name} has stopped",
            IncidentSeverity.High,
            host.CustomerId,
            host.OrganizationId
        );
        
        await _notificationService.SendServiceDownNotificationAsync(service, host);
    }
    
    private async Task HandleServiceRecoveredAsync(Service service)
    {
        _logger.LogInformation("Service {ServiceName} recovered", 
            service.DisplayName ?? service.ServiceName);
        
        // Auto-resolve related incidents
        await _incidentService.AutoResolveIncidentsAsync("Service", service.Id, "Service is back online");
        
        var host = await _unitOfWork.Hosts.GetByIdAsync(service.HostId);
        if (host != null)
        {
            await _notificationService.SendServiceRecoveredNotificationAsync(service, host);
        }
    }
    
    private async Task CheckThresholdAlertsAsync(Host host, HeartbeatRequest request)
    {
        // Check CPU threshold
        if (host.AlertOnHighCpu && request.System.CpuPercent >= host.CpuCriticalThreshold)
        {
            // Could create alert or incident
        }
        
        // Check RAM threshold
        if (host.AlertOnHighRam && request.System.RamPercent >= host.RamCriticalThreshold)
        {
            // Could create alert or incident
        }
        
        // Check Disk thresholds
        if (host.AlertOnHighDisk)
        {
            foreach (var disk in request.Disks)
            {
                if ((decimal)disk.UsedPercent >= host.DiskCriticalThreshold)
                {
                    // Could create alert or incident
                }
            }
        }
    }
    
    private async Task BroadcastHostUpdateAsync(Host host)
    {
        var update = new
        {
            hostId = host.Id,
            hostName = host.Name,
            currentStatus = host.CurrentStatus.ToString(),
            cpuPercent = host.CpuPercent,
            ramPercent = host.RamPercent,
            lastSeenAt = host.LastSeenAt,
            statusChangedAt = host.StatusChangedAt
        };
        
        await _realTimeService.BroadcastHostUpdateAsync(update, host.CustomerId);
    }
    
    private static StatusType MapServiceStatus(string status)
    {
        return status.ToLowerInvariant() switch
        {
            "running" => StatusType.Up,
            "started" => StatusType.Up,
            "active" => StatusType.Up,
            "stopped" => StatusType.Down,
            "failed" => StatusType.Down,
            "inactive" => StatusType.Down,
            "starting" => StatusType.Warning,
            "stopping" => StatusType.Warning,
            "paused" => StatusType.Warning,
            "degraded" => StatusType.Degraded,
            _ => StatusType.Unknown
        };
    }
    
    #endregion
}
