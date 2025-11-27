namespace ERAMonitor.Core.Enums;

public enum UserRole
{
    SuperAdmin,
    Admin,
    Operator,
    Viewer,
    CustomerUser
}

public enum OsType
{
    Windows,
    Linux
}

public enum HostCategory
{
    PhysicalServer,
    VirtualMachine,
    VPS,
    DedicatedServer,
    CloudInstance,
    Website
}

public enum LocationCategory
{
    Colocation,
    CloudProvider,
    HostingProvider,
    OnPremise
}

public enum CheckType
{
    HTTP,
    TCP,
    Ping,
    DNS,
    CustomHealth
}

public enum StatusType
{
    Up,
    Down,
    Warning,
    Degraded,
    Unknown,
    Disabled
}

public enum IncidentStatus
{
    New,
    Acknowledged,
    InProgress,
    Resolved,
    Closed
}

public enum IncidentSeverity
{
    Critical,
    High,
    Medium,
    Low,
    Info
}

public enum IncidentPriority
{
    Urgent,
    High,
    Medium,
    Low
}


public enum ServiceType
{
    IIS_Site,
    IIS_AppPool,
    WindowsService,
    SystemdUnit,
    DockerContainer,
    Process
}

