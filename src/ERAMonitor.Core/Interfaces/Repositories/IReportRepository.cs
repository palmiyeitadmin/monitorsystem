using ERAMonitor.Core.DTOs.Common;
using ERAMonitor.Core.DTOs.Reports;
using ERAMonitor.Core.Entities;
using ERAMonitor.Core.Enums;

namespace ERAMonitor.Core.Interfaces.Repositories;

public interface IReportRepository : IRepository<Report>
{
    Task<PagedResponse<ReportDto>> GetPagedAsync(Guid organizationId, PagedRequest request, ReportType? type = null);
    Task<ReportDetailDto?> GetDetailAsync(Guid id, Guid organizationId);
    Task<List<ERAMonitor.Core.Entities.Report>> GetScheduledAsync();
}

public interface IReportExecutionRepository : IRepository<ReportExecution>
{
    Task<List<ReportExecutionDto>> GetByReportAsync(Guid reportId, int limit);
    Task<ReportExecutionDto?> GetDetailAsync(Guid id, Guid organizationId);
}
