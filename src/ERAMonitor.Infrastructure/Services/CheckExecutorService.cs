using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using DnsClient;
using Microsoft.Extensions.Logging;
using ERAMonitor.Core.Entities;
using ERAMonitor.Core.Enums;
using ERAMonitor.Core.Interfaces.Services;

namespace ERAMonitor.Infrastructure.Services;

public class CheckExecutorService : ICheckExecutorService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<CheckExecutorService> _logger;
    
    public CheckExecutorService(
        IHttpClientFactory httpClientFactory,
        ILogger<CheckExecutorService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }
    
    public async Task<CheckResult> ExecuteCheckAsync(Check check)
    {
        return check.CheckType switch
        {
            CheckType.HTTP => await ExecuteHttpCheckAsync(check),
            CheckType.TCP => await ExecuteTcpCheckAsync(check),
            CheckType.Ping => await ExecutePingCheckAsync(check),
            CheckType.DNS => await ExecuteDnsCheckAsync(check),
            _ => CreateErrorResult(check, "Unknown check type", "UnsupportedCheckType")
        };
    }
    
    public async Task<CheckResult> ExecuteHttpCheckAsync(Check check)
    {
        var result = new CheckResult
        {
            CheckId = check.Id,
            CheckedAt = DateTime.UtcNow
        };
        
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            var handler = new HttpClientHandler();
            X509Certificate2? certificate = null;
            
            if (check.MonitorSsl && check.Target.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                handler.ServerCertificateCustomValidationCallback = (message, cert, chain, sslPolicyErrors) =>
                {
                    if (cert != null)
                    {
                        certificate = new X509Certificate2(cert);
                    }
                    return true;
                };
            }
            
            using var client = new HttpClient(handler)
            {
                Timeout = TimeSpan.FromSeconds(check.TimeoutSeconds)
            };
            
            var request = new HttpRequestMessage(
                new HttpMethod(check.HttpMethod ?? "GET"),
                check.Target
            );
            
            if (!string.IsNullOrEmpty(check.RequestBody) && 
                (check.HttpMethod == "POST" || check.HttpMethod == "PUT" || check.HttpMethod == "PATCH"))
            {
                request.Content = new StringContent(check.RequestBody, Encoding.UTF8, "application/json");
            }
            
            var response = await client.SendAsync(request);
            
            stopwatch.Stop();
            result.ResponseTimeMs = (int)stopwatch.ElapsedMilliseconds;
            result.StatusCode = (int)response.StatusCode;
            
            var statusCodeOk = (int)response.StatusCode == check.ExpectedStatusCode;
            
            bool keywordOk = true;
            if (!string.IsNullOrEmpty(check.ExpectedKeyword))
            {
                var content = await response.Content.ReadAsStringAsync();
                // Store preview of response
                result.ResponseBodyPreview = content.Length > 1000 ? content.Substring(0, 1000) : content;
                
                var keywordFound = content.Contains(check.ExpectedKeyword, StringComparison.OrdinalIgnoreCase);
                keywordOk = check.KeywordShouldExist ? keywordFound : !keywordFound;
            }
            
            if (certificate != null)
            {
                result.SslExpiryDate = certificate.NotAfter;
                result.SslDaysRemaining = (int)(certificate.NotAfter - DateTime.UtcNow).TotalDays;
            }
            
            bool success = statusCodeOk && keywordOk;
            result.Status = success ? StatusType.Up : StatusType.Down;
            
            if (!statusCodeOk)
            {
                result.ErrorMessage = $"Unexpected status code: {response.StatusCode}";
            }
            else if (!keywordOk)
            {
                result.ErrorMessage = check.KeywordShouldExist 
                    ? $"Expected keyword '{check.ExpectedKeyword}' not found" 
                    : $"Unexpected keyword '{check.ExpectedKeyword}' found";
            }
        }
        catch (TaskCanceledException)
        {
            stopwatch.Stop();
            result.ResponseTimeMs = (int)stopwatch.ElapsedMilliseconds;
            result.Status = StatusType.Down;
            result.ErrorMessage = $"Request timed out after {check.TimeoutSeconds} seconds";
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            result.ResponseTimeMs = (int)stopwatch.ElapsedMilliseconds;
            result.Status = StatusType.Down;
            result.ErrorMessage = ex.Message;
            
            _logger.LogError(ex, "Error executing HTTP check for {Target}", check.Target);
        }
        
        return result;
    }
    
    public async Task<CheckResult> ExecuteTcpCheckAsync(Check check)
    {
        var result = new CheckResult
        {
            CheckId = check.Id,
            CheckedAt = DateTime.UtcNow
        };
        
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            var port = check.TcpPort ?? 80;
            var host = check.Target;
            
            if (host.Contains(':'))
            {
                var parts = host.Split(':');
                host = parts[0];
                if (parts.Length > 1 && int.TryParse(parts[1], out int p))
                {
                    port = p;
                }
            }
            
            using var client = new TcpClient();
            var connectTask = client.ConnectAsync(host, port);
            
            if (await Task.WhenAny(connectTask, Task.Delay(TimeSpan.FromSeconds(check.TimeoutSeconds))) == connectTask)
            {
                await connectTask;
                
                stopwatch.Stop();
                result.ResponseTimeMs = (int)stopwatch.ElapsedMilliseconds;
                result.Status = StatusType.Up;
            }
            else
            {
                stopwatch.Stop();
                result.ResponseTimeMs = (int)stopwatch.ElapsedMilliseconds;
                result.Status = StatusType.Down;
                result.ErrorMessage = $"Connection timed out after {check.TimeoutSeconds} seconds";
            }
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            result.ResponseTimeMs = (int)stopwatch.ElapsedMilliseconds;
            result.Status = StatusType.Down;
            result.ErrorMessage = ex.Message;
            
            _logger.LogError(ex, "Error executing TCP check for {Target}", check.Target);
        }
        
        return result;
    }
    
    public async Task<CheckResult> ExecutePingCheckAsync(Check check)
    {
        var result = new CheckResult
        {
            CheckId = check.Id,
            CheckedAt = DateTime.UtcNow
        };
        
        try
        {
            using var ping = new Ping();
            var reply = await ping.SendPingAsync(
                check.Target,
                (int)TimeSpan.FromSeconds(check.TimeoutSeconds).TotalMilliseconds
            );
            
            result.ResponseTimeMs = (int)reply.RoundtripTime;
            bool success = reply.Status == IPStatus.Success;
            result.Status = success ? StatusType.Up : StatusType.Down;
            
            if (!success)
            {
                result.ErrorMessage = $"Ping failed: {reply.Status}";
            }
        }
        catch (Exception ex)
        {
            result.Status = StatusType.Down;
            result.ErrorMessage = ex.Message;
            
            _logger.LogError(ex, "Error executing Ping check for {Target}", check.Target);
        }
        
        return result;
    }
    
    public async Task<CheckResult> ExecuteDnsCheckAsync(Check check)
    {
        var result = new CheckResult
        {
            CheckId = check.Id,
            CheckedAt = DateTime.UtcNow
        };
        
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            var lookup = new LookupClient();
            var queryType = QueryType.A; 
            
            var response = await lookup.QueryAsync(check.Target, queryType);
            
            stopwatch.Stop();
            result.ResponseTimeMs = (int)stopwatch.ElapsedMilliseconds;
            
            if (response.HasError)
            {
                result.Status = StatusType.Down;
                result.ErrorMessage = response.ErrorMessage;
            }
            else
            {
                var records = response.Answers.Select(a => a.ToString()).ToList();
                // Store DNS results in ResponseBodyPreview
                result.ResponseBodyPreview = string.Join("; ", records);
                
                bool success = records.Count > 0;
                result.Status = success ? StatusType.Up : StatusType.Down;
                
                if (!success)
                {
                    result.ErrorMessage = "No DNS records found";
                }
            }
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            result.ResponseTimeMs = (int)stopwatch.ElapsedMilliseconds;
            result.Status = StatusType.Down;
            result.ErrorMessage = ex.Message;
            
            _logger.LogError(ex, "Error executing DNS check for {Target}", check.Target);
        }
        
        return result;
    }
    
    private static CheckResult CreateErrorResult(Check check, string message, string errorType)
    {
        return new CheckResult
        {
            CheckId = check.Id,
            CheckedAt = DateTime.UtcNow,
            Status = StatusType.Down,
            ErrorMessage = message
        };
    }
}
