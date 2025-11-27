using ERAMonitor.BackgroundJobs.Jobs;
using ERAMonitor.Core.Enums;
using ERAMonitor.Infrastructure.Data;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ERAMonitor.BackgroundJobs.Services;

public class CheckSchedulerService : IHostedService
{
    private readonly IServiceProvider _serviceProvider;

    public CheckSchedulerService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var checks = await context.Checks
            .Where(c => c.MonitoringEnabled)
            .ToListAsync(cancellationToken);

        foreach (var check in checks)
        {
            var jobId = $"check_{check.Id}";

            if (check.CheckType == CheckType.HTTP)
            {
                BackgroundJob.Enqueue<HttpCheckerJob>(x => x.ExecuteHttpCheck(check.Id));
            }
        }

        // Schedule Host Down Detector
        RecurringJob.AddOrUpdate<HostDownDetectorJob>(
            "host-down-detector",
            x => x.ExecuteAsync(),
            "*/30 * * * * *" // Every 30 seconds
        );
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
