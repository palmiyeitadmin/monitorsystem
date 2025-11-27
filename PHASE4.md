PHASE 4: Checks & Monitoring Engine (Days 13-17)4.1 Check Management APIEndpoints:
GET    /api/checks                         # List with filters
GET    /api/checks/{id}                    # Get details
POST   /api/checks                         # Create check
PUT    /api/checks/{id}                    # Update
DELETE /api/checks/{id}                    # Delete
POST   /api/checks/{id}/run                # Run check immediately
GET    /api/checks/{id}/results            # Get results history
PUT    /api/checks/{id}/toggle             # Enable/Disable

Filters:
- checkType (HTTP, TCP, Ping, DNS)
- customerId
- hostId
- status
- search4.2 HTTP Checker Job (Hangfire)csharp// HttpCheckerJob.cs

public class HttpCheckerJob
{
    private readonly ICheckRepository _checkRepository;
    private readonly ICheckResultRepository _resultRepository;
    private readonly IIncidentService _incidentService;
    private readonly INotificationService _notificationService;
    private readonly HttpClient _httpClient;
    
    [AutomaticRetry(Attempts = 0)]
    public async Task ExecuteHttpCheck(Guid checkId)
    {
        var check = await _checkRepository.GetByIdAsync(checkId);
        if (check == null || !check.MonitoringEnabled) return;
        
        var result = new CheckResult
        {
            CheckId = checkId,
            CheckedAt = DateTime.UtcNow
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
            if (check.RequestHeaders != null)
            {
                foreach (var header in check.RequestHeaders)
                {
                    request.Headers.TryAddWithoutValidation(header.Key, header.Value);
                }
            }
            
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
            var expectedStatus = check.ExpectedStatusCode ?? 200;
            var statusMatch = result.StatusCode == expectedStatus;
            
            // Check keyword if configured
            var keywordMatch = true;
            if (!string.IsNullOrEmpty(check.ExpectedKeyword))
            {
                var content = await response.Content.ReadAsStringAsync();
                result.ResponseBodyPreview = content.Length > 500 ? content[..500] : content;
                
                var keywordFound = content.Contains(check.ExpectedKeyword, StringComparison.OrdinalIgnoreCase);
                keywordMatch = check.KeywordShouldExist ? keywordFound : !keywordFound;
            }
            
            // Check SSL certificate
            if (check.MonitorSsl && check.Target.StartsWith("https://"))
            {
                var sslInfo = await GetSslCertificateInfo(check.Target);
                result.SslExpiryDate = sslInfo.ExpiryDate;
                result.SslDaysRemaining = sslInfo.DaysRemaining;
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
        await _resultRepository.InsertAsync(result);
        
        // Update check status
        var previousStatus = check.CurrentStatus;
        check.CurrentStatus = result.Status;
        check.LastCheckAt = result.CheckedAt;
        check.LastResponseTimeMs = result.ResponseTimeMs;
        check.LastStatusCode = result.StatusCode;
        check.LastErrorMessage = result.ErrorMessage;
        check.SslExpiryDate = result.SslExpiryDate;
        check.SslDaysRemaining = result.SslDaysRemaining;
        
        await _checkRepository.UpdateAsync(check);
        
        // Handle status change
        if (previousStatus != StatusType.Down && result.Status == StatusType.Down)
        {
            await _incidentService.CreateAutoIncident(check, $"Check DOWN: {check.Name}", IncidentSeverity.High);
            await _notificationService.SendCheckDownNotification(check, result);
        }
        else if (previousStatus == StatusType.Down && result.Status == StatusType.Up)
        {
            await _incidentService.AutoResolveIncident(check);
            await _notificationService.SendCheckUpNotification(check, result);
        }
        
        // Check SSL expiry warning
        if (result.SslDaysRemaining.HasValue && result.SslDaysRemaining <= check.SslExpiryWarningDays)
        {
            await _notificationService.SendSslExpiryWarning(check, result.SslDaysRemaining.Value);
        }
        
        // Schedule next check
        BackgroundJob.Schedule<HttpCheckerJob>(
            x => x.ExecuteHttpCheck(checkId),
            TimeSpan.FromSeconds(check.IntervalSeconds)
        );
    }
}4.3 TCP Checker Jobcsharp// TcpCheckerJob.cs

public class TcpCheckerJob
{
    [AutomaticRetry(Attempts = 0)]
    public async Task ExecuteTcpCheck(Guid checkId)
    {
        var check = await _checkRepository.GetByIdAsync(checkId);
        if (check == null || !check.MonitoringEnabled) return;
        
        var result = new CheckResult
        {
            CheckId = checkId,
            CheckedAt = DateTime.UtcNow
        };
        
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            // Parse host:port from target
            var uri = new Uri($"tcp://{check.Target}");
            var host = uri.Host;
            var port = check.TcpPort ?? uri.Port;
            
            using var client = new TcpClient();
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(check.TimeoutSeconds));
            
            await client.ConnectAsync(host, port, cts.Token);
            
            stopwatch.Stop();
            
            if (client.Connected)
            {
                result.Status = StatusType.Up;
                result.ResponseTimeMs = (int)stopwatch.ElapsedMilliseconds;
                
                // Optional: Send test data and check response
                if (!string.IsNullOrEmpty(check.SendData))
                {
                    var stream = client.GetStream();
                    var data = Encoding.UTF8.GetBytes(check.SendData);
                    await stream.WriteAsync(data, cts.Token);
                    
                    var buffer = new byte[1024];
                    var bytesRead = await stream.ReadAsync(buffer, cts.Token);
                    result.ResponseBodyPreview = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                }
            }
            else
            {
                result.Status = StatusType.Down;
                result.ErrorMessage = "Connection refused";
            }
        }
        catch (OperationCanceledException)
        {
            result.Status = StatusType.Down;
            result.ErrorMessage = "Connection timed out";
        }
        catch (SocketException ex)
        {
            result.Status = StatusType.Down;
            result.ErrorMessage = ex.Message;
        }
        
        // Save and update (similar to HTTP checker)
        // ...
    }
}4.4 Check Schedulercsharp// CheckSchedulerService.cs

