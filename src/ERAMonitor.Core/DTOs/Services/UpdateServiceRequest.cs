namespace ERAMonitor.Core.DTOs.Services;

public class UpdateServiceRequest
{
    public string? DisplayName { get; set; }
    public string? Description { get; set; }
    public bool MonitoringEnabled { get; set; } = true;
    public bool AlertOnStop { get; set; } = true;
}
