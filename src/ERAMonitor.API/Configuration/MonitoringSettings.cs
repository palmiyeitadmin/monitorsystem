namespace ERAMonitor.API.Configuration;

public class MonitoringSettings
{
    public int DefaultCheckIntervalSeconds { get; set; } = 60;
    public int HostDownThresholdSeconds { get; set; } = 90;
    public int MetricsRetentionDays { get; set; } = 30;
    public int CheckResultsRetentionDays { get; set; } = 30;
    public int MaxMetricsPerHost { get; set; } = 10000;
}
