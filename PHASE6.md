PHASE 6: Dashboard API & Real-time (Days 24-27)6.1 Dashboard APIEndpoints:
GET    /api/dashboard/summary              # Main dashboard stats
GET    /api/dashboard/health               # System health overview
GET    /api/dashboard/map-data             # Datacenter map data
GET    /api/dashboard/recent-incidents     # Recent incidents (last 10)
GET    /api/dashboard/charts/system-health # Chart data for system health
GET    /api/dashboard/charts/response-times # Chart data for check response times

Response for /api/dashboard/summary:
{
  "totalHosts": 1254,
  "hostsUp": 1250,
  "hostsDown": 3,
  "hostsWarning": 1,
  "totalServices": 8732,
  "servicesHealthy": 8700,
  "activeIncidents": 3,
  "uptime30d": 99.98,
  "datacenters": [
    { "id": "uuid", "name": "Turkcell DC", "status": "Operational", "hostCount": 450 },
    { "id": "uuid", "name": "Hetzner", "status": "Degraded", "hostCount": 320 }
  ]
}6.2 Customer Portal APIEndpoints (for customer users):
GET    /api/portal/dashboard               # Customer dashboard
GET    /api/portal/hosts                   # Customer's hosts
GET    /api/portal/hosts/{id}              # Host details
GET    /api/portal/websites                # Customer's website checks
GET    /api/portal/incidents               # Customer's incidents
GET    /api/portal/incidents/{id}          # Incident details
POST   /api/portal/incidents/{id}/comment  # Add comment to incident
GET    /api/portal/reports                 # Customer's reports
POST   /api/portal/reports/request         # Request custom report
GET    /api/portal/notifications           # Customer's notification history6.3 SignalR Hub for Real-time Updatescsharp// MonitoringHub.cs

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

// Usage in services:
await _hubContext.Clients.Group("dashboard").SendAsync("HostStatusChanged", new {
    hostId = host.Id,
    hostName = host.Name,
    oldStatus = previousStatus,
    newStatus = host.CurrentStatus
});

await _hubContext.Clients.Group($"customer_{customerId}").SendAsync("IncidentCreated", incident);