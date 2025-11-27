using ERAMonitor.Core.Enums;

namespace ERAMonitor.Core.Entities;

public class Check : BaseEntity
{
    public Guid OrganizationId { get; set; }
    public Organization Organization { get; set; } = null!;
    
    public Guid? HostId { get; set; }
    public Host? Host { get; set; }
    
    public Guid? CustomerId { get; set; }
    public Customer? Customer { get; set; }
    
    public Guid? LocationId { get; set; }
    public Location? Location { get; set; }
    
    public string Name { get; set; } = string.Empty;
    public CheckType CheckType { get; set; }
    
    public string Target { get; set; } = string.Empty;
    
    // HTTP
    public string HttpMethod { get; set; } = "GET";
    public int ExpectedStatusCode { get; set; } = 200;
    public string? ExpectedKeyword { get; set; }
    public bool KeywordShouldExist { get; set; } = true;
    public string? RequestHeaders { get; set; } // JSONB
    public string? RequestBody { get; set; }
    public bool FollowRedirects { get; set; } = true;
    
    // TCP
    public int? TcpPort { get; set; }
    public string? SendData { get; set; }
    
    // SSL
    public bool MonitorSsl { get; set; } = true;
    public int SslExpiryWarningDays { get; set; } = 14;
    
    // Timing
    public int TimeoutSeconds { get; set; } = 30;
    public int IntervalSeconds { get; set; } = 60;
    
    // Status
    public StatusType CurrentStatus { get; set; } = StatusType.Unknown;
    public DateTime? LastCheckAt { get; set; }
    public int? LastResponseTimeMs { get; set; }
    public int? LastStatusCode { get; set; }
    public string? LastErrorMessage { get; set; }
    public DateTime? SslExpiryDate { get; set; }
    public int? SslDaysRemaining { get; set; }
    
    public bool MonitoringEnabled { get; set; } = true;
    public bool IsActive { get; set; } = true;
}
