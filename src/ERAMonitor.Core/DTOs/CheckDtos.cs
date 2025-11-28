using System.ComponentModel.DataAnnotations;
using ERAMonitor.Core.Enums;

namespace ERAMonitor.Core.DTOs;

public class CheckDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string CheckType { get; set; } = string.Empty;
    public string Target { get; set; } = string.Empty;
    public int IntervalSeconds { get; set; }
    public int TimeoutSeconds { get; set; }
    public bool MonitoringEnabled { get; set; }
    public string CurrentStatus { get; set; } = string.Empty;
    public DateTime? LastCheckAt { get; set; }
    public int? LastResponseTimeMs { get; set; }
    public int? LastStatusCode { get; set; }
    public string? LastErrorMessage { get; set; }
    public bool MonitorSsl { get; set; }
    public int? SslDaysRemaining { get; set; }
    public Guid? HostId { get; set; }
    public Guid? CustomerId { get; set; }
    public List<CheckResultDto> History { get; set; } = new();
}

public class CreateCheckRequest
{
    [Required]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    public CheckType CheckType { get; set; }
    
    [Required]
    public string Target { get; set; } = string.Empty;
    
    public int IntervalSeconds { get; set; } = 60;
    public int TimeoutSeconds { get; set; } = 30;
    public bool MonitoringEnabled { get; set; } = true;
    
    // HTTP Specific
    public string? HttpMethod { get; set; }
    public string? RequestBody { get; set; }
    public Dictionary<string, string>? RequestHeaders { get; set; }
    public int? ExpectedStatusCode { get; set; }
    public string? ExpectedKeyword { get; set; }
    public bool KeywordShouldExist { get; set; } = true;
    public bool MonitorSsl { get; set; }
    public int SslExpiryWarningDays { get; set; } = 30;
    
    // TCP Specific
    public int? TcpPort { get; set; }
    
    public Guid? HostId { get; set; }
    public Guid? CustomerId { get; set; }
}

public class UpdateCheckRequest
{
    public string Name { get; set; } = string.Empty;
    public string Target { get; set; } = string.Empty;
    public int IntervalSeconds { get; set; }
    public int TimeoutSeconds { get; set; }
    public bool MonitoringEnabled { get; set; }
    
    // HTTP Specific
    public string? HttpMethod { get; set; }
    public string? RequestBody { get; set; }
    public Dictionary<string, string>? RequestHeaders { get; set; }
    public int? ExpectedStatusCode { get; set; }
    public string? ExpectedKeyword { get; set; }
    public bool KeywordShouldExist { get; set; }
    public bool MonitorSsl { get; set; }
    public int SslExpiryWarningDays { get; set; }
    
    // TCP Specific
    public int? TcpPort { get; set; }
    
    public Guid? HostId { get; set; }
    public Guid? CustomerId { get; set; }
}

public class CheckResultDto
{
    public Guid Id { get; set; }
    public string Status { get; set; } = string.Empty;
    public int? ResponseTimeMs { get; set; }
    public int? StatusCode { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime CheckedAt { get; set; }
}
