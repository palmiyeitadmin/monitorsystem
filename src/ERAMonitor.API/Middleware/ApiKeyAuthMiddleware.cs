using ERAMonitor.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ERAMonitor.API.Middleware;

public class ApiKeyAuthMiddleware
{
    private readonly RequestDelegate _next;
    private const string API_KEY_HEADER = "X-API-Key";

    public ApiKeyAuthMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ApplicationDbContext dbContext)
    {
        // Only apply to agent endpoints
        if (!context.Request.Path.StartsWithSegments("/api/agent"))
        {
            await _next(context);
            return;
        }

        if (!context.Request.Headers.TryGetValue(API_KEY_HEADER, out var apiKeyHeader))
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsJsonAsync(new { error = "API Key is required" });
            return;
        }

        var apiKey = apiKeyHeader.ToString();
        var host = await dbContext.Hosts.FirstOrDefaultAsync(h => h.ApiKey == apiKey);

        if (host == null)
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsJsonAsync(new { error = "Invalid API Key" });
            return;
        }

        if (!host.IsActive)
        {
            context.Response.StatusCode = 403;
            await context.Response.WriteAsJsonAsync(new { error = "Host is deactivated" });
            return;
        }

        // Store host info in HttpContext for use in controllers
        context.Items["HostId"] = host.Id;
        context.Items["Host"] = host;

        await _next(context);
    }
}
