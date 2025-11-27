using ERAMonitor.BackgroundJobs.Jobs;
using Hangfire;

namespace ERAMonitor.BackgroundJobs;

public static class JobRegistration
{
    public static void RegisterJobs()
    {
        // Host Down Detection - run every minute
        RecurringJob.AddOrUpdate<HostDownDetectorJob>(
            "host-down-detection",
            job => job.ExecuteAsync(),
            Cron.Minutely);
            
        // Maintenance Expiration - run every 5 minutes
        RecurringJob.AddOrUpdate<MaintenanceExpirationJob>(
            "maintenance-expiration",
            job => job.ExecuteAsync(),
            "*/5 * * * *");
            
        // Metrics Cleanup - run daily at 2 AM
        RecurringJob.AddOrUpdate<MetricsCleanupJob>(
            "metrics-cleanup",
            job => job.ExecuteAsync(),
            Cron.Daily(2));
    }
}
