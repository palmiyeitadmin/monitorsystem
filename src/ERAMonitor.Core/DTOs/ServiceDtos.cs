using ERAMonitor.Core.Enums;

namespace ERAMonitor.Core.DTOs;



public class UpdateServiceRequest
{
    public bool MonitoringEnabled { get; set; }
    public string? DisplayName { get; set; }
    public string? Description { get; set; }
}
