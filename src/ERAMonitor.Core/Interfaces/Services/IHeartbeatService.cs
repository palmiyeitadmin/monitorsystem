using ERAMonitor.Core.DTOs.Agent;
using ERAMonitor.Core.Entities;

namespace ERAMonitor.Core.Interfaces.Services;

public interface IHeartbeatService
{
    Task<HeartbeatResponse> ProcessHeartbeatAsync(string apiKey, HeartbeatRequest request);
    
    // Called by background job to detect down hosts
    Task ProcessHostDownDetectionAsync();
    
    // Process maintenance mode expiration
    Task ProcessMaintenanceExpirationAsync();
}
