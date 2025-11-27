namespace ERAMonitor.Core.Enums;

public enum DashboardVisibility
{
    Private,    // Only owner can see
    Team,       // All users in organization
    Public      // Public status page (no auth required)
}

public enum WidgetType
{
    // Status widgets
    StatusOverview,
    HostStatusGrid,
    ServiceStatusList,
    CheckStatusList,
    IncidentList,
    
    // Metric widgets
    CpuChart,
    MemoryChart,
    DiskChart,
    NetworkChart,
    ResponseTimeChart,
    
    // Aggregated widgets
    UptimeGauge,
    UptimeChart,
    AvailabilityHeatmap,
    
    // Counter widgets
    HostCount,
    IncidentCount,
    CheckCount,
    AlertCount,
    
    // Table widgets
    TopHostsByCpu,
    TopHostsByMemory,
    RecentIncidents,
    RecentAlerts,
    FailingChecks,
    ExpiringCertificates,
    
    // Map widgets
    HostMap,
    
    // Custom
    Markdown,
    IFrame,
    Image
}
