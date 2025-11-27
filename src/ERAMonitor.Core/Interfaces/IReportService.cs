using ERAMonitor.Core.DTOs.Common;
using ERAMonitor.Core.DTOs.Reports;
using ERAMonitor.Core.Enums;

namespace ERAMonitor.Core.Interfaces;

public interface IReportService
{
    Task<PagedResponse<ReportDto>> GetPagedAsync(Guid organizationId, PagedRequest request, ReportType? type = null);
    Task<ReportDetailDto?> GetDetailAsync(Guid id, Guid organizationId);
    Task<ReportDetailDto> CreateAsync(Guid organizationId, CreateReportDto dto);
    Task<ReportDetailDto?> UpdateAsync(Guid id, Guid organizationId, UpdateReportDto dto);
    Task<bool> DeleteAsync(Guid id, Guid organizationId);
    
    // Execution
    Task<ReportExecutionDto> GenerateAsync(Guid reportId, Guid organizationId, Guid? triggeredByUserId = null);
    Task<PagedResponse<ReportExecutionDto>> GetExecutionsAsync(Guid reportId, Guid organizationId, PagedRequest request);
    Task<byte[]> DownloadExecutionAsync(Guid executionId, Guid organizationId);
}
