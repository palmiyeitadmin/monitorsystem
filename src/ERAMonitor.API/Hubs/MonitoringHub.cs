using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace ERAMonitor.API.Hubs;

[Authorize]
public class MonitoringHub : Hub
{
    public async Task JoinHostGroup(string hostId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"host_{hostId}");
    }
    
    public async Task LeaveHostGroup(string hostId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"host_{hostId}");
    }
    
    public async Task JoinCustomerGroup(string customerId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"customer_{customerId}");
    }
    
    public async Task JoinDashboard()
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, "dashboard");
    }
}
