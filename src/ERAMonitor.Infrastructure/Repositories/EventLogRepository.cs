using Microsoft.EntityFrameworkCore;
using ERAMonitor.Core.DTOs.Common;
using ERAMonitor.Core.DTOs.EventLog;
using ERAMonitor.Core.Entities;
using ERAMonitor.Core.Interfaces.Repositories;
using ERAMonitor.Infrastructure.Data;
using ERAMonitor.Infrastructure.Data.Repositories;

namespace ERAMonitor.Infrastructure.Repositories;

public class EventLogRepository : Repository<EventLog>, IEventLogRepository
{
    public EventLogRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<PagedResponse<EventLogListItemDto>> GetPagedByHostAsync(
        Guid hostId,
        PagedRequest request,
        string? category = null,
        string? level = null,
        DateTime? fromDate = null,
        DateTime? toDate = null)
    {
        var query = _dbSet.Where(el => el.HostId == hostId);

        // Apply filters
        if (!string.IsNullOrEmpty(category))
        {
            query = query.Where(el => el.Category == category);
        }

        if (!string.IsNullOrEmpty(level))
        {
            query = query.Where(el => el.Level == level);
        }

        if (fromDate.HasValue)
        {
            query = query.Where(el => el.TimeCreated >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            query = query.Where(el => el.TimeCreated <= toDate.Value);
        }

        if (!string.IsNullOrEmpty(request.Search))
        {
            var search = request.Search.ToLower();
            query = query.Where(el =>
                el.Source.ToLower().Contains(search) ||
                el.Message.ToLower().Contains(search));
        }

        // Default sort by TimeCreated descending (newest first)
        query = query.OrderByDescending(el => el.TimeCreated);

        var totalCount = await query.CountAsync();

        var items = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(el => new EventLogListItemDto
            {
                Id = el.Id,
                HostId = el.HostId,
                LogName = el.LogName,
                EventId = el.EventId,
                Level = el.Level,
                Source = el.Source,
                Category = el.Category,
                Message = el.Message,
                TimeCreated = el.TimeCreated,
                RecordedAt = el.RecordedAt
            })
            .ToListAsync();

        return new PagedResponse<EventLogListItemDto>(items, totalCount, request.Page, request.PageSize);
    }

    public async Task<List<string>> GetCategoriesByHostAsync(Guid hostId)
    {
        return await _dbSet
            .Where(el => el.HostId == hostId)
            .Select(el => el.Category)
            .Distinct()
            .OrderBy(c => c)
            .ToListAsync();
    }

    public async Task<int> DeleteOlderThanAsync(int retentionDays)
    {
        var threshold = DateTime.UtcNow.AddDays(-retentionDays);

        return await _dbSet
            .Where(el => el.RecordedAt < threshold)
            .ExecuteDeleteAsync();
    }
}
