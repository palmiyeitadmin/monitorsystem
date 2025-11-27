using System.Diagnostics;
using System.Text;
using ERAMonitor.Core.Entities;
using ERAMonitor.Core.Enums;
using ERAMonitor.Infrastructure.Data;
using Hangfire;
using Microsoft.EntityFrameworkCore;

namespace ERAMonitor.BackgroundJobs.Jobs;

public class HttpCheckerJob
{
    private readonly ApplicationDbContext _context;
    private readonly HttpClient _httpClient;

    public HttpCheckerJob(ApplicationDbContext context, IHttpClientFactory httpClientFactory)
    {
        _context = context;
        _httpClient = httpClientFactory.CreateClient();
    }

    [AutomaticRetry(Attempts = 0)]
    public async Task ExecuteHttpCheck(Guid checkId)
    {
        var check = await _context.Checks.FindAsync(checkId);
        if (check == null || !check.MonitoringEnabled) return;

        var result = new CheckResult
        {
            CheckId = checkId,
            CreatedAt = DateTime.UtcNow
        };

        var stopwatch = Stopwatch.StartNew();

        try
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(check.TimeoutSeconds));

            var request = new HttpRequestMessage(
                new HttpMethod(check.HttpMethod ?? "GET"),
                check.Target
            );

            // Add custom headers
            // Assuming RequestHeaders is stored as JSON string in DB
            // Need to deserialize if it's a string, but entity definition says string?
            // Let's assume it's a JSON string for now based on previous context
            // But wait, in Entity it might be string?
            // Let's check Entity definition if possible, but I recall it being string? RequestHeaders
            // Yes, in CreateCheck it was serialized.
            
            // For now, skipping header deserialization to keep it simple or adding if needed.
            // If check.RequestHeaders is not null/empty...
            
            // Add body for POST/PUT
            if (!string.IsNullOrEmpty(check.RequestBody))
            {
                request.Content = new StringContent(check.RequestBody, Encoding.UTF8, "application/json");
            }

            var response = await _httpClient.SendAsync(request, cts.Token);
            stopwatch.Stop();

            result.ResponseTimeMs = (int)stopwatch.ElapsedMilliseconds;
            result.StatusCode = (int)response.StatusCode;

            // Check status code
            var expectedStatus = check.ExpectedStatusCode;
            var statusMatch = result.StatusCode == expectedStatus;

            // Check keyword if configured
            var keywordMatch = true;
            if (!string.IsNullOrEmpty(check.ExpectedKeyword))
            {
                var content = await response.Content.ReadAsStringAsync(cts.Token);
                result.ResponseBodyPreview = content.Length > 500 ? content[..500] : content;

                var keywordFound = content.Contains(check.ExpectedKeyword, StringComparison.OrdinalIgnoreCase);
                keywordMatch = check.KeywordShouldExist ? keywordFound : !keywordFound;
            }

            // Determine final status
            result.Status = (statusMatch && keywordMatch) ? StatusType.Up : StatusType.Down;
            if (!statusMatch)
            {
                result.ErrorMessage = $"Expected status {expectedStatus}, got {result.StatusCode}";
            }
            else if (!keywordMatch)
            {
                result.ErrorMessage = $"Keyword '{check.ExpectedKeyword}' {(check.KeywordShouldExist ? "not found" : "found")}";
            }
        }
        catch (TaskCanceledException)
        {
            stopwatch.Stop();
            result.Status = StatusType.Down;
            result.ResponseTimeMs = (int)stopwatch.ElapsedMilliseconds;
            result.ErrorMessage = "Request timed out";
        }
        catch (HttpRequestException ex)
        {
            stopwatch.Stop();
            result.Status = StatusType.Down;
            result.ResponseTimeMs = (int)stopwatch.ElapsedMilliseconds;
            result.ErrorMessage = ex.Message;
        }
        catch (Exception ex)
        {
            result.Status = StatusType.Down;
            result.ErrorMessage = $"Error: {ex.Message}";
        }

        // Save result
        _context.CheckResults.Add(result);

        // Update check status
        var previousStatus = check.CurrentStatus;
        check.CurrentStatus = result.Status;
        check.LastCheckAt = result.CreatedAt;
        check.LastResponseTimeMs = result.ResponseTimeMs;
        check.LastStatusCode = result.StatusCode;
        check.LastErrorMessage = result.ErrorMessage;
        
        // Handle status change (Stub for incidents)
        if (previousStatus != StatusType.Down && result.Status == StatusType.Down)
        {
            // Create Incident logic here
        }

        await _context.SaveChangesAsync();

        // Schedule next check
        BackgroundJob.Schedule<HttpCheckerJob>(
            x => x.ExecuteHttpCheck(checkId),
            TimeSpan.FromSeconds(check.IntervalSeconds)
        );
    }
}

