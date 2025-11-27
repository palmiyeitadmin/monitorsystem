using System.ComponentModel.DataAnnotations;

namespace ERAMonitor.Core.DTOs.Hosts;

public class SetMaintenanceRequest
{
    public bool Enable { get; set; }
    
    public DateTime? StartAt { get; set; }
    
    public DateTime? EndAt { get; set; }
    
    [MaxLength(500)]
    public string? Reason { get; set; }
}
