namespace ERAMonitor.Core.DTOs.Hosts;

public class HostMetricsDto
{
    public Guid HostId { get; set; }
    public List<MetricDataPoint> CpuHistory { get; set; } = new();
    public List<MetricDataPoint> RamHistory { get; set; } = new();
    public List<MetricDataPoint> DiskHistory { get; set; } = new();
    public List<MetricDataPoint> NetworkHistory { get; set; } = new();
}

public class MetricDataPoint
{
    public DateTime Timestamp { get; set; }
    public decimal Value { get; set; }
}
