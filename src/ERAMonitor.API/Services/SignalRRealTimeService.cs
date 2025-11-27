using Microsoft.AspNetCore.SignalR;
using ERAMonitor.API.Hubs;
using ERAMonitor.Core.Interfaces.Services;

namespace ERAMonitor.API.Services;

public class SignalRRealTimeService : IRealTimeService
{
    private readonly IHubContext<MonitoringHub> _hubContext;

    public SignalRRealTimeService(IHubContext<MonitoringHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task BroadcastHostUpdateAsync(object hostUpdate, Guid? customerId = null)
    {
        await _hubContext.Clients.All.SendAsync("HostUpdated", hostUpdate);
        
        if (customerId.HasValue)
        {
            await _hubContext.Clients.Group($"customer_{customerId}").SendAsync("HostUpdated", hostUpdate);
        }
    }

    public async Task BroadcastIncidentCreatedAsync(object incident, Guid? customerId = null)
    {
        await _hubContext.Clients.Group("dashboard").SendAsync("IncidentCreated", incident);
        
        if (customerId.HasValue)
        {
            await _hubContext.Clients.Group($"customer_{customerId}").SendAsync("IncidentCreated", incident);
        }
    }

    public async Task BroadcastIncidentUpdatedAsync(object incident, Guid? customerId = null)
    {
        await _hubContext.Clients.Group("dashboard").SendAsync("IncidentUpdated", incident);
        
        if (customerId.HasValue)
        {
            await _hubContext.Clients.Group($"customer_{customerId}").SendAsync("IncidentUpdated", incident);
        }
    }
}
