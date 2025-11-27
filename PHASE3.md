PHASE 3: Host & Service Management (Days 8-12)3.1 Host Management APIEndpoints:
GET    /api/hosts                          # List with filters
GET    /api/hosts/{id}                     # Get details
POST   /api/hosts                          # Create host, generate API key
PUT    /api/hosts/{id}                     # Update
DELETE /api/hosts/{id}                     # Delete
POST   /api/hosts/{id}/regenerate-key      # Generate new API key
PUT    /api/hosts/{id}/toggle-monitoring   # Enable/Disable
GET    /api/hosts/{id}/metrics             # Get metrics history
GET    /api/hosts/{id}/services            # Get services
GET    /api/hosts/{id}/checks              # Get associated checks
GET    /api/hosts/{id}/incidents           # Get incidents history

Filters:
- status (Up, Down, Warning, Unknown)
- customerId
- locationId
- osType (Windows, Linux)
- category
- tags
- search (name, hostname)
- page, pageSize
- sortBy, sortOrder3.2 Agent Heartbeat APIThis is the critical endpoint that agents will call:POST /api/agent/heartbeat
Headers:
  X-API-Key: {host_api_key}
  Content-Type: application/json

Request Body:
{
  "timestamp": "2024-11-25T14:30:00Z",
  "system": {
    "hostname": "PROD-WEB-01",
    "osType": "Windows",
    "osVersion": "Windows Server 2022",
    "cpuPercent": 45.2,
    "ramPercent": 72.8,
    "ramUsedMB": 12048,
    "ramTotalMB": 16384,
    "uptimeSeconds": 864000
  },
  "disks": [
    { "name": "C:", "totalGB": 256, "usedGB": 180, "usedPercent": 70.3 },
    { "name": "D:", "totalGB": 500, "usedGB": 120, "usedPercent": 24.0 }
  ],
  "services": [
    { 
      "name": "W3SVC", 
      "displayName": "World Wide Web Publishing Service",
      "type": "WindowsService", 
      "status": "Running" 
    },
    { 
      "name": "Default Web Site", 
      "type": "IIS_Site", 
      "status": "Started",
      "config": {
        "bindings": ["http://*:80", "https://*:443"],
        "physicalPath": "C:\\inetpub\\wwwroot"
      }
    },
    { 
      "name": "DefaultAppPool", 
      "type": "IIS_AppPool", 
      "status": "Started",
      "config": {
        "managedRuntimeVersion": "v4.0",
        "startMode": "OnDemand"
      }
    }
  ],
  "network": {
    "primaryIP": "192.168.1.50",
    "publicIP": "85.123.45.67"
  },
  "agentVersion": "1.0.0"
}

Response:
{
  "success": true,
  "hostId": "uuid",
  "nextCheckIn": 60,
  "commands": [] // Future: remote commands
}3.3 Heartbeat Processing Logiccsharp// HeartbeatService.cs

public async Task<HeartbeatResponse> ProcessHeartbeat(string apiKey, HeartbeatRequest request)
{
    // 1. Validate API key and get host
    var host = await _hostRepository.GetByApiKeyAsync(apiKey);
    if (host == null) throw new UnauthorizedException("Invalid API key");
    
    // 2. Determine host status
    var previousStatus = host.CurrentStatus;
    var newStatus = DetermineHostStatus(request);
    
    // 3. Update host record
    host.LastSeenAt = DateTime.UtcNow;
    host.CurrentStatus = newStatus;
    host.CpuPercent = request.System.CpuPercent;
    host.RamPercent = request.System.RamPercent;
    host.RamUsedMb = request.System.RamUsedMB;
    host.RamTotalMb = request.System.RamTotalMB;
    host.UptimeSeconds = request.System.UptimeSeconds;
    host.AgentVersion = request.AgentVersion;
    host.LastHeartbeat = JsonSerializer.Serialize(request);
    
    // 4. Update/Insert disks
    await UpdateHostDisks(host.Id, request.Disks);
    
    // 5. Update/Insert services
    await UpdateHostServices(host.Id, request.Services);
    
    // 6. Store metrics (time series)
    await _metricsRepository.InsertAsync(new HostMetric
    {
        HostId = host.Id,
        CpuPercent = request.System.CpuPercent,
        RamPercent = request.System.RamPercent,
        RamUsedMb = request.System.RamUsedMB,
        RamTotalMb = request.System.RamTotalMB,
        DiskInfo = JsonSerializer.Serialize(request.Disks),
        UptimeSeconds = request.System.UptimeSeconds,
        RecordedAt = DateTime.UtcNow
    });
    
    // 7. Check for status change and create incident if needed
    if (previousStatus != StatusType.Down && newStatus == StatusType.Down)
    {
        await _incidentService.CreateAutoIncident(host, "Host DOWN", IncidentSeverity.Critical);
    }
    
    // 8. Check for service status changes
    await CheckServiceStatusChanges(host.Id, request.Services);
    
    // 9. Broadcast update via SignalR
    await _hubContext.Clients.Group($"host_{host.Id}").SendAsync("HostUpdated", host);
    
    await _unitOfWork.SaveChangesAsync();
    
    return new HeartbeatResponse
    {
        Success = true,
        HostId = host.Id,
        NextCheckIn = host.CheckIntervalSeconds
    };
}3.4 Service Management APIEndpoints:
GET    /api/services                       # List all services
GET    /api/services/{id}                  # Get service details
GET    /api/services/{id}/history          # Get status history
PUT    /api/services/{id}/toggle-monitoring

Filters:
- hostId
- customerId
- serviceType
- status
- search