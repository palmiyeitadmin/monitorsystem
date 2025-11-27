PHASE 7: Reports (Days 28-30)7.1 Report Generation APIEndpoints:
GET    /api/reports                        # List reports
GET    /api/reports/{id}                   # Get report details
POST   /api/reports/generate               # Generate new report
GET    /api/reports/{id}/download          # Download report file
DELETE /api/reports/{id}                   # Delete report

POST   /api/reports/scheduled              # Create scheduled report
GET    /api/reports/scheduled              # List scheduled reports
PUT    /api/reports/scheduled/{id}         # Update scheduled report
DELETE /api/reports/scheduled/{id}         # Delete scheduled report7.2 Report Generation Servicecsharp// ReportGenerationService.cs

public class ReportGenerationService : IReportGenerationService
{
    public async Task<Report> GenerateUptimeReport(UptimeReportRequest request)
    {
        var report = new Report
        {
            OrganizationId = request.OrganizationId,
            CustomerId = request.CustomerId,
            Name = $"Uptime Report - {request.DateFrom:MMM d} to {request.DateTo:MMM d, yyyy}",
            ReportType = "Uptime",
            Parameters = JsonSerializer.Serialize(request),
            Status = "Generating"
        };
        
        await _reportRepository.InsertAsync(report);
        
        // Background job for generation
        BackgroundJob.Enqueue<ReportGeneratorJob>(
            x => x.GenerateUptimeReport(report.Id)
        );
        
        return report;
    }
}

// ReportGeneratorJob.cs
public class ReportGeneratorJob
{
    public async Task GenerateUptimeReport(Guid reportId)
    {
        var report = await _reportRepository.GetByIdAsync(reportId);
        var request = JsonSerializer.Deserialize<UptimeReportRequest>(report.Parameters);
        
        try
        {
            // Gather data
            var hosts = await _hostRepository.GetByIdsAsync(request.HostIds);
            var checks = await _checkRepository.GetByIdsAsync(request.CheckIds);
            
            // Calculate uptime for each
            var uptimeData = new List<UptimeReportItem>();
            
            foreach (var host in hosts)
            {
                var metrics = await _metricsRepository.GetForDateRange(
                    host.Id, request.DateFrom, request.DateTo
                );
                
                var uptime = CalculateUptimePercentage(metrics);
                
                uptimeData.Add(new UptimeReportItem
                {
                    ResourceType = "Host",
                    ResourceName = host.Name,
                    UptimePercent = uptime,
                    DowntimeMinutes = CalculateDowntimeMinutes(metrics),
                    IncidentCount = await _incidentRepository.CountForResource("Host", host.Id, request.DateFrom, request.DateTo)
                });
            }
            
            // Generate PDF using QuestPDF or similar
            var pdfBytes = GeneratePdf(uptimeData, request);
            
            // Save to storage
            var fileName = $"uptime-report-{report.Id}.pdf";
            var fileUrl = await _storageService.UploadAsync(fileName, pdfBytes, "application/pdf");
            
            report.FileUrl = fileUrl;
            report.FileFormat = "PDF";
            report.FileSizeBytes = pdfBytes.Length;
            report.Status = "Ready";
            report.GeneratedAt = DateTime.UtcNow;
            report.ExpiresAt = DateTime.UtcNow.AddDays(30);
            
            await _reportRepository.UpdateAsync(report);
        }
        catch (Exception ex)
        {
            report.Status = "Failed";
            report.ErrorMessage = ex.Message;
            await _reportRepository.UpdateAsync(report);
        }
    }
}