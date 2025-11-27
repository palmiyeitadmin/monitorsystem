using ERAMonitor.Core.Interfaces.Repositories;
using Microsoft.Extensions.Logging;

namespace ERAMonitor.BackgroundJobs.Jobs;

public class MetricsCleanupJob
{
    private readonly IHostMetricRepository _metricRepository;
    private readonly IServiceStatusHistoryRepository _serviceHistoryRepository;
    private readonly ILogger<MetricsCleanupJob> _logger;

    public MetricsCleanupJob(
        IHostMetricRepository metricRepository,
        IServiceStatusHistoryRepository serviceHistoryRepository,
        ILogger<MetricsCleanupJob> logger)
    {
        _metricRepository = metricRepository;
        _serviceHistoryRepository = serviceHistoryRepository;
        _logger = logger;
    }

    public async Task ExecuteAsync()
    {
        _logger.LogInformation("Starting metrics cleanup job");
        try
        {
            // Retention policy - could be configurable
            int metricRetentionDays = 30;
            int historyRetentionDays = 90;
            
            var deletedMetrics = await _metricRepository.DeleteOlderThanAsync(metricRetentionDays);
            _logger.LogInformation("Deleted {Count} old host metrics", deletedMetrics);
            
            var deletedHistory = await _serviceHistoryRepository.DeleteOlderThanAsync(historyRetentionDays);
            _logger.LogInformation("Deleted {Count} old service status history records", deletedHistory);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing metrics cleanup job");
            throw;
        }
        _logger.LogInformation("Metrics cleanup job completed");
    }
}
