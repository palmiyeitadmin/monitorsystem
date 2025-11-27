namespace ERAMonitor.Core.DTOs.Hosts;

public class HostMetricsDto
{
    public Guid HostId { get; set; }
    public List<MetricDataPoint> CpuHistory { get; set; } = new();
    public List<MetricDataPoint> RamHistory { get; set; } = new();
    public List<DiskMetricDataPoint> DiskHistory { get; set; } = new();
    public List<NetworkMetricDataPoint> NetworkHistory { get; set; } = new();
}

public class MetricDataPoint
{
    public DateTime Timestamp { get; set; }
    public decimal Value { get; set; }
}

public class DiskMetricDataPoint
{
    public DateTime Timestamp { get; set; }
    public string DiskName { get; set; } = string.Empty;
    public decimal UsedPercent { get; set; }
}

public class NetworkMetricDataPoint
{
    public DateTime Timestamp { get; set; }
    public long InBytes { get; set; }
    public long OutBytes { get; set; }
}
