namespace ERAMonitor.Core.DTOs.Agent;

public class HeartbeatResponse
{
    public bool Success { get; set; }
    public string HostId { get; set; } = string.Empty;
    public int NextCheckIn { get; set; } // Seconds until next expected heartbeat
    public List<AgentCommand>? Commands { get; set; } // Future: remote commands
    public string? Message { get; set; }
}

public class AgentCommand
{
    public string CommandType { get; set; } = string.Empty;
    public string? Payload { get; set; }
    public DateTime IssuedAt { get; set; }
}
