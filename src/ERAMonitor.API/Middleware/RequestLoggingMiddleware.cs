using System.Diagnostics;

namespace ERAMonitor.API.Middleware;

public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            await _next(context);
        }
        finally
        {
            stopwatch.Stop();
            
            var request = context.Request;
            var response = context.Response;
            
            _logger.LogInformation(
                "Request {Method} {Path}{Query} completed with status {StatusCode} in {Elapsed}ms",
                request.Method,
                request.Path,
                request.QueryString,
                response.StatusCode,
                stopwatch.ElapsedMilliseconds);
        }
    }
}
