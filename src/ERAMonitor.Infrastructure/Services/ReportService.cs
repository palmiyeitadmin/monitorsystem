using ERAMonitor.Core.DTOs.Common;
using ERAMonitor.Core.DTOs.Reports;
using ERAMonitor.Core.Enums;
using ERAMonitor.Core.Interfaces;
using ERAMonitor.Core.Interfaces.Jobs;
using ERAMonitor.Core.Interfaces.Repositories;
using Hangfire;

namespace ERAMonitor.Infrastructure.Services;

public class ReportService : IReportService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IBackgroundJobClient _backgroundJobClient;

    public ReportService(IUnitOfWork unitOfWork, IBackgroundJobClient backgroundJobClient)
    {
        _unitOfWork = unitOfWork;
        _backgroundJobClient = backgroundJobClient;
    }

    public async Task<PagedResponse<ReportDto>> GetPagedAsync(Guid organizationId, PagedRequest request, ReportType? type = null)
    {
        return await _unitOfWork.Reports.GetPagedAsync(organizationId, request, type);
    }

    public async Task<ReportDetailDto?> GetDetailAsync(Guid id, Guid organizationId)
    {
        return await _unitOfWork.Reports.GetDetailAsync(id, organizationId);
    }

    public async Task<ReportDetailDto> CreateAsync(Guid organizationId, CreateReportDto dto)
    {
        var report = new ERAMonitor.Core.Entities.Report
        {
            OrganizationId = organizationId,
            Name = dto.Name,
            Description = dto.Description,
            Type = dto.Type,
            IsScheduled = dto.IsScheduled,
            Schedule = dto.Schedule,
            CronExpression = dto.CronExpression,
            Timezone = dto.Timezone ?? "Europe/Istanbul",
            TimeRange = dto.TimeRange,
            CustomStartDate = dto.CustomStartDate,
            CustomEndDate = dto.CustomEndDate,
            Format = dto.Format,
            Sections = dto.Sections, // JSON
            SendEmail = dto.SendEmail,
            EmailRecipients = dto.EmailRecipients, // JSON
            SaveToStorage = dto.SaveToStorage,
            LogoUrl = dto.LogoUrl,
            CompanyName = dto.CompanyName,
            PrimaryColor = dto.PrimaryColor,
            IsActive = dto.IsActive,
            HostIds = dto.HostIds,
            CheckIds = dto.CheckIds,
            Tags = dto.Tags
        };

        // Calculate NextRunAt if scheduled
        if (report.IsScheduled)
        {
            // Simple logic for now, real cron parsing needed for CronExpression
            // If using ReportSchedule enum:
            report.NextRunAt = CalculateNextRun(report.Schedule, report.CronExpression, report.Timezone);
        }

        await _unitOfWork.Reports.AddAsync(report);
        await _unitOfWork.SaveChangesAsync();

        return await GetDetailAsync(report.Id, organizationId) ?? throw new InvalidOperationException("Failed to retrieve created report");
    }

    public async Task<ReportDetailDto?> UpdateAsync(Guid id, Guid organizationId, UpdateReportDto dto)
    {
        var report = await _unitOfWork.Reports.GetByIdAsync(id);
        if (report == null || report.OrganizationId != organizationId) return null;

        report.Name = dto.Name;
        report.Description = dto.Description;
        report.Type = dto.Type;
        report.IsScheduled = dto.IsScheduled;
        report.Schedule = dto.Schedule;
        report.CronExpression = dto.CronExpression;
        report.Timezone = dto.Timezone ?? report.Timezone;
        report.TimeRange = dto.TimeRange;
        report.CustomStartDate = dto.CustomStartDate;
        report.CustomEndDate = dto.CustomEndDate;
        report.Format = dto.Format;
        report.Sections = dto.Sections;
        report.SendEmail = dto.SendEmail;
        report.EmailRecipients = dto.EmailRecipients;
        report.SaveToStorage = dto.SaveToStorage;
        report.LogoUrl = dto.LogoUrl;
        report.CompanyName = dto.CompanyName;
        report.PrimaryColor = dto.PrimaryColor;
        report.IsActive = dto.IsActive;
        report.HostIds = dto.HostIds;
        report.CheckIds = dto.CheckIds;
        report.Tags = dto.Tags;

        if (report.IsScheduled)
        {
             report.NextRunAt = CalculateNextRun(report.Schedule, report.CronExpression, report.Timezone);
        }
        else
        {
            report.NextRunAt = null;
        }

        _unitOfWork.Reports.Update(report);
        await _unitOfWork.SaveChangesAsync();

        return await GetDetailAsync(id, organizationId);
    }

    public async Task<bool> DeleteAsync(Guid id, Guid organizationId)
    {
        var report = await _unitOfWork.Reports.GetByIdAsync(id);
        if (report == null || report.OrganizationId != organizationId) return false;

        _unitOfWork.Reports.Remove(report);
        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    public async Task<ReportExecutionDto> GenerateAsync(Guid reportId, Guid organizationId, Guid? triggeredByUserId = null)
    {
        var report = await _unitOfWork.Reports.GetByIdAsync(reportId);
        if (report == null || report.OrganizationId != organizationId) throw new KeyNotFoundException("Report not found");

        var (from, to) = report.GetTimeRange();

        var execution = new ERAMonitor.Core.Entities.ReportExecution
        {
            ReportId = reportId,
            StartedAt = DateTime.UtcNow,
            Status = ReportExecutionStatus.Pending,
            DataFromDate = from,
            DataToDate = to,
            Format = report.Format,
            IsManual = triggeredByUserId.HasValue,
            TriggeredByUserId = triggeredByUserId
        };

        await _unitOfWork.ReportExecutions.AddAsync(execution);
        await _unitOfWork.SaveChangesAsync();

        // Enqueue Job
        _backgroundJobClient.Enqueue<IReportGeneratorJob>(x => x.GenerateReport(execution.Id));

        return new ReportExecutionDto
        {
            Id = execution.Id,
            ReportId = execution.ReportId,
            StartedAt = execution.StartedAt,
            Status = execution.Status,
            DataFromDate = execution.DataFromDate,
            DataToDate = execution.DataToDate,
            Format = execution.Format,
            IsManual = execution.IsManual,
            TriggeredByUserId = execution.TriggeredByUserId
        };
    }

    public async Task<PagedResponse<ReportExecutionDto>> GetExecutionsAsync(Guid reportId, Guid organizationId, PagedRequest request)
    {
        // This should be in Repository, but implementing here for now or assuming repository has it?
        // IReportExecutionRepository inherits IRepository<ReportExecution>.
        // I need a custom method in repository for paging.
        // Or I can use FindAsync but paging is manual.
        // For now, I'll throw NotImplemented or implement basic Find.
        // Actually, IUnitOfWork.ReportExecutions is generic IRepository?
        // No, it's IReportExecutionRepository.
        // Let's check IReportExecutionRepository in IReportRepository.cs.
        // It inherits IRepository<ReportExecution>.
        // It doesn't have GetPagedAsync.
        // So I should implement it in Repository or just do simple query here if I can access Query().
        
        var query = _unitOfWork.ReportExecutions.Query()
            .Where(x => x.ReportId == reportId && x.Report.OrganizationId == organizationId)
            .OrderByDescending(x => x.StartedAt);

        var total = query.Count();
        var items = query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(x => new ReportExecutionDto
            {
                Id = x.Id,
                ReportId = x.ReportId,
                StartedAt = x.StartedAt,
                CompletedAt = x.CompletedAt,
                Status = x.Status,
                DataFromDate = x.DataFromDate,
                DataToDate = x.DataToDate,
                Format = x.Format,
                FileUrl = x.FileUrl,
                ErrorMessage = x.ErrorMessage,
                IsManual = x.IsManual,
                TriggeredByUserId = x.TriggeredByUserId
            })
            .ToList();

        return await Task.FromResult(new PagedResponse<ReportExecutionDto>(items, total, request.Page, request.PageSize));
    }

    public async Task<byte[]> DownloadExecutionAsync(Guid executionId, Guid organizationId)
    {
        var execution = await _unitOfWork.ReportExecutions.GetByIdAsync(executionId, x => x.Report);
        if (execution == null || execution.Report.OrganizationId != organizationId) throw new KeyNotFoundException("Execution not found");

        if (string.IsNullOrEmpty(execution.FileUrl)) return Array.Empty<byte>();

        // If it's a URL, we might redirect in Controller.
        // But if Controller calls this, it expects bytes.
        // For now, return empty.
        return Array.Empty<byte>();
    }

    private DateTime? CalculateNextRun(ReportSchedule? schedule, string? cron, string timezoneId)
    {
        if (!schedule.HasValue) return null;
        
        var now = DateTime.UtcNow;
        // Basic logic
        return schedule switch
        {
            ReportSchedule.Daily => now.AddDays(1).Date, // Midnight next day
            ReportSchedule.Weekly => now.AddDays(7).Date,
            ReportSchedule.Monthly => now.AddMonths(1).Date,
            _ => now.AddDays(1)
        };
    }
}