public class CheckSchedulerService : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        // On startup, schedule all active checks
        var checks = await _checkRepository.GetAllActiveAsync();
        
        foreach (var check in checks)
        {
            ScheduleCheck(check);
        }
    }
    
    private void ScheduleCheck(Check check)
    {
        var jobId = $"check_{check.Id}";
        
        switch (check.CheckType)
        {
            case CheckType.HTTP:
                RecurringJob.AddOrUpdate<HttpCheckerJob>(
                    jobId,
                    x => x.ExecuteHttpCheck(check.Id),
                    $"*/{check.IntervalSeconds} * * * * *" // CRON for seconds
                );
                break;
                
            case CheckType.TCP:
                RecurringJob.AddOrUpdate<TcpCheckerJob>(
                    jobId,
                    x => x.ExecuteTcpCheck(check.Id),
                    $"*/{check.IntervalSeconds} * * * * *"
                );
                break;
                
            // ... other check types
        }
    }
}4.5 Host Down Detection Jobcsharp// HostDownDetectorJob.cs

// Runs every 30 seconds to detect hosts that haven't sent heartbeat

public class HostDownDetectorJob
{
    [AutomaticRetry(Attempts = 0)]
    public async Task Execute()
    {
        var threshold = DateTime.UtcNow.AddSeconds(-90); // 90 seconds threshold
        
        var hosts = await _hostRepository.GetHostsNotSeenSince(threshold);
        
        foreach (var host in hosts)
        {
            if (host.CurrentStatus != StatusType.Down)
            {
                host.CurrentStatus = StatusType.Down;
                await _hostRepository.UpdateAsync(host);
                
                await _incidentService.CreateAutoIncident(
                    host, 
                    $"Host DOWN: {host.Name} - No heartbeat received",
                    IncidentSeverity.Critical
                );
                
                await _notificationService.SendHostDownNotification(host);
            }
        }
    }
}

// Register in Hangfire
RecurringJob.AddOrUpdate<HostDownDetectorJob>(
    "host-down-detector",
    x => x.Execute(),
    "*/30 * * * * *" // Every 30 seconds
);