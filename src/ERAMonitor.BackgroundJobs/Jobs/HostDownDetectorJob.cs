using ERAMonitor.Core.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace ERAMonitor.BackgroundJobs.Jobs;

public class HostDownDetectorJob
{
    private readonly IHeartbeatService _heartbeatService;
    private readonly ILogger<HostDownDetectorJob> _logger;

    public HostDownDetectorJob(
        IHeartbeatService heartbeatService,
        ILogger<HostDownDetectorJob> logger)
    {
        _heartbeatService = heartbeatService;
        _logger = logger;
    }

    public async Task ExecuteAsync()
    {
        _logger.LogInformation("Starting host down detection job");
        try
        {
            await _heartbeatService.ProcessHostDownDetectionAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing host down detection job");
            throw;
        }
        _logger.LogInformation("Host down detection job completed");
    }
}
