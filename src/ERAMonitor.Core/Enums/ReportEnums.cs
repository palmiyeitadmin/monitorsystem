namespace ERAMonitor.Core.Enums;

public enum ReportType
{
    Executive,          // High-level summary
    Uptime,            // Detailed uptime/availability
    Performance,       // CPU, RAM, response times
    Incident,          // Incident analysis
    SLA,               // SLA compliance
    Security,          // SSL, vulnerabilities
    Capacity,          // Disk, resource trends
    Custom             // User-defined
}

public enum ReportSchedule
{
    Daily,
    Weekly,
    BiWeekly,
    Monthly,
    Quarterly
}

public enum ReportTimeRange
{
    Today,
    Yesterday,
    Last24Hours,
    Last7Days,
    Last30Days,
    LastMonth,
    LastQuarter,
    Custom
}

public enum ReportFormat
{
    PDF,
    Excel,
    HTML,
    JSON
}

public enum ReportExecutionStatus
{
    Pending,
    Running,
    Completed,
    Failed,
    Cancelled
}
