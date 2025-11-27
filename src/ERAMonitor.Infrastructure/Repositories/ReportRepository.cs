using Microsoft.EntityFrameworkCore;
using ERAMonitor.Core.DTOs.Common;
using ERAMonitor.Core.DTOs.Reports;
using ERAMonitor.Core.Entities;
using ERAMonitor.Core.Enums;
using ERAMonitor.Core.Interfaces.Repositories;
using ERAMonitor.Infrastructure.Data;
using ERAMonitor.Infrastructure.Data.Repositories;

namespace ERAMonitor.Infrastructure.Repositories;

public class ReportRepository : Repository<Report>, IReportRepository
{
    public ReportRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<PagedResponse<ReportDto>> GetPagedAsync(Guid organizationId, PagedRequest request, ReportType? type = null)
    {
        var query = _dbSet.Where(r => r.OrganizationId == organizationId);

        if (type.HasValue)
        {
            query = query.Where(r => r.Type == type.Value);
        }

        if (!string.IsNullOrEmpty(request.Search))
        {
            var search = request.Search.ToLower();
            query = query.Where(r => r.Name.ToLower().Contains(search));
        }

        // Sorting
        query = request.SortBy?.ToLower() switch
        {
            "name" => request.SortOrder == "desc" ? query.OrderByDescending(r => r.Name) : query.OrderBy(r => r.Name),
            "createdat" => request.SortOrder == "desc" ? query.OrderByDescending(r => r.CreatedAt) : query.OrderBy(r => r.CreatedAt),
            "lastrun" => request.SortOrder == "desc" ? query.OrderByDescending(r => r.LastRunAt) : query.OrderBy(r => r.LastRunAt),
            _ => query.OrderByDescending(r => r.CreatedAt)
        };

        var totalCount = await query.CountAsync();
        
        var items = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(r => new ReportDto
            {
                Id = r.Id,
                Name = r.Name,
                Type = r.Type,
                IsScheduled = r.IsScheduled,
                Schedule = r.Schedule,
                Format = r.Format,
                IsActive = r.IsActive,
                LastRunAt = r.LastRunAt,
                NextRunAt = r.NextRunAt,
                CreatedAt = r.CreatedAt
            })
            .ToListAsync();

        return new PagedResponse<ReportDto>(items, totalCount, request.Page, request.PageSize);
    }

    public async Task<ReportDetailDto?> GetDetailAsync(Guid id, Guid organizationId)
    {
        var report = await _dbSet
            .Include(r => r.Executions.OrderByDescending(e => e.StartedAt).Take(5))
            .FirstOrDefaultAsync(r => r.Id == id && r.OrganizationId == organizationId);

        if (report == null) return null;

        return new ReportDetailDto
        {
            Id = report.Id,
            Name = report.Name,
            Description = report.Description,
            Type = report.Type,
            IsScheduled = report.IsScheduled,
            Schedule = report.Schedule,
            CronExpression = report.CronExpression,
            Timezone = report.Timezone,
            NextRunAt = report.NextRunAt,
            LastRunAt = report.LastRunAt,
            CustomerId = report.CustomerId,
            CustomerName = report.Customer?.Name,
            HostIds = report.GetHostIds(),
            TimeRange = report.TimeRange,
            CustomStartDate = report.CustomStartDate,
            CustomEndDate = report.CustomEndDate,
            Format = report.Format,
            SendEmail = report.SendEmail,
            EmailRecipients = report.GetEmailRecipients(),
            SaveToStorage = report.SaveToStorage,
            LogoUrl = report.LogoUrl,
            CompanyName = report.CompanyName,
            IsActive = report.IsActive,
            RecentExecutions = report.Executions.Select(e => new ReportExecutionDto
            {
                Id = e.Id,
                StartedAt = e.StartedAt,
                CompletedAt = e.CompletedAt,
                Status = e.Status,
                Format = e.Format,
                FileUrl = e.FileUrl,
                FileSizeBytes = e.FileSizeBytes,
                ErrorMessage = e.ErrorMessage,
                EmailSent = e.EmailSent,
                IsManual = e.IsManual
            }).ToList(),
            CreatedAt = report.CreatedAt,
            UpdatedAt = report.UpdatedAt
        };
    }

    public async Task<List<ERAMonitor.Core.Entities.Report>> GetScheduledAsync()
    {
        return await _dbSet
            .Where(r => r.IsActive && r.IsScheduled && r.NextRunAt.HasValue && r.NextRunAt.Value <= DateTime.UtcNow)
            .ToListAsync();
    }
}

public class ReportExecutionRepository : Repository<ReportExecution>, IReportExecutionRepository
{
    public ReportExecutionRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<List<ReportExecutionDto>> GetByReportAsync(Guid reportId, int limit)
    {
        return await _dbSet
            .Where(e => e.ReportId == reportId)
            .OrderByDescending(e => e.StartedAt)
            .Take(limit)
            .Select(e => new ReportExecutionDto
            {
                Id = e.Id,
                StartedAt = e.StartedAt,
                CompletedAt = e.CompletedAt,
                Status = e.Status,
                Format = e.Format,
                FileUrl = e.FileUrl,
                FileSizeBytes = e.FileSizeBytes,
                ErrorMessage = e.ErrorMessage,
                EmailSent = e.EmailSent,
                IsManual = e.IsManual,
                TriggeredByUserName = e.TriggeredByUser != null ? e.TriggeredByUser.FullName : null
            })
            .ToListAsync();
    }

    public async Task<ReportExecutionDto?> GetDetailAsync(Guid id, Guid organizationId)
    {
        var execution = await _dbSet
            .Include(e => e.Report)
            .Include(e => e.TriggeredByUser)
            .FirstOrDefaultAsync(e => e.Id == id && e.Report.OrganizationId == organizationId);

        if (execution == null) return null;

        return new ReportExecutionDto
        {
            Id = execution.Id,
            StartedAt = execution.StartedAt,
            CompletedAt = execution.CompletedAt,
            Status = execution.Status,
            Format = execution.Format,
            FileUrl = execution.FileUrl,
            FileSizeBytes = execution.FileSizeBytes,
            ErrorMessage = execution.ErrorMessage,
            EmailSent = execution.EmailSent,
            IsManual = execution.IsManual,
            TriggeredByUserName = execution.TriggeredByUser?.FullName
        };
    }
}
