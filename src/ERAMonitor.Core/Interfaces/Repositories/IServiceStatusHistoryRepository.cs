using ERAMonitor.Core.Entities;
using ERAMonitor.Core.DTOs.Services;

namespace ERAMonitor.Core.Interfaces.Repositories;

public interface IServiceStatusHistoryRepository
{
    Task<ServiceStatusHistory> AddAsync(ServiceStatusHistory history);
    
    Task<List<ServiceStatusHistory>> GetByServiceAsync(Guid serviceId, int limit = 100);
    Task<List<ServiceStatusHistory>> GetByServiceAsync(Guid serviceId, DateTime from, DateTime to);
    
    Task<List<ServiceStatusHistoryDto>> GetRecentByServiceAsync(Guid serviceId, int limit = 20);
    
    // For uptime calculation
    Task<List<ServiceStatusHistory>> GetStatusChangesAsync(Guid serviceId, DateTime from, DateTime to);
    
    Task<int> DeleteOlderThanAsync(int retentionDays);
    
    Task SaveChangesAsync();
}
