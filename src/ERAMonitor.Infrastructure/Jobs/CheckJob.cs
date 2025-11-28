using ERAMonitor.Core.Entities;
using ERAMonitor.Core.Enums;
using ERAMonitor.Core.Interfaces.Services;
using ERAMonitor.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ERAMonitor.Infrastructure.Jobs;

public class CheckJob
{
    private readonly ApplicationDbContext _context;
    private readonly ICheckExecutorService _checkExecutor;
    private readonly ILogger<CheckJob> _logger;

    public CheckJob(
        ApplicationDbContext context,
        ICheckExecutorService checkExecutor,
        ILogger<CheckJob> logger)
    {
        _context = context;
        _checkExecutor = checkExecutor;
        _logger = logger;
    }

    public async Task ExecuteAsync()
    {
        _logger.LogInformation("Starting CheckJob execution");

        try
        {
            // Fetch checks that are due for execution
            // Logic: MonitoringEnabled is true AND (LastCheckAt is null OR LastCheckAt + Interval < Now)
            var now = DateTime.UtcNow;
            var dueChecks = await _context.Checks
                .Where(c => c.MonitoringEnabled && c.IsActive)
                .ToListAsync();

            // Filter in memory for now to avoid complex EF translation issues with date math if any
            // Ideally should be done in DB
            var checksToRun = dueChecks.Where(c => 
                c.LastCheckAt == null || 
                c.LastCheckAt.Value.AddSeconds(c.IntervalSeconds) <= now)
                .ToList();

            _logger.LogInformation("Found {Count} checks to run", checksToRun.Count);

            foreach (var check in checksToRun)
            {
                try
                {
                    await RunCheckAsync(check);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error running check {CheckId}", check.Id);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in CheckJob");
        }
    }

    private async Task RunCheckAsync(Check check)
    {
        var result = await _checkExecutor.ExecuteCheckAsync(check);

        // Update Check status
        check.LastCheckAt = result.CheckedAt;
        check.LastResponseTimeMs = result.ResponseTimeMs;
        check.LastStatusCode = result.StatusCode;
        check.LastErrorMessage = result.ErrorMessage;
        check.CurrentStatus = result.Status;
        
        // Update SSL info if available
        if (result.SslExpiryDate.HasValue)
        {
            check.SslExpiryDate = result.SslExpiryDate;
            check.SslDaysRemaining = result.SslDaysRemaining;
        }

        // Save Result
        _context.CheckResults.Add(result);
        
        // Save changes (Check update + Result insert)
        await _context.SaveChangesAsync();
    }
}
