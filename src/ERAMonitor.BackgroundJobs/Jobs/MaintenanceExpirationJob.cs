using ERAMonitor.Core.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace ERAMonitor.BackgroundJobs.Jobs;

public class MaintenanceExpirationJob
{
    private readonly IHeartbeatService _heartbeatService;
    private readonly ILogger<MaintenanceExpirationJob> _logger;

    public MaintenanceExpirationJob(
        IHeartbeatService heartbeatService,
        ILogger<MaintenanceExpirationJob> logger)
    {
        _heartbeatService = heartbeatService;
        _logger = logger;
    }

    public async Task ExecuteAsync()
    {
        _logger.LogInformation("Starting maintenance expiration job");
        try
        {
            await _heartbeatService.ProcessMaintenanceExpirationAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing maintenance expiration job");
            throw;
        }
        _logger.LogInformation("Maintenance expiration job completed");
    }
}
