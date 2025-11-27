using System.Text;
using System.Text.Json;
using ERAMonitor.Core.DTOs;
using ERAMonitor.Core.Entities;
using ERAMonitor.Core.Enums;
using ERAMonitor.Core.Interfaces;
using ERAMonitor.Core.Interfaces.Jobs;
using ERAMonitor.Core.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ERAMonitor.BackgroundJobs.Jobs;

public class ReportGeneratorJob : IReportGeneratorJob
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ReportGeneratorJob> _logger;

    public ReportGeneratorJob(IUnitOfWork unitOfWork, ILogger<ReportGeneratorJob> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task GenerateReport(Guid executionId)
    {
        _logger.LogInformation("Starting report generation for execution {ExecutionId}", executionId);

        var execution = await _unitOfWork.ReportExecutions.GetByIdAsync(executionId, e => e.Report);
        if (execution == null)
        {
            _logger.LogError("Report execution {ExecutionId} not found", executionId);
            return;
        }

        try
        {
            execution.Status = ReportExecutionStatus.Running;
            _unitOfWork.ReportExecutions.Update(execution);
            await _unitOfWork.SaveChangesAsync();

            // Simulate report generation
            await Task.Delay(2000); // Simulate work

            // Generate dummy content
            var content = $"Report: {execution.Report.Name}\nGenerated At: {DateTime.UtcNow}\nData Range: {execution.DataFromDate} - {execution.DataToDate}";
            var fileName = $"report_{execution.ReportId}_{execution.Id}.txt";
            var filePath = Path.Combine(Path.GetTempPath(), fileName);
            
            await File.WriteAllTextAsync(filePath, content);

            execution.Status = ReportExecutionStatus.Completed;
            execution.CompletedAt = DateTime.UtcNow;
            execution.FilePath = filePath;
            execution.FileUrl = $"file://{filePath}"; // Mock URL
            execution.FileSizeBytes = new FileInfo(filePath).Length;

            _unitOfWork.ReportExecutions.Update(execution);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Report generation completed for execution {ExecutionId}", executionId);
            
            // TODO: Send email if configured
            if (execution.Report.SendEmail)
            {
                // Send email logic here
                execution.EmailSent = true;
                execution.EmailSentAt = DateTime.UtcNow;
                _unitOfWork.ReportExecutions.Update(execution);
                await _unitOfWork.SaveChangesAsync();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating report for execution {ExecutionId}", executionId);
            
            execution.Status = ReportExecutionStatus.Failed;
            execution.ErrorMessage = ex.Message;
            execution.CompletedAt = DateTime.UtcNow;
            
            _unitOfWork.ReportExecutions.Update(execution);
            await _unitOfWork.SaveChangesAsync();
        }
    }
}
