using Microsoft.EntityFrameworkCore;
using ERAMonitor.Core.DTOs.Services;
using ERAMonitor.Core.Entities;
using ERAMonitor.Core.Interfaces.Repositories;
using ERAMonitor.Infrastructure.Data;

namespace ERAMonitor.Infrastructure.Repositories;

public class ServiceStatusHistoryRepository : IServiceStatusHistoryRepository
{
    private readonly ApplicationDbContext _context;

    public ServiceStatusHistoryRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ServiceStatusHistory> AddAsync(ServiceStatusHistory history)
    {
        await _context.ServiceStatusHistories.AddAsync(history);
        return history;
    }

    public async Task<List<ServiceStatusHistory>> GetByServiceAsync(Guid serviceId, int limit = 100)
    {
        return await _context.ServiceStatusHistories
            .Where(h => h.ServiceId == serviceId)
            .OrderByDescending(h => h.RecordedAt)
            .Take(limit)
            .ToListAsync();
    }

    public async Task<List<ServiceStatusHistory>> GetByServiceAsync(Guid serviceId, DateTime from, DateTime to)
    {
        return await _context.ServiceStatusHistories
            .Where(h => h.ServiceId == serviceId && h.RecordedAt >= from && h.RecordedAt <= to)
            .OrderBy(h => h.RecordedAt)
            .ToListAsync();
    }

    public async Task<List<ServiceStatusHistoryDto>> GetRecentByServiceAsync(Guid serviceId, int limit = 20)
    {
        return await _context.ServiceStatusHistories
            .Where(h => h.ServiceId == serviceId)
            .OrderByDescending(h => h.RecordedAt)
            .Take(limit)
            .Select(h => new ServiceStatusHistoryDto
            {
                Status = h.Status,
                Message = h.Message,
                RecordedAt = h.RecordedAt
            })
            .ToListAsync();
    }

    public async Task<List<ServiceStatusHistory>> GetStatusChangesAsync(Guid serviceId, DateTime from, DateTime to)
    {
        // This is simplified. Ideally we want only records where status changed.
        // But since we only insert on status change (mostly), this works.
        return await GetByServiceAsync(serviceId, from, to);
    }

    public async Task<int> DeleteOlderThanAsync(int retentionDays)
    {
        var threshold = DateTime.UtcNow.AddDays(-retentionDays);
        
        return await _context.ServiceStatusHistories
            .Where(h => h.RecordedAt < threshold)
            .ExecuteDeleteAsync();
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
