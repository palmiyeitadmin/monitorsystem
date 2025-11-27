using Microsoft.Extensions.Logging;
using ERAMonitor.Core.DTOs.Common;
using ERAMonitor.Core.DTOs.Services;
using ERAMonitor.Core.Enums;
using ERAMonitor.Core.Exceptions;
using ERAMonitor.Core.Interfaces.Repositories;
using ERAMonitor.Core.Interfaces.Services;

namespace ERAMonitor.Infrastructure.Services;

public class ServiceMonitorService : IServiceMonitorService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditService _auditService;
    private readonly ILogger<ServiceMonitorService> _logger;
    
    public ServiceMonitorService(
        IUnitOfWork unitOfWork,
        IAuditService auditService,
        ILogger<ServiceMonitorService> logger)
    {
        _unitOfWork = unitOfWork;
        _auditService = auditService;
        _logger = logger;
    }
    
    public async Task<PagedResponse<ServiceListItemDto>> GetPagedAsync(
        Guid organizationId,
        PagedRequest request,
        Guid? hostId = null,
        Guid? customerId = null,
        ServiceType? serviceType = null,
        StatusType? status = null)
    {
        return await _unitOfWork.Services.GetPagedAsync(
            organizationId,
            request,
            hostId,
            customerId,
            serviceType,
            status
        );
    }
    
    public async Task<ServiceDetailDto> GetByIdAsync(Guid id)
    {
        var service = await _unitOfWork.Services.GetDetailAsync(id);
        
        if (service == null)
        {
            throw new NotFoundException($"Service with ID {id} not found");
        }
        
        return service;
    }
    
    public async Task<ServiceDetailDto> UpdateAsync(Guid id, UpdateServiceRequest request)
    {
        var service = await _unitOfWork.Services.GetByIdAsync(id);
        
        if (service == null)
        {
            throw new NotFoundException($"Service with ID {id} not found");
        }
        
        var oldValues = new
        {
            service.DisplayName,
            service.Description,
            service.MonitoringEnabled,
            service.AlertOnStop
        };
        
        service.DisplayName = request.DisplayName ?? service.DisplayName;
        service.Description = request.Description;
        service.MonitoringEnabled = request.MonitoringEnabled;
        service.AlertOnStop = request.AlertOnStop;
        
        _unitOfWork.Services.Update(service);
        await _unitOfWork.SaveChangesAsync();
        
        await _auditService.LogUpdateAsync(service, oldValues);
        
        return await GetByIdAsync(id);
    }
    
    public async Task<bool> ToggleMonitoringAsync(Guid id)
    {
        var service = await _unitOfWork.Services.GetByIdAsync(id);
        
        if (service == null)
        {
            throw new NotFoundException($"Service with ID {id} not found");
        }
        
        service.MonitoringEnabled = !service.MonitoringEnabled;
        
        _unitOfWork.Services.Update(service);
        await _unitOfWork.SaveChangesAsync();
        
        await _auditService.LogAsync(
            "ToggleMonitoring",
            "Service",
            service.Id,
            service.DisplayName ?? service.ServiceName,
            newValues: new { service.MonitoringEnabled }
        );
        
        return service.MonitoringEnabled;
    }
    
    public async Task<List<ServiceStatusHistoryDto>> GetStatusHistoryAsync(Guid id, DateTime from, DateTime to)
    {
        var service = await _unitOfWork.Services.GetByIdAsync(id);
        
        if (service == null)
        {
            throw new NotFoundException($"Service with ID {id} not found");
        }
        
        var history = await _unitOfWork.ServiceStatusHistory.GetByServiceAsync(id, from, to);
        
        return history.Select(h => new ServiceStatusHistoryDto
        {
            Status = h.Status,
            Message = h.Message,
            RecordedAt = h.RecordedAt
        }).ToList();
    }
    
    public async Task<Dictionary<StatusType, int>> GetStatusCountsAsync(Guid organizationId)
    {
        return await _unitOfWork.Services.GetStatusCountsAsync(organizationId);
    }
    
    public async Task<Dictionary<ServiceType, int>> GetCountsByTypeAsync(Guid organizationId)
    {
        return await _unitOfWork.Services.GetCountsByTypeAsync(organizationId);
    }
}
