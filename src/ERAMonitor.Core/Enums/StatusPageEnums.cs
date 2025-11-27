namespace ERAMonitor.Core.Enums;

public enum StatusPageComponentType
{
    Host,
    Check,
    Service,
    Manual // Manually controlled component
}

public enum StatusPageComponentStatus
{
    Operational,
    DegradedPerformance,
    PartialOutage,
    MajorOutage,
    UnderMaintenance
}
