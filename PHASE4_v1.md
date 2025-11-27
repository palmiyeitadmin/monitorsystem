PHASE 4: External Checks & Incident Management (Days 13-18)
4.1 Overview
Phase 4 focuses on implementing:

HTTP/HTTPS website monitoring with SSL certificate checks
TCP port connectivity checks
Ping (ICMP) checks
DNS resolution checks
Check result storage and history
Automatic incident creation from failed checks
Manual incident management with timeline
SLA tracking (response time, resolution time)
Incident assignment and escalation


4.2 Entities
Check Entity
csharp// src/ERAMonitor.Core/Entities/Check.cs

using ERAMonitor.Core.Enums;

namespace ERAMonitor.Core.Entities;

public class Check : BaseEntityWithOrganization
{
    public Guid? CustomerId { get; set; }
    public Guid? HostId { get; set; } // Optional: associate with a host
    
    // Basic Info
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public CheckType Type { get; set; }
    public string[] Tags { get; set; } = Array.Empty<string>();
    
    // Target
    public string Target { get; set; } = string.Empty; // URL, IP:Port, hostname
    
    // HTTP-specific settings
    public string? HttpMethod { get; set; } = "GET"; // GET, POST, HEAD
    public Dictionary<string, string>? HttpHeaders { get; set; }
    public string? HttpBody { get; set; }
    public int[]? ExpectedStatusCodes { get; set; } = new[] { 200, 201, 204, 301, 302 };
    public string? ExpectedKeyword { get; set; } // Text that should be present
    public bool KeywordShouldExist { get; set; } = true; // true = should exist, false = should NOT exist
    
    // SSL Certificate Monitoring
    public bool CheckSsl { get; set; } = true;
    public int SslExpiryWarningDays { get; set; } = 30;
    public int SslExpiryCriticalDays { get; set; } = 7;
    
    // TCP-specific
    public int? TcpPort { get; set; }
    
    // DNS-specific
    public string? DnsRecordType { get; set; } = "A"; // A, AAAA, CNAME, MX, TXT
    public string? ExpectedDnsResult { get; set; }
    
    // Timing
    public int IntervalSeconds { get; set; } = 60;
    public int TimeoutSeconds { get; set; } = 30;
    
    // Retry settings
    public int RetryCount { get; set; } = 2;
    public int RetryDelaySeconds { get; set; } = 5;
    
    // Failure threshold
    public int FailureThreshold { get; set; } = 3; // Consecutive failures before alerting
    public int CurrentFailureCount { get; set; } = 0;
    
    // Current Status
    public StatusType CurrentStatus { get; set; } = StatusType.Unknown;
    public DateTime? LastCheckAt { get; set; }
    public DateTime? NextCheckAt { get; set; }
    public DateTime? StatusChangedAt { get; set; }
    public StatusType? PreviousStatus { get; set; }
    
    // Last Result Summary
    public int? LastResponseTimeMs { get; set; }
    public string? LastStatusCode { get; set; }
    public string? LastError { get; set; }
    
    // SSL Info (for HTTP checks)
    public DateTime? SslExpiresAt { get; set; }
    public string? SslIssuer { get; set; }
    public string? SslSubject { get; set; }
    
    // Monitoring Settings
    public bool MonitoringEnabled { get; set; } = true;
    public bool AlertOnFailure { get; set; } = true;
    public bool AlertOnSslExpiry { get; set; } = true;
    public bool AlertOnSlowResponse { get; set; } = false;
    public int SlowResponseThresholdMs { get; set; } = 5000;
    
    // Maintenance Mode
    public bool MaintenanceMode { get; set; } = false;
    public DateTime? MaintenanceStartAt { get; set; }
    public DateTime? MaintenanceEndAt { get; set; }
    
    // Locations to check from (future: multiple locations)
    public string[]? CheckFromLocations { get; set; }
    
    // Soft Delete
    public bool IsActive { get; set; } = true;
    
    // Navigation
    public virtual Customer? Customer { get; set; }
    public virtual Host? Host { get; set; }
    public virtual ICollection<CheckResult> Results { get; set; } = new List<CheckResult>();
    
    // Helper Methods
    public bool IsInMaintenance()
    {
        if (!MaintenanceMode) return false;
        var now = DateTime.UtcNow;
        if (MaintenanceStartAt.HasValue && now < MaintenanceStartAt.Value) return false;
        if (MaintenanceEndAt.HasValue && now > MaintenanceEndAt.Value) return false;
        return true;
    }
    
    public bool ShouldAlert()
    {
        return MonitoringEnabled && !IsInMaintenance() && AlertOnFailure;
    }
    
    public bool IsSslExpiringSoon()
    {
        if (!SslExpiresAt.HasValue) return false;
        var daysUntilExpiry = (SslExpiresAt.Value - DateTime.UtcNow).TotalDays;
        return daysUntilExpiry <= SslExpiryWarningDays;
    }
    
    public bool IsSslCritical()
    {
        if (!SslExpiresAt.HasValue) return false;
        var daysUntilExpiry = (SslExpiresAt.Value - DateTime.UtcNow).TotalDays;
        return daysUntilExpiry <= SslExpiryCriticalDays;
    }
}
CheckResult Entity
csharp// src/ERAMonitor.Core/Entities/CheckResult.cs

using ERAMonitor.Core.Enums;

namespace ERAMonitor.Core.Entities;

public class CheckResult
{
    public long Id { get; set; } // BIGSERIAL for time series
    public Guid CheckId { get; set; }
    
    // Result
    public bool Success { get; set; }
    public StatusType Status { get; set; }
    
    // Timing
    public int ResponseTimeMs { get; set; }
    public DateTime CheckedAt { get; set; } = DateTime.UtcNow;
    
    // HTTP-specific
    public int? HttpStatusCode { get; set; }
    public string? HttpStatusMessage { get; set; }
    public bool? KeywordFound { get; set; }
    public long? ResponseSizeBytes { get; set; }
    
    // SSL-specific
    public bool? SslValid { get; set; }
    public DateTime? SslExpiresAt { get; set; }
    public int? SslDaysUntilExpiry { get; set; }
    public string? SslIssuer { get; set; }
    
    // DNS-specific
    public string? DnsResult { get; set; }
    public int? DnsRecordCount { get; set; }
    
    // Error Info
    public string? ErrorMessage { get; set; }
    public string? ErrorType { get; set; } // Timeout, ConnectionRefused, DnsFailure, etc.
    
    // Check Location (future: multi-location checks)
    public string? CheckedFrom { get; set; }
    
    // Navigation
    public virtual Check Check { get; set; } = null!;
}
Incident Entity
csharp// src/ERAMonitor.Core/Entities/Incident.cs

using ERAMonitor.Core.Enums;

namespace ERAMonitor.Core.Entities;

public class Incident : BaseEntityWithOrganization
{
    public Guid? CustomerId { get; set; }
    
    // Identification
    public int IncidentNumber { get; set; } // Auto-generated sequence: INC-0001
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    
    // Classification
    public IncidentSeverity Severity { get; set; } = IncidentSeverity.Medium;
    public IncidentStatus Status { get; set; } = IncidentStatus.New;
    public string? Category { get; set; } // Infrastructure, Application, Network, Security
    
    // Source (auto vs manual)
    public bool IsAutoCreated { get; set; } = false;
    public string? SourceType { get; set; } // Host, Service, Check
    public Guid? SourceId { get; set; }
    public string? SourceName { get; set; }
    
    // Assignment
    public Guid? AssignedToId { get; set; }
    public DateTime? AssignedAt { get; set; }
    
    // Acknowledgment
    public Guid? AcknowledgedById { get; set; }
    public DateTime? AcknowledgedAt { get; set; }
    
    // Resolution
    public Guid? ResolvedById { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public string? ResolutionNotes { get; set; }
    public string? RootCause { get; set; }
    
    // Closure
    public Guid? ClosedById { get; set; }
    public DateTime? ClosedAt { get; set; }
    
    // SLA Tracking
    public DateTime? SlaResponseDue { get; set; } // Time to first response
    public DateTime? SlaResolutionDue { get; set; } // Time to resolution
    public bool SlaResponseBreached { get; set; } = false;
    public bool SlaResolutionBreached { get; set; } = false;
    
    // Impact
    public string? Impact { get; set; } // Business impact description
    public int? AffectedUsersCount { get; set; }
    
    // Affected Resources (JSON array)
    public string? AffectedResources { get; set; } // [{type, id, name}]
    
    // Related Incidents
    public Guid? ParentIncidentId { get; set; }
    
    // Priority Score (calculated)
    public int PriorityScore { get; set; }
    
    // Timestamps
    public DateTime? FirstOccurredAt { get; set; }
    public DateTime? LastOccurredAt { get; set; }
    public int OccurrenceCount { get; set; } = 1;
    
    // Notification tracking
    public bool NotificationSent { get; set; } = false;
    public DateTime? NotificationSentAt { get; set; }
    
    // Navigation
    public virtual Customer? Customer { get; set; }
    public virtual User? AssignedTo { get; set; }
    public virtual User? AcknowledgedBy { get; set; }
    public virtual User? ResolvedBy { get; set; }
    public virtual User? ClosedBy { get; set; }
    public virtual Incident? ParentIncident { get; set; }
    public virtual ICollection<Incident> ChildIncidents { get; set; } = new List<Incident>();
    public virtual ICollection<IncidentTimeline> Timeline { get; set; } = new List<IncidentTimeline>();
    public virtual ICollection<IncidentResource> Resources { get; set; } = new List<IncidentResource>();
    
    // Helper Methods
    public string GetIncidentId() => $"INC-{IncidentNumber:D5}";
    
    public TimeSpan? GetTimeToAcknowledge()
    {
        if (!AcknowledgedAt.HasValue) return null;
        return AcknowledgedAt.Value - CreatedAt;
    }
    
    public TimeSpan? GetTimeToResolve()
    {
        if (!ResolvedAt.HasValue) return null;
        return ResolvedAt.Value - CreatedAt;
    }
    
    public TimeSpan? GetDuration()
    {
        var endTime = ResolvedAt ?? ClosedAt ?? DateTime.UtcNow;
        return endTime - CreatedAt;
    }
    
    public bool IsOpen => Status != IncidentStatus.Resolved && Status != IncidentStatus.Closed;
    
    public void CalculatePriorityScore()
    {
        // Priority = Severity * Urgency factors
        var severityScore = Severity switch
        {
            IncidentSeverity.Critical => 100,
            IncidentSeverity.High => 75,
            IncidentSeverity.Medium => 50,
            IncidentSeverity.Low => 25,
            IncidentSeverity.Info => 10,
            _ => 50
        };
        
        // Urgency factors
        if (SlaResponseBreached || SlaResolutionBreached) severityScore += 20;
        if (AffectedUsersCount > 100) severityScore += 15;
        else if (AffectedUsersCount > 10) severityScore += 5;
        
        PriorityScore = Math.Min(severityScore, 100);
    }
}
IncidentTimeline Entity
csharp// src/ERAMonitor.Core/Entities/IncidentTimeline.cs

namespace ERAMonitor.Core.Entities;

public class IncidentTimeline : BaseEntity
{
    public Guid IncidentId { get; set; }
    public Guid? UserId { get; set; }
    
    // Event Type
    public string EventType { get; set; } = string.Empty; 
    // Created, StatusChanged, Assigned, Acknowledged, Comment, Updated, Resolved, Closed, Escalated, Notification
    
    // Content
    public string? Title { get; set; }
    public string? Content { get; set; }
    
    // For status changes
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    
    // Visibility
    public bool IsInternal { get; set; } = false; // Internal notes not visible to customer
    public bool IsSystemGenerated { get; set; } = false;
    
    // Navigation
    public virtual Incident Incident { get; set; } = null!;
    public virtual User? User { get; set; }
}
IncidentResource Entity
csharp// src/ERAMonitor.Core/Entities/IncidentResource.cs

namespace ERAMonitor.Core.Entities;

public class IncidentResource : BaseEntity
{
    public Guid IncidentId { get; set; }
    
    public string ResourceType { get; set; } = string.Empty; // Host, Service, Check
    public Guid ResourceId { get; set; }
    public string ResourceName { get; set; } = string.Empty;
    
    // Navigation
    public virtual Incident Incident { get; set; } = null!;
}

4.3 Enums
csharp// src/ERAMonitor.Core/Enums/CheckType.cs

namespace ERAMonitor.Core.Enums;

public enum CheckType
{
    HTTP,
    TCP,
    Ping,
    DNS,
    CustomHealth // For future custom health endpoints
}
csharp// src/ERAMonitor.Core/Enums/IncidentStatus.cs

namespace ERAMonitor.Core.Enums;

public enum IncidentStatus
{
    New,
    Acknowledged,
    InProgress,
    Resolved,
    Closed
}
csharp// src/ERAMonitor.Core/Enums/IncidentSeverity.cs

namespace ERAMonitor.Core.Enums;

public enum IncidentSeverity
{
    Critical,
    High,
    Medium,
    Low,
    Info
}

4.4 DTOs
Check DTOs
csharp// src/ERAMonitor.Core/DTOs/Checks/CheckDto.cs

using ERAMonitor.Core.Enums;

namespace ERAMonitor.Core.DTOs.Checks;

public class CheckDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public CheckType Type { get; set; }
    public string Target { get; set; } = string.Empty;
    public StatusType CurrentStatus { get; set; }
    public DateTime? LastCheckAt { get; set; }
    public int? LastResponseTimeMs { get; set; }
    public bool MonitoringEnabled { get; set; }
    public int IntervalSeconds { get; set; }
    
    // SSL
    public DateTime? SslExpiresAt { get; set; }
    public int? SslDaysUntilExpiry => SslExpiresAt.HasValue 
        ? (int)(SslExpiresAt.Value - DateTime.UtcNow).TotalDays 
        : null;
    
    // Related
    public Guid? CustomerId { get; set; }
    public string? CustomerName { get; set; }
    
    public DateTime CreatedAt { get; set; }
}
csharp// src/ERAMonitor.Core/DTOs/Checks/CheckListItemDto.cs

using ERAMonitor.Core.Enums;

namespace ERAMonitor.Core.DTOs.Checks;

public class CheckListItemDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public CheckType Type { get; set; }
    public string TypeDisplay => Type.ToString();
    public string Target { get; set; } = string.Empty;
    public string[] Tags { get; set; } = Array.Empty<string>();
    
    // Status
    public StatusType CurrentStatus { get; set; }
    public string StatusDisplay { get; set; } = string.Empty;
    public DateTime? LastCheckAt { get; set; }
    public int? LastResponseTimeMs { get; set; }
    public string? LastError { get; set; }
    
    // SSL (for HTTP checks)
    public DateTime? SslExpiresAt { get; set; }
    public int? SslDaysUntilExpiry { get; set; }
    public string? SslStatus { get; set; } // Valid, Warning, Critical, Expired
    
    // Monitoring
    public bool MonitoringEnabled { get; set; }
    public bool MaintenanceMode { get; set; }
    public int IntervalSeconds { get; set; }
    
    // Related
    public CustomerSummaryDto? Customer { get; set; }
    public HostSummaryDto? Host { get; set; }
    
    // Statistics
    public decimal? Uptime24h { get; set; }
    public decimal? Uptime7d { get; set; }
    public int? AvgResponseTimeMs { get; set; }
    
    public DateTime CreatedAt { get; set; }
}

public class HostSummaryDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
}
csharp// src/ERAMonitor.Core/DTOs/Checks/CheckDetailDto.cs

using ERAMonitor.Core.Enums;

namespace ERAMonitor.Core.DTOs.Checks;

public class CheckDetailDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public CheckType Type { get; set; }
    public string Target { get; set; } = string.Empty;
    public string[] Tags { get; set; } = Array.Empty<string>();
    
    // HTTP-specific
    public HttpCheckSettingsDto? HttpSettings { get; set; }
    
    // TCP-specific
    public TcpCheckSettingsDto? TcpSettings { get; set; }
    
    // DNS-specific
    public DnsCheckSettingsDto? DnsSettings { get; set; }
    
    // SSL Settings
    public SslCheckSettingsDto? SslSettings { get; set; }
    
    // Timing
    public int IntervalSeconds { get; set; }
    public int TimeoutSeconds { get; set; }
    public int RetryCount { get; set; }
    public int RetryDelaySeconds { get; set; }
    public int FailureThreshold { get; set; }
    
    // Current Status
    public StatusType CurrentStatus { get; set; }
    public DateTime? LastCheckAt { get; set; }
    public DateTime? NextCheckAt { get; set; }
    public DateTime? StatusChangedAt { get; set; }
    public int? LastResponseTimeMs { get; set; }
    public string? LastStatusCode { get; set; }
    public string? LastError { get; set; }
    
    // SSL Info
    public SslInfoDto? SslInfo { get; set; }
    
    // Monitoring Settings
    public MonitoringSettingsDto MonitoringSettings { get; set; } = new();
    
    // Maintenance
    public MaintenanceDto? Maintenance { get; set; }
    
    // Related
    public CustomerSummaryDto? Customer { get; set; }
    public HostSummaryDto? Host { get; set; }
    
    // Statistics
    public CheckStatisticsDto Statistics { get; set; } = new();
    
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class HttpCheckSettingsDto
{
    public string Method { get; set; } = "GET";
    public Dictionary<string, string>? Headers { get; set; }
    public string? Body { get; set; }
    public int[] ExpectedStatusCodes { get; set; } = new[] { 200 };
    public string? ExpectedKeyword { get; set; }
    public bool KeywordShouldExist { get; set; } = true;
}

public class TcpCheckSettingsDto
{
    public int Port { get; set; }
}

public class DnsCheckSettingsDto
{
    public string RecordType { get; set; } = "A";
    public string? ExpectedResult { get; set; }
}

public class SslCheckSettingsDto
{
    public bool Enabled { get; set; }
    public int WarningDays { get; set; }
    public int CriticalDays { get; set; }
}

public class SslInfoDto
{
    public DateTime? ExpiresAt { get; set; }
    public int? DaysUntilExpiry { get; set; }
    public string? Issuer { get; set; }
    public string? Subject { get; set; }
    public string Status { get; set; } = "Unknown"; // Valid, Warning, Critical, Expired
}

public class MonitoringSettingsDto
{
    public bool Enabled { get; set; }
    public bool AlertOnFailure { get; set; }
    public bool AlertOnSslExpiry { get; set; }
    public bool AlertOnSlowResponse { get; set; }
    public int SlowResponseThresholdMs { get; set; }
}

public class MaintenanceDto
{
    public bool InMaintenance { get; set; }
    public DateTime? StartAt { get; set; }
    public DateTime? EndAt { get; set; }
}

public class CheckStatisticsDto
{
    public decimal Uptime24h { get; set; }
    public decimal Uptime7d { get; set; }
    public decimal Uptime30d { get; set; }
    public int AvgResponseTimeMs { get; set; }
    public int MinResponseTimeMs { get; set; }
    public int MaxResponseTimeMs { get; set; }
    public int ChecksLast24h { get; set; }
    public int FailuresLast24h { get; set; }
}
csharp// src/ERAMonitor.Core/DTOs/Checks/CreateCheckRequest.cs

using System.ComponentModel.DataAnnotations;
using ERAMonitor.Core.Enums;

namespace ERAMonitor.Core.DTOs.Checks;

public class CreateCheckRequest
{
    [Required(ErrorMessage = "Name is required")]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;
    
    public string? Description { get; set; }
    
    [Required(ErrorMessage = "Type is required")]
    public CheckType Type { get; set; }
    
    [Required(ErrorMessage = "Target is required")]
    [MaxLength(500)]
    public string Target { get; set; } = string.Empty;
    
    public string[]? Tags { get; set; }
    
    // Assignment
    public Guid? CustomerId { get; set; }
    public Guid? HostId { get; set; }
    
    // HTTP Settings
    public string HttpMethod { get; set; } = "GET";
    public Dictionary<string, string>? HttpHeaders { get; set; }
    public string? HttpBody { get; set; }
    public int[]? ExpectedStatusCodes { get; set; }
    public string? ExpectedKeyword { get; set; }
    public bool KeywordShouldExist { get; set; } = true;
    
    // SSL Settings
    public bool CheckSsl { get; set; } = true;
    public int SslExpiryWarningDays { get; set; } = 30;
    public int SslExpiryCriticalDays { get; set; } = 7;
    
    // TCP Settings
    public int? TcpPort { get; set; }
    
    // DNS Settings
    public string? DnsRecordType { get; set; } = "A";
    public string? ExpectedDnsResult { get; set; }
    
    // Timing
    [Range(10, 3600)]
    public int IntervalSeconds { get; set; } = 60;
    
    [Range(1, 120)]
    public int TimeoutSeconds { get; set; } = 30;
    
    [Range(0, 5)]
    public int RetryCount { get; set; } = 2;
    
    [Range(1, 60)]
    public int RetryDelaySeconds { get; set; } = 5;
    
    [Range(1, 10)]
    public int FailureThreshold { get; set; } = 3;
    
    // Monitoring
    public bool MonitoringEnabled { get; set; } = true;
    public bool AlertOnFailure { get; set; } = true;
    public bool AlertOnSslExpiry { get; set; } = true;
    public bool AlertOnSlowResponse { get; set; } = false;
    
    [Range(100, 60000)]
    public int SlowResponseThresholdMs { get; set; } = 5000;
}
csharp// src/ERAMonitor.Core/DTOs/Checks/UpdateCheckRequest.cs

using System.ComponentModel.DataAnnotations;
using ERAMonitor.Core.Enums;

namespace ERAMonitor.Core.DTOs.Checks;

public class UpdateCheckRequest
{
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;
    
    public string? Description { get; set; }
    
    [Required]
    [MaxLength(500)]
    public string Target { get; set; } = string.Empty;
    
    public string[]? Tags { get; set; }
    
    public Guid? CustomerId { get; set; }
    public Guid? HostId { get; set; }
    
    // HTTP Settings
    public string HttpMethod { get; set; } = "GET";
    public Dictionary<string, string>? HttpHeaders { get; set; }
    public string? HttpBody { get; set; }
    public int[]? ExpectedStatusCodes { get; set; }
    public string? ExpectedKeyword { get; set; }
    public bool KeywordShouldExist { get; set; } = true;
    
    // SSL Settings
    public bool CheckSsl { get; set; } = true;
    public int SslExpiryWarningDays { get; set; } = 30;
    public int SslExpiryCriticalDays { get; set; } = 7;
    
    // TCP Settings
    public int? TcpPort { get; set; }
    
    // DNS Settings
    public string? DnsRecordType { get; set; }
    public string? ExpectedDnsResult { get; set; }
    
    // Timing
    [Range(10, 3600)]
    public int IntervalSeconds { get; set; } = 60;
    
    [Range(1, 120)]
    public int TimeoutSeconds { get; set; } = 30;
    
    [Range(0, 5)]
    public int RetryCount { get; set; } = 2;
    
    [Range(1, 60)]
    public int RetryDelaySeconds { get; set; } = 5;
    
    [Range(1, 10)]
    public int FailureThreshold { get; set; } = 3;
    
    // Monitoring
    public bool MonitoringEnabled { get; set; } = true;
    public bool AlertOnFailure { get; set; } = true;
    public bool AlertOnSslExpiry { get; set; } = true;
    public bool AlertOnSlowResponse { get; set; } = false;
    
    [Range(100, 60000)]
    public int SlowResponseThresholdMs { get; set; } = 5000;
    
    public bool IsActive { get; set; } = true;
}
csharp// src/ERAMonitor.Core/DTOs/Checks/CheckResultDto.cs

using ERAMonitor.Core.Enums;

namespace ERAMonitor.Core.DTOs.Checks;

public class CheckResultDto
{
    public long Id { get; set; }
    public bool Success { get; set; }
    public StatusType Status { get; set; }
    public int ResponseTimeMs { get; set; }
    public DateTime CheckedAt { get; set; }
    
    // HTTP
    public int? HttpStatusCode { get; set; }
    public string? HttpStatusMessage { get; set; }
    public bool? KeywordFound { get; set; }
    
    // SSL
    public bool? SslValid { get; set; }
    public int? SslDaysUntilExpiry { get; set; }
    
    // DNS
    public string? DnsResult { get; set; }
    
    // Error
    public string? ErrorMessage { get; set; }
    public string? ErrorType { get; set; }
    
    public string? CheckedFrom { get; set; }
}
csharp// src/ERAMonitor.Core/DTOs/Checks/CheckHistoryDto.cs

namespace ERAMonitor.Core.DTOs.Checks;

public class CheckHistoryDto
{
    public Guid CheckId { get; set; }
    public List<CheckResultDto> Results { get; set; } = new();
    public List<ResponseTimeDataPoint> ResponseTimeHistory { get; set; } = new();
    public UptimeHistoryDto UptimeHistory { get; set; } = new();
}

public class ResponseTimeDataPoint
{
    public DateTime Timestamp { get; set; }
    public int ResponseTimeMs { get; set; }
    public bool Success { get; set; }
}

public class UptimeHistoryDto
{
    public List<UptimeDataPoint> Daily { get; set; } = new();
    public List<UptimeDataPoint> Hourly { get; set; } = new();
}

public class UptimeDataPoint
{
    public DateTime Period { get; set; }
    public decimal UptimePercent { get; set; }
    public int TotalChecks { get; set; }
    public int FailedChecks { get; set; }
}
Incident DTOs
csharp// src/ERAMonitor.Core/DTOs/Incidents/IncidentDto.cs

using ERAMonitor.Core.Enums;

namespace ERAMonitor.Core.DTOs.Incidents;

public class IncidentDto
{
    public Guid Id { get; set; }
    public string IncidentId { get; set; } = string.Empty; // INC-00001
    public string Title { get; set; } = string.Empty;
    public IncidentSeverity Severity { get; set; }
    public IncidentStatus Status { get; set; }
    public bool IsAutoCreated { get; set; }
    public string? SourceType { get; set; }
    public string? SourceName { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ResolvedAt { get; set; }
    
    // Related
    public Guid? CustomerId { get; set; }
    public string? CustomerName { get; set; }
    public UserSummaryDto? AssignedTo { get; set; }
}
csharp// src/ERAMonitor.Core/DTOs/Incidents/IncidentListItemDto.cs

using ERAMonitor.Core.Enums;

namespace ERAMonitor.Core.DTOs.Incidents;

public class IncidentListItemDto
{
    public Guid Id { get; set; }
    public string IncidentId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public IncidentSeverity Severity { get; set; }
    public string SeverityDisplay => Severity.ToString();
    public IncidentStatus Status { get; set; }
    public string StatusDisplay => Status.ToString();
    public string? Category { get; set; }
    public int PriorityScore { get; set; }
    
    // Source
    public bool IsAutoCreated { get; set; }
    public string? SourceType { get; set; }
    public string? SourceName { get; set; }
    
    // Assignment
    public UserSummaryDto? AssignedTo { get; set; }
    
    // SLA
    public bool SlaResponseBreached { get; set; }
    public bool SlaResolutionBreached { get; set; }
    public DateTime? SlaResponseDue { get; set; }
    public DateTime? SlaResolutionDue { get; set; }
    
    // Timing
    public DateTime CreatedAt { get; set; }
    public DateTime? AcknowledgedAt { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public string? Duration { get; set; }
    
    // Related
    public CustomerSummaryDto? Customer { get; set; }
    public int ResourceCount { get; set; }
    public int TimelineCount { get; set; }
}

public class UserSummaryDto
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
}
csharp// src/ERAMonitor.Core/DTOs/Incidents/IncidentDetailDto.cs

using ERAMonitor.Core.Enums;

namespace ERAMonitor.Core.DTOs.Incidents;

public class IncidentDetailDto
{
    public Guid Id { get; set; }
    public string IncidentId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public IncidentSeverity Severity { get; set; }
    public IncidentStatus Status { get; set; }
    public string? Category { get; set; }
    public int PriorityScore { get; set; }
    
    // Source
    public bool IsAutoCreated { get; set; }
    public string? SourceType { get; set; }
    public Guid? SourceId { get; set; }
    public string? SourceName { get; set; }
    
    // Assignment & Actions
    public UserSummaryDto? AssignedTo { get; set; }
    public DateTime? AssignedAt { get; set; }
    public UserSummaryDto? AcknowledgedBy { get; set; }
    public DateTime? AcknowledgedAt { get; set; }
    public UserSummaryDto? ResolvedBy { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public string? ResolutionNotes { get; set; }
    public string? RootCause { get; set; }
    public UserSummaryDto? ClosedBy { get; set; }
    public DateTime? ClosedAt { get; set; }
    
    // SLA
    public SlaInfoDto Sla { get; set; } = new();
    
    // Impact
    public string? Impact { get; set; }
    public int? AffectedUsersCount { get; set; }
    
    // Occurrence
    public DateTime? FirstOccurredAt { get; set; }
    public DateTime? LastOccurredAt { get; set; }
    public int OccurrenceCount { get; set; }
    
    // Related
    public CustomerSummaryDto? Customer { get; set; }
    public Guid? ParentIncidentId { get; set; }
    public string? ParentIncidentTitle { get; set; }
    
    // Resources
    public List<IncidentResourceDto> Resources { get; set; } = new();
    
    // Timeline
    public List<IncidentTimelineDto> Timeline { get; set; } = new();
    
    // Child Incidents
    public List<IncidentSummaryDto> ChildIncidents { get; set; } = new();
    
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class SlaInfoDto
{
    public DateTime? ResponseDue { get; set; }
    public DateTime? ResolutionDue { get; set; }
    public bool ResponseBreached { get; set; }
    public bool ResolutionBreached { get; set; }
    public string? TimeToResponseDue { get; set; }
    public string? TimeToResolutionDue { get; set; }
}

public class IncidentResourceDto
{
    public Guid Id { get; set; }
    public string ResourceType { get; set; } = string.Empty;
    public Guid ResourceId { get; set; }
    public string ResourceName { get; set; } = string.Empty;
}

public class IncidentTimelineDto
{
    public Guid Id { get; set; }
    public string EventType { get; set; } = string.Empty;
    public string? Title { get; set; }
    public string? Content { get; set; }
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    public bool IsInternal { get; set; }
    public bool IsSystemGenerated { get; set; }
    public UserSummaryDto? User { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class IncidentSummaryDto
{
    public Guid Id { get; set; }
    public string IncidentId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public IncidentSeverity Severity { get; set; }
    public IncidentStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
}
csharp// src/ERAMonitor.Core/DTOs/Incidents/CreateIncidentRequest.cs

using System.ComponentModel.DataAnnotations;
using ERAMonitor.Core.Enums;

namespace ERAMonitor.Core.DTOs.Incidents;

public class CreateIncidentRequest
{
    [Required(ErrorMessage = "Title is required")]
    [MaxLength(500)]
    public string Title { get; set; } = string.Empty;
    
    public string? Description { get; set; }
    
    [Required]
    public IncidentSeverity Severity { get; set; } = IncidentSeverity.Medium;
    
    [MaxLength(100)]
    public string? Category { get; set; }
    
    public Guid? CustomerId { get; set; }
    public Guid? AssignedToId { get; set; }
    
    public string? Impact { get; set; }
    public int? AffectedUsersCount { get; set; }
    
    // Resources to link
    public List<CreateIncidentResourceRequest>? Resources { get; set; }
}

public class CreateIncidentResourceRequest
{
    [Required]
    public string ResourceType { get; set; } = string.Empty;
    
    [Required]
    public Guid ResourceId { get; set; }
    
    [Required]
    public string ResourceName { get; set; } = string.Empty;
}
csharp// src/ERAMonitor.Core/DTOs/Incidents/UpdateIncidentRequest.cs

using System.ComponentModel.DataAnnotations;
using ERAMonitor.Core.Enums;

namespace ERAMonitor.Core.DTOs.Incidents;

public class UpdateIncidentRequest
{
    [Required]
    [MaxLength(500)]
    public string Title { get; set; } = string.Empty;
    
    public string? Description { get; set; }
    
    [Required]
    public IncidentSeverity Severity { get; set; }
    
    [MaxLength(100)]
    public string? Category { get; set; }
    
    public Guid? AssignedToId { get; set; }
    
    public string? Impact { get; set; }
    public int? AffectedUsersCount { get; set; }
}
csharp// src/ERAMonitor.Core/DTOs/Incidents/IncidentActionRequests.cs

using System.ComponentModel.DataAnnotations;
using ERAMonitor.Core.Enums;

namespace ERAMonitor.Core.DTOs.Incidents;

public class AcknowledgeIncidentRequest
{
    public string? Comment { get; set; }
}

public class AssignIncidentRequest
{
    [Required]
    public Guid AssignToUserId { get; set; }
    
    public string? Comment { get; set; }
}

public class UpdateStatusRequest
{
    [Required]
    public IncidentStatus Status { get; set; }
    
    public string? Comment { get; set; }
}

public class ResolveIncidentRequest
{
    [MaxLength(2000)]
    public string? ResolutionNotes { get; set; }
    
    [MaxLength(1000)]
    public string? RootCause { get; set; }
}

public class AddCommentRequest
{
    [Required(ErrorMessage = "Comment is required")]
    [MaxLength(5000)]
    public string Content { get; set; } = string.Empty;
    
    public bool IsInternal { get; set; } = false;
}

public class LinkResourceRequest
{
    [Required]
    public string ResourceType { get; set; } = string.Empty;
    
    [Required]
    public Guid ResourceId { get; set; }
    
    [Required]
    public string ResourceName { get; set; } = string.Empty;
}

public class EscalateIncidentRequest
{
    [Required]
    public IncidentSeverity NewSeverity { get; set; }
    
    public Guid? AssignToUserId { get; set; }
    
    [Required]
    public string Reason { get; set; } = string.Empty;
}

4.5 Repository Interfaces
csharp// src/ERAMonitor.Core/Interfaces/Repositories/ICheckRepository.cs

using ERAMonitor.Core.Entities;
using ERAMonitor.Core.DTOs.Common;
using ERAMonitor.Core.DTOs.Checks;
using ERAMonitor.Core.Enums;

namespace ERAMonitor.Core.Interfaces.Repositories;

public interface ICheckRepository : IRepository<Check>
{
    Task<PagedResponse<CheckListItemDto>> GetPagedAsync(
        Guid organizationId,
        PagedRequest request,
        CheckType? type = null,
        StatusType? status = null,
        Guid? customerId = null,
        Guid? hostId = null,
        bool? monitoringEnabled = null,
        bool? isActive = null);
    
    Task<CheckDetailDto?> GetDetailAsync(Guid id, Guid organizationId);
    
    Task<List<Check>> GetDueChecksAsync(DateTime before);
    Task<List<Check>> GetByCustomerAsync(Guid customerId);
    Task<List<Check>> GetByHostAsync(Guid hostId);
    
    Task<List<Check>> GetExpiringCertificatesAsync(int daysUntilExpiry);
    
    Task<int> GetCountByStatusAsync(Guid organizationId, StatusType status);
    Task<Dictionary<StatusType, int>> GetStatusCountsAsync(Guid organizationId);
    Task<Dictionary<CheckType, int>> GetCountsByTypeAsync(Guid organizationId);
}
csharp// src/ERAMonitor.Core/Interfaces/Repositories/ICheckResultRepository.cs

using ERAMonitor.Core.Entities;
using ERAMonitor.Core.DTOs.Checks;

namespace ERAMonitor.Core.Interfaces.Repositories;

public interface ICheckResultRepository
{
    Task<CheckResult> AddAsync(CheckResult result);
    Task AddRangeAsync(IEnumerable<CheckResult> results);
    
    Task<List<CheckResult>> GetByCheckAsync(Guid checkId, int limit = 100);
    Task<List<CheckResult>> GetByCheckAsync(Guid checkId, DateTime from, DateTime to);
    Task<CheckResult?> GetLatestByCheckAsync(Guid checkId);
    
    Task<CheckHistoryDto> GetHistoryAsync(Guid checkId, DateTime from, DateTime to);
    Task<List<ResponseTimeDataPoint>> GetResponseTimeHistoryAsync(Guid checkId, DateTime from, DateTime to, string interval = "5m");
    
    // Statistics
    Task<decimal> CalculateUptimeAsync(Guid checkId, DateTime from, DateTime to);
    Task<(int avg, int min, int max)> GetResponseTimeStatsAsync(Guid checkId, DateTime from, DateTime to);
    
    // Cleanup
    Task<int> DeleteOlderThanAsync(int retentionDays);
    
    Task SaveChangesAsync();
}
csharp// src/ERAMonitor.Core/Interfaces/Repositories/IIncidentRepository.cs

using ERAMonitor.Core.Entities;
using ERAMonitor.Core.DTOs.Common;
using ERAMonitor.Core.DTOs.Incidents;
using ERAMonitor.Core.Enums;

namespace ERAMonitor.Core.Interfaces.Repositories;

public interface IIncidentRepository : IRepository<Incident>
{
    Task<int> GetNextIncidentNumberAsync(Guid organizationId);
    
    Task<PagedResponse<IncidentListItemDto>> GetPagedAsync(
        Guid organizationId,
        PagedRequest request,
        IncidentStatus? status = null,
        IncidentSeverity? severity = null,
        Guid? customerId = null,
        Guid? assignedToId = null,
        bool? isOpen = null,
        DateTime? from = null,
        DateTime? to = null);
    
    Task<IncidentDetailDto?> GetDetailAsync(Guid id, Guid organizationId);
    
    Task<List<Incident>> GetOpenBySourceAsync(string sourceType, Guid sourceId);
    Task<Incident?> GetOpenBySourceAsync(Guid organizationId, string sourceType, Guid sourceId);
    
    Task<List<Incident>> GetByCustomerAsync(Guid customerId, bool onlyOpen = false);
    Task<List<Incident>> GetAssignedToUserAsync(Guid userId, bool onlyOpen = true);
    
    Task<List<Incident>> GetSlaBreachingAsync(DateTime before);
    
    Task<int> GetCountByStatusAsync(Guid organizationId, IncidentStatus status);
    Task<Dictionary<IncidentStatus, int>> GetStatusCountsAsync(Guid organizationId);
    Task<Dictionary<IncidentSeverity, int>> GetSeverityCountsAsync(Guid organizationId, bool onlyOpen = true);
    
    // Statistics
    Task<IncidentStatisticsDto> GetStatisticsAsync(Guid organizationId, DateTime from, DateTime to);
}

public class IncidentStatisticsDto
{
    public int TotalIncidents { get; set; }
    public int OpenIncidents { get; set; }
    public int ResolvedIncidents { get; set; }
    public decimal AvgTimeToAcknowledgeMinutes { get; set; }
    public decimal AvgTimeToResolveMinutes { get; set; }
    public int SlaBreaches { get; set; }
    public Dictionary<IncidentSeverity, int> BySeverity { get; set; } = new();
    public Dictionary<string, int> ByCategory { get; set; } = new();
}
csharp// src/ERAMonitor.Core/Interfaces/Repositories/IIncidentTimelineRepository.cs

using ERAMonitor.Core.Entities;
using ERAMonitor.Core.DTOs.Incidents;

namespace ERAMonitor.Core.Interfaces.Repositories;

public interface IIncidentTimelineRepository : IRepository<IncidentTimeline>
{
    Task<List<IncidentTimelineDto>> GetByIncidentAsync(Guid incidentId, bool includeInternal = true);
    Task<List<IncidentTimeline>> GetByIncidentEntityAsync(Guid incidentId);
}

4.6 Service Interfaces
csharp// src/ERAMonitor.Core/Interfaces/Services/ICheckService.cs

using ERAMonitor.Core.DTOs.Common;
using ERAMonitor.Core.DTOs.Checks;
using ERAMonitor.Core.Enums;

namespace ERAMonitor.Core.Interfaces.Services;

public interface ICheckService
{
    Task<PagedResponse<CheckListItemDto>> GetPagedAsync(
        Guid organizationId,
        PagedRequest request,
        CheckType? type = null,
        StatusType? status = null,
        Guid? customerId = null,
        Guid? hostId = null);
    
    Task<CheckDetailDto> GetByIdAsync(Guid id, Guid organizationId);
    Task<CheckDetailDto> CreateAsync(Guid organizationId, CreateCheckRequest request);
    Task<CheckDetailDto> UpdateAsync(Guid id, Guid organizationId, UpdateCheckRequest request);
    Task DeleteAsync(Guid id, Guid organizationId);
    
    Task<bool> ToggleMonitoringAsync(Guid id, Guid organizationId);
    Task<CheckDetailDto> SetMaintenanceAsync(Guid id, Guid organizationId, SetMaintenanceRequest request);
    
    // Manual check trigger
    Task<CheckResultDto> RunCheckNowAsync(Guid id, Guid organizationId);
    
    // History
    Task<CheckHistoryDto> GetHistoryAsync(Guid id, Guid organizationId, DateTime from, DateTime to);
    Task<List<CheckResultDto>> GetResultsAsync(Guid id, Guid organizationId, int limit = 100);
    
    // Statistics
    Task<Dictionary<StatusType, int>> GetStatusCountsAsync(Guid organizationId);
    Task<Dictionary<CheckType, int>> GetTypeCountsAsync(Guid organizationId);
    Task<List<CheckListItemDto>> GetExpiringCertificatesAsync(Guid organizationId, int daysUntilExpiry = 30);
}
csharp// src/ERAMonitor.Core/Interfaces/Services/ICheckExecutorService.cs

using ERAMonitor.Core.Entities;
using ERAMonitor.Core.DTOs.Checks;

namespace ERAMonitor.Core.Interfaces.Services;

public interface ICheckExecutorService
{
    Task<CheckResult> ExecuteCheckAsync(Check check);
    Task<CheckResult> ExecuteHttpCheckAsync(Check check);
    Task<CheckResult> ExecuteTcpCheckAsync(Check check);
    Task<CheckResult> ExecutePingCheckAsync(Check check);
    Task<CheckResult> ExecuteDnsCheckAsync(Check check);
}
csharp// src/ERAMonitor.Core/Interfaces/Services/IIncidentService.cs

using ERAMonitor.Core.DTOs.Common;
using ERAMonitor.Core.DTOs.Incidents;
using ERAMonitor.Core.Enums;

namespace ERAMonitor.Core.Interfaces.Services;

public interface IIncidentService
{
    Task<PagedResponse<IncidentListItemDto>> GetPagedAsync(
        Guid organizationId,
        PagedRequest request,
        IncidentStatus? status = null,
        IncidentSeverity? severity = null,
        Guid? customerId = null,
        Guid? assignedToId = null,
        bool? isOpen = null);
    
    Task<IncidentDetailDto> GetByIdAsync(Guid id, Guid organizationId);
    Task<IncidentDetailDto> CreateAsync(Guid organizationId, Guid userId, CreateIncidentRequest request);
    Task<IncidentDetailDto> UpdateAsync(Guid id, Guid organizationId, Guid userId, UpdateIncidentRequest request);
    
    // Actions
    Task<IncidentDetailDto> AcknowledgeAsync(Guid id, Guid organizationId, Guid userId, AcknowledgeIncidentRequest request);
    Task<IncidentDetailDto> AssignAsync(Guid id, Guid organizationId, Guid userId, AssignIncidentRequest request);
    Task<IncidentDetailDto> UpdateStatusAsync(Guid id, Guid organizationId, Guid userId, UpdateStatusRequest request);
    Task<IncidentDetailDto> ResolveAsync(Guid id, Guid organizationId, Guid userId, ResolveIncidentRequest request);
    Task<IncidentDetailDto> CloseAsync(Guid id, Guid organizationId, Guid userId);
    Task<IncidentDetailDto> ReopenAsync(Guid id, Guid organizationId, Guid userId, string? reason = null);
    Task<IncidentDetailDto> EscalateAsync(Guid id, Guid organizationId, Guid userId, EscalateIncidentRequest request);
    
    // Timeline
    Task<IncidentTimelineDto> AddCommentAsync(Guid id, Guid organizationId, Guid userId, AddCommentRequest request);
    Task<List<IncidentTimelineDto>> GetTimelineAsync(Guid id, Guid organizationId, bool includeInternal = true);
    
    // Resources
    Task<IncidentResourceDto> LinkResourceAsync(Guid id, Guid organizationId, LinkResourceRequest request);
    Task UnlinkResourceAsync(Guid id, Guid organizationId, Guid resourceId);
    
    // Auto-incident creation (from checks/hosts/services)
    Task<Guid> CreateAutoIncidentAsync(
        string sourceType,
        Guid sourceId,
        string sourceName,
        string title,
        string description,
        IncidentSeverity severity,
        Guid? customerId,
        Guid organizationId);
    
    // Auto-resolve (when source recovers)
    Task AutoResolveIncidentsAsync(string sourceType, Guid sourceId, string resolutionNote);
    
    // Statistics
    Task<Dictionary<IncidentStatus, int>> GetStatusCountsAsync(Guid organizationId);
    Task<Dictionary<IncidentSeverity, int>> GetSeverityCountsAsync(Guid organizationId);
    Task<IncidentStatisticsDto> GetStatisticsAsync(Guid organizationId, DateTime from, DateTime to);
}

4.7 Service Implementations
CheckExecutorService
csharp// src/ERAMonitor.Infrastructure/Services/CheckExecutorService.cs

using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using DnsClient;
using Microsoft.Extensions.Logging;
using ERAMonitor.Core.Entities;
using ERAMonitor.Core.Enums;
using ERAMonitor.Core.Interfaces.Services;

namespace ERAMonitor.Infrastructure.Services;

public class CheckExecutorService : ICheckExecutorService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<CheckExecutorService> _logger;
    
    public CheckExecutorService(
        IHttpClientFactory httpClientFactory,
        ILogger<CheckExecutorService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }
    
    public async Task<CheckResult> ExecuteCheckAsync(Check check)
    {
        return check.Type switch
        {
            CheckType.HTTP => await ExecuteHttpCheckAsync(check),
            CheckType.TCP => await ExecuteTcpCheckAsync(check),
            CheckType.Ping => await ExecutePingCheckAsync(check),
            CheckType.DNS => await ExecuteDnsCheckAsync(check),
            _ => CreateErrorResult(check, "Unknown check type", "UnsupportedCheckType")
        };
    }
    
    public async Task<CheckResult> ExecuteHttpCheckAsync(Check check)
    {
        var result = new CheckResult
        {
            CheckId = check.Id,
            CheckedAt = DateTime.UtcNow
        };
        
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            // Create handler with SSL callback if we need to check certificates
            var handler = new HttpClientHandler();
            X509Certificate2? certificate = null;
            
            if (check.CheckSsl && check.Target.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                handler.ServerCertificateCustomValidationCallback = (message, cert, chain, sslPolicyErrors) =>
                {
                    if (cert != null)
                    {
                        certificate = new X509Certificate2(cert);
                    }
                    return true; // Accept all for the check, we'll evaluate separately
                };
            }
            
            using var client = new HttpClient(handler)
            {
                Timeout = TimeSpan.FromSeconds(check.TimeoutSeconds)
            };
            
            // Build request
            var request = new HttpRequestMessage(
                new HttpMethod(check.HttpMethod ?? "GET"),
                check.Target
            );
            
            // Add headers
            if (check.HttpHeaders != null)
            {
                foreach (var header in check.HttpHeaders)
                {
                    request.Headers.TryAddWithoutValidation(header.Key, header.Value);
                }
            }
            
            // Add body for POST/PUT
            if (!string.IsNullOrEmpty(check.HttpBody) && 
                (check.HttpMethod == "POST" || check.HttpMethod == "PUT" || check.HttpMethod == "PATCH"))
            {
                request.Content = new StringContent(check.HttpBody, Encoding.UTF8, "application/json");
            }
            
            // Execute request
            var response = await client.SendAsync(request);
            
            stopwatch.Stop();
            result.ResponseTimeMs = (int)stopwatch.ElapsedMilliseconds;
            result.HttpStatusCode = (int)response.StatusCode;
            result.HttpStatusMessage = response.ReasonPhrase;
            
            // Check status code
            var expectedCodes = check.ExpectedStatusCodes ?? new[] { 200, 201, 204, 301, 302 };
            var statusCodeOk = expectedCodes.Contains((int)response.StatusCode);
            
            // Check keyword if specified
            bool keywordOk = true;
            if (!string.IsNullOrEmpty(check.ExpectedKeyword))
            {
                var content = await response.Content.ReadAsStringAsync();
                result.ResponseSizeBytes = content.Length;
                
                var keywordFound = content.Contains(check.ExpectedKeyword, StringComparison.OrdinalIgnoreCase);
                result.KeywordFound = keywordFound;
                
                keywordOk = check.KeywordShouldExist ? keywordFound : !keywordFound;
            }
            
            // Process SSL certificate
            if (certificate != null)
            {
                result.SslExpiresAt = certificate.NotAfter;
                result.SslDaysUntilExpiry = (int)(certificate.NotAfter - DateTime.UtcNow).TotalDays;
                result.SslIssuer = certificate.Issuer;
                result.SslValid = certificate.NotAfter > DateTime.UtcNow && certificate.NotBefore < DateTime.UtcNow;
            }
            
            // Determine overall success
            result.Success = statusCodeOk && keywordOk;
            result.Status = result.Success ? StatusType.Up : StatusType.Down;
            
            if (!statusCodeOk)
            {
                result.ErrorMessage = $"Unexpected status code: {response.StatusCode}";
                result.ErrorType = "UnexpectedStatusCode";
            }
            else if (!keywordOk)
            {
                result.ErrorMessage = check.KeywordShouldExist 
                    ? $"Expected keyword '{check.ExpectedKeyword}' not found" 
                    : $"Unexpected keyword '{check.ExpectedKeyword}' found";
                result.ErrorType = "KeywordMismatch";
            }
        }
        catch (TaskCanceledException)
        {
            stopwatch.Stop();
            result.ResponseTimeMs = (int)stopwatch.ElapsedMilliseconds;
            result.Success = false;
            result.Status = StatusType.Down;
            result.ErrorMessage = $"Request timed out after {check.TimeoutSeconds} seconds";
            result.ErrorType = "Timeout";
        }
        catch (HttpRequestException ex)
        {
            stopwatch.Stop();
            result.ResponseTimeMs = (int)stopwatch.ElapsedMilliseconds;
            result.Success = false;
            result.Status = StatusType.Down;
            result.ErrorMessage = ex.Message;
            result.ErrorType = DetermineHttpErrorType(ex);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            result.ResponseTimeMs = (int)stopwatch.ElapsedMilliseconds;
            result.Success = false;
            result.Status = StatusType.Down;
            result.ErrorMessage = ex.Message;
            result.ErrorType = "UnknownError";
            
            _logger.LogError(ex, "Error executing HTTP check for {Target}", check.Target);
        }
        
        return result;
    }
    
    public async Task<CheckResult> ExecuteTcpCheckAsync(Check check)
    {
        var result = new CheckResult
        {
            CheckId = check.Id,
            CheckedAt = DateTime.UtcNow
        };
        
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            // Parse target (host:port or just host if port is specified separately)
            var (host, port) = ParseTargetForTcp(check.Target, check.TcpPort);
            
            using var client = new TcpClient();
            var connectTask = client.ConnectAsync(host, port);
            
            if (await Task.WhenAny(connectTask, Task.Delay(TimeSpan.FromSeconds(check.TimeoutSeconds))) == connectTask)
            {
                await connectTask; // Throw if faulted
                
                stopwatch.Stop();
                result.ResponseTimeMs = (int)stopwatch.ElapsedMilliseconds;
                result.Success = true;
                result.Status = StatusType.Up;
            }
            else
            {
                stopwatch.Stop();
                result.ResponseTimeMs = (int)stopwatch.ElapsedMilliseconds;
                result.Success = false;
                result.Status = StatusType.Down;
                result.ErrorMessage = $"Connection timed out after {check.TimeoutSeconds} seconds";
                result.ErrorType = "Timeout";
            }
        }
        catch (SocketException ex)
        {
            stopwatch.Stop();
            result.ResponseTimeMs = (int)stopwatch.ElapsedMilliseconds;
            result.Success = false;
            result.Status = StatusType.Down;
            result.ErrorMessage = ex.Message;
            result.ErrorType = ex.SocketErrorCode.ToString();
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            result.ResponseTimeMs = (int)stopwatch.ElapsedMilliseconds;
            result.Success = false;
            result.Status = StatusType.Down;
            result.ErrorMessage = ex.Message;
            result.ErrorType = "UnknownError";
            
            _logger.LogError(ex, "Error executing TCP check for {Target}", check.Target);
        }
        
        return result;
    }
    
    public async Task<CheckResult> ExecutePingCheckAsync(Check check)
    {
        var result = new CheckResult
        {
            CheckId = check.Id,
            CheckedAt = DateTime.UtcNow
        };
        
        try
        {
            using var ping = new Ping();
            var reply = await ping.SendPingAsync(
                check.Target,
                (int)TimeSpan.FromSeconds(check.TimeoutSeconds).TotalMilliseconds
            );
            
            result.ResponseTimeMs = (int)reply.RoundtripTime;
            result.Success = reply.Status == IPStatus.Success;
            result.Status = result.Success ? StatusType.Up : StatusType.Down;
            
            if (!result.Success)
            {
                result.ErrorMessage = $"Ping failed: {reply.Status}";
                result.ErrorType = reply.Status.ToString();
            }
        }
        catch (PingException ex)
        {
            result.Success = false;
            result.Status = StatusType.Down;
            result.ErrorMessage = ex.Message;
            result.ErrorType = "PingException";
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.Status = StatusType.Down;
            result.ErrorMessage = ex.Message;
            result.ErrorType = "UnknownError";
            
            _logger.LogError(ex, "Error executing Ping check for {Target}", check.Target);
        }
        
        return result;
    }
    
    public async Task<CheckResult> ExecuteDnsCheckAsync(Check check)
    {
        var result = new CheckResult
        {
            CheckId = check.Id,
            CheckedAt = DateTime.UtcNow
        };
        
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            var lookup = new LookupClient();
            var queryType = ParseDnsQueryType(check.DnsRecordType ?? "A");
            
            var response = await lookup.QueryAsync(check.Target, queryType);
            
            stopwatch.Stop();
            result.ResponseTimeMs = (int)stopwatch.ElapsedMilliseconds;
            
            if (response.HasError)
            {
                result.Success = false;
                result.Status = StatusType.Down;
                result.ErrorMessage = response.ErrorMessage;
                result.ErrorType = "DnsError";
            }
            else
            {
                var records = response.Answers.Select(a => a.ToString()).ToList();
                result.DnsResult = string.Join("; ", records);
                result.DnsRecordCount = records.Count;
                
                // Check expected result if specified
                if (!string.IsNullOrEmpty(check.ExpectedDnsResult))
                {
                    result.Success = records.Any(r => 
                        r.Contains(check.ExpectedDnsResult, StringComparison.OrdinalIgnoreCase));
                    
                    if (!result.Success)
                    {
                        result.ErrorMessage = $"Expected DNS result '{check.ExpectedDnsResult}' not found";
                        result.ErrorType = "DnsMismatch";
                    }
                }
                else
                {
                    result.Success = records.Count > 0;
                    if (!result.Success)
                    {
                        result.ErrorMessage = "No DNS records found";
                        result.ErrorType = "NoRecords";
                    }
                }
                
                result.Status = result.Success ? StatusType.Up : StatusType.Down;
            }
        }
        catch (DnsResponseException ex)
        {
            stopwatch.Stop();
            result.ResponseTimeMs = (int)stopwatch.ElapsedMilliseconds;
            result.Success = false;
            result.Status = StatusType.Down;
            result.ErrorMessage = ex.Message;
            result.ErrorType = "DnsResponseException";
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            result.ResponseTimeMs = (int)stopwatch.ElapsedMilliseconds;
            result.Success = false;
            result.Status = StatusType.Down;
            result.ErrorMessage = ex.Message;
            result.ErrorType = "UnknownError";
            
            _logger.LogError(ex, "Error executing DNS check for {Target}", check.Target);
        }
        
        return result;
    }
    
    #region Private Helper Methods
    
    private static CheckResult CreateErrorResult(Check check, string message, string errorType)
    {
        return new CheckResult
        {
            CheckId = check.Id,
            CheckedAt = DateTime.UtcNow,
            Success = false,
            Status = StatusType.Down,
            ErrorMessage = message,
            ErrorType = errorType
        };
    }
    
    private static string DetermineHttpErrorType(HttpRequestException ex)
    {
        var message = ex.Message.ToLowerInvariant();
        
        if (message.Contains("connection refused"))
            return "ConnectionRefused";
        if (message.Contains("host") && message.Contains("not"))
            return "HostNotFound";
        if (message.Contains("ssl") || message.Contains("certificate"))
            return "SslError";
        if (message.Contains("timeout"))
            return "Timeout";
        
        return "HttpRequestException";
    }
    
    private static (string host, int port) ParseTargetForTcp(string target, int? defaultPort)
    {
        if (target.Contains(':'))
        {
            var parts = target.Split(':');
            return (parts[0], int.Parse(parts[1]));
        }
        
        return (target, defaultPort ?? 80);
    }
    
    private static QueryType ParseDnsQueryType(string type)
    {
        return type.ToUpperInvariant() switch
        {
            "A" => QueryType.A,
            "AAAA" => QueryType.AAAA,
            "CNAME" => QueryType.CNAME,
            "MX" => QueryType.MX,
            "TXT" => QueryType.TXT,
            "NS" => QueryType.NS,
            "SOA" => QueryType.SOA,
            "PTR" => QueryType.PTR,
            _ => QueryType.A
        };
    }
    
    #endregion
}
CheckService Implementation
csharp// src/ERAMonitor.Infrastructure/Services/CheckService.cs

using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using ERAMonitor.API.Hubs;
using ERAMonitor.Core.DTOs.Common;
using ERAMonitor.Core.DTOs.Checks;
using ERAMonitor.Core.DTOs.Hosts;
using ERAMonitor.Core.Entities;
using ERAMonitor.Core.Enums;
using ERAMonitor.Core.Exceptions;
using ERAMonitor.Core.Interfaces.Repositories;
using ERAMonitor.Core.Interfaces.Services;

namespace ERAMonitor.Infrastructure.Services;

public class CheckService : ICheckService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICheckResultRepository _resultRepository;
    private readonly ICheckExecutorService _executorService;
    private readonly IIncidentService _incidentService;
    private readonly IHubContext<MonitoringHub> _hubContext;
    private readonly IAuditService _auditService;
    private readonly ILogger<CheckService> _logger;
    
    public CheckService(
        IUnitOfWork unitOfWork,
        ICheckResultRepository resultRepository,
        ICheckExecutorService executorService,
        IIncidentService incidentService,
        IHubContext<MonitoringHub> hubContext,
        IAuditService auditService,
        ILogger<CheckService> logger)
    {
        _unitOfWork = unitOfWork;
        _resultRepository = resultRepository;
        _executorService = executorService;
        _incidentService = incidentService;
        _hubContext = hubContext;
        _auditService = auditService;
        _logger = logger;
    }
    
    public async Task<PagedResponse<CheckListItemDto>> GetPagedAsync(
        Guid organizationId,
        PagedRequest request,
        CheckType? type = null,
        StatusType? status = null,
        Guid? customerId = null,
        Guid? hostId = null)
    {
        return await _unitOfWork.Checks.GetPagedAsync(
            organizationId,
            request,
            type,
            status,
            customerId,
            hostId
        );
    }
    
    public async Task<CheckDetailDto> GetByIdAsync(Guid id, Guid organizationId)
    {
        var check = await _unitOfWork.Checks.GetDetailAsync(id, organizationId);
        
        if (check == null)
        {
            throw new NotFoundException($"Check with ID {id} not found");
        }
        
        return check;
    }
    
    public async Task<CheckDetailDto> CreateAsync(Guid organizationId, CreateCheckRequest request)
    {
        // Validate customer if provided
        if (request.CustomerId.HasValue)
        {
            var customer = await _unitOfWork.Customers.GetByIdAsync(request.CustomerId.Value);
            if (customer == null || customer.OrganizationId != organizationId)
            {
                throw new BusinessException("Invalid customer ID", "INVALID_CUSTOMER");
            }
        }
        
        // Validate host if provided
        if (request.HostId.HasValue)
        {
            var host = await _unitOfWork.Hosts.GetByIdAsync(request.HostId.Value);
            if (host == null || host.OrganizationId != organizationId)
            {
                throw new BusinessException("Invalid host ID", "INVALID_HOST");
            }
        }
        
        var check = new Check
        {
            OrganizationId = organizationId,
            CustomerId = request.CustomerId,
            HostId = request.HostId,
            Name = request.Name,
            Description = request.Description,
            Type = request.Type,
            Target = request.Target,
            Tags = request.Tags ?? Array.Empty<string>(),
            HttpMethod = request.HttpMethod,
            HttpHeaders = request.HttpHeaders,
            HttpBody = request.HttpBody,
            ExpectedStatusCodes = request.ExpectedStatusCodes ?? new[] { 200, 201, 204, 301, 302 },
            ExpectedKeyword = request.ExpectedKeyword,
            KeywordShouldExist = request.KeywordShouldExist,
            CheckSsl = request.CheckSsl,
            SslExpiryWarningDays = request.SslExpiryWarningDays,
            SslExpiryCriticalDays = request.SslExpiryCriticalDays,
            TcpPort = request.TcpPort,
            DnsRecordType = request.DnsRecordType,
            ExpectedDnsResult = request.ExpectedDnsResult,
            IntervalSeconds = request.IntervalSeconds,
            TimeoutSeconds = request.TimeoutSeconds,
            RetryCount = request.RetryCount,
            RetryDelaySeconds = request.RetryDelaySeconds,
            FailureThreshold = request.FailureThreshold,
            MonitoringEnabled = request.MonitoringEnabled,
            AlertOnFailure = request.AlertOnFailure,
            AlertOnSslExpiry = request.AlertOnSslExpiry,
            AlertOnSlowResponse = request.AlertOnSlowResponse,
            SlowResponseThresholdMs = request.SlowResponseThresholdMs,
            CurrentStatus = StatusType.Unknown,
            NextCheckAt = DateTime.UtcNow // Schedule immediately
        };
        
        await _unitOfWork.Checks.AddAsync(check);
        await _unitOfWork.SaveChangesAsync();
        
        await _auditService.LogCreateAsync(check);
        
        _logger.LogInformation("Check created: {CheckName} ({CheckId})", check.Name, check.Id);
        
        return await GetByIdAsync(check.Id, organizationId);
    }
    
    public async Task<CheckDetailDto> UpdateAsync(Guid id, Guid organizationId, UpdateCheckRequest request)
    {
        var check = await _unitOfWork.Checks.GetByIdAsync(id);
        
        if (check == null || check.OrganizationId != organizationId)
        {
            throw new NotFoundException($"Check with ID {id} not found");
        }
        
        var oldValues = new
        {
            check.Name,
            check.Target,
            check.Type,
            check.MonitoringEnabled
        };
        
        // Update check properties
        check.Name = request.Name;
        check.Description = request.Description;
        check.Target = request.Target;
        check.Tags = request.Tags ?? Array.Empty<string>();
        check.CustomerId = request.CustomerId;
        check.HostId = request.HostId;
        check.HttpMethod = request.HttpMethod;
        check.HttpHeaders = request.HttpHeaders;
        check.HttpBody = request.HttpBody;
        check.ExpectedStatusCodes = request.ExpectedStatusCodes;
        check.ExpectedKeyword = request.ExpectedKeyword;
        check.KeywordShouldExist = request.KeywordShouldExist;
        check.CheckSsl = request.CheckSsl;
        check.SslExpiryWarningDays = request.SslExpiryWarningDays;
        check.SslExpiryCriticalDays = request.SslExpiryCriticalDays;
        check.TcpPort = request.TcpPort;
        check.DnsRecordType = request.DnsRecordType;
        check.ExpectedDnsResult = request.ExpectedDnsResult;
        check.IntervalSeconds = request.IntervalSeconds;
        check.TimeoutSeconds = request.TimeoutSeconds;
        check.RetryCount = request.RetryCount;
        check.RetryDelaySeconds = request.RetryDelaySeconds;
        check.FailureThreshold = request.FailureThreshold;
        check.MonitoringEnabled = request.MonitoringEnabled;
        check.AlertOnFailure = request.AlertOnFailure;
        check.AlertOnSslExpiry = request.AlertOnSslExpiry;
        check.AlertOnSlowResponse = request.AlertOnSlowResponse;
        check.SlowResponseThresholdMs = request.SlowResponseThresholdMs;
        check.IsActive = request.IsActive;
        
        _unitOfWork.Checks.Update(check);
        await _unitOfWork.SaveChangesAsync();
        
        await _auditService.LogUpdateAsync(check, oldValues);
        
        return await GetByIdAsync(id, organizationId);
    }
    
    public async Task DeleteAsync(Guid id, Guid organizationId)
    {
        var check = await _unitOfWork.Checks.GetByIdAsync(id);
        
        if (check == null || check.OrganizationId != organizationId)
        {
            throw new NotFoundException($"Check with ID {id} not found");
        }
        
        // Soft delete
        check.IsActive = false;
        check.MonitoringEnabled = false;
        
        _unitOfWork.Checks.Update(check);
        await _unitOfWork.SaveChangesAsync();
        
        await _auditService.LogDeleteAsync(check);
        
        _logger.LogInformation("Check deleted: {CheckName} ({CheckId})", check.Name, check.Id);
    }
    
    public async Task<bool> ToggleMonitoringAsync(Guid id, Guid organizationId)
    {
        var check = await _unitOfWork.Checks.GetByIdAsync(id);
        
        if (check == null || check.OrganizationId != organizationId)
        {
            throw new NotFoundException($"Check with ID {id} not found");
        }
        
        check.MonitoringEnabled = !check.MonitoringEnabled;
        
        if (check.MonitoringEnabled)
        {
            check.NextCheckAt = DateTime.UtcNow; // Schedule immediately
        }
        
        _unitOfWork.Checks.Update(check);
        await _unitOfWork.SaveChangesAsync();
        
        await _auditService.LogAsync(
            "ToggleMonitoring",
            "Check",
            check.Id,
            check.Name,
            newValues: new { check.MonitoringEnabled },
            organizationId: organizationId
        );
        
        return check.MonitoringEnabled;
    }
    
    public async Task<CheckDetailDto> SetMaintenanceAsync(Guid id, Guid organizationId, SetMaintenanceRequest request)
    {
        var check = await _unitOfWork.Checks.GetByIdAsync(id);
        
        if (check == null || check.OrganizationId != organizationId)
        {
            throw new NotFoundException($"Check with ID {id} not found");
        }
        
        check.MaintenanceMode = request.Enable;
        check.MaintenanceStartAt = request.Enable ? (request.StartAt ?? DateTime.UtcNow) : null;
        check.MaintenanceEndAt = request.Enable ? request.EndAt : null;
        
        _unitOfWork.Checks.Update(check);
        await _unitOfWork.SaveChangesAsync();
        
        await _auditService.LogAsync(
            request.Enable ? "MaintenanceEnabled" : "MaintenanceDisabled",
            "Check",
            check.Id,
            check.Name,
            organizationId: organizationId
        );
        
        return await GetByIdAsync(id, organizationId);
    }
    
    public async Task<CheckResultDto> RunCheckNowAsync(Guid id, Guid organizationId)
    {
        var check = await _unitOfWork.Checks.GetByIdAsync(id);
        
        if (check == null || check.OrganizationId != organizationId)
        {
            throw new NotFoundException($"Check with ID {id} not found");
        }
        
        _logger.LogInformation("Running manual check: {CheckName}", check.Name);
        
        // Execute check
        var result = await ExecuteCheckWithRetryAsync(check);
        
        // Process result
        await ProcessCheckResultAsync(check, result);
        
        return MapToCheckResultDto(result);
    }
    
    public async Task<CheckHistoryDto> GetHistoryAsync(Guid id, Guid organizationId, DateTime from, DateTime to)
    {
        var check = await _unitOfWork.Checks.GetByIdAsync(id);
        
        if (check == null || check.OrganizationId != organizationId)
        {
            throw new NotFoundException($"Check with ID {id} not found");
        }
        
        return await _resultRepository.GetHistoryAsync(id, from, to);
    }
    
    public async Task<List<CheckResultDto>> GetResultsAsync(Guid id, Guid organizationId, int limit = 100)
    {
        var check = await _unitOfWork.Checks.GetByIdAsync(id);
        
        if (check == null || check.OrganizationId != organizationId)
        {
            throw new NotFoundException($"Check with ID {id} not found");
        }
        
        var results = await _resultRepository.GetByCheckAsync(id, limit);
        return results.Select(MapToCheckResultDto).ToList();
    }
    
    public async Task<Dictionary<StatusType, int>> GetStatusCountsAsync(Guid organizationId)
    {
        return await _unitOfWork.Checks.GetStatusCountsAsync(organizationId);
    }
    
    public async Task<Dictionary<CheckType, int>> GetTypeCountsAsync(Guid organizationId)
    {
        return await _unitOfWork.Checks.GetCountsByTypeAsync(organizationId);
    }
    
    public async Task<List<CheckListItemDto>> GetExpiringCertificatesAsync(Guid organizationId, int daysUntilExpiry = 30)
    {
        var checks = await _unitOfWork.Checks.GetExpiringCertificatesAsync(daysUntilExpiry);
        
        return checks
            .Where(c => c.OrganizationId == organizationId)
            .Select(c => new CheckListItemDto
            {
                Id = c.Id,
                Name = c.Name,
                Type = c.Type,
                Target = c.Target,
                CurrentStatus = c.CurrentStatus,
                SslExpiresAt = c.SslExpiresAt,
                SslDaysUntilExpiry = c.SslExpiresAt.HasValue
                    ? (int)(c.SslExpiresAt.Value - DateTime.UtcNow).TotalDays
                    : null,
                MonitoringEnabled = c.MonitoringEnabled
            })
            .OrderBy(c => c.SslDaysUntilExpiry)
            .ToList();
    }
    
    #region Private Methods
    
    private async Task<CheckResult> ExecuteCheckWithRetryAsync(Check check)
    {
        CheckResult? lastResult = null;
        
        for (int attempt = 0; attempt <= check.RetryCount; attempt++)
        {
            lastResult = await _executorService.ExecuteCheckAsync(check);
            
            if (lastResult.Success)
            {
                return lastResult;
            }
            
            if (attempt < check.RetryCount)
            {
                _logger.LogDebug("Check {CheckName} failed, retrying in {Delay}s (attempt {Attempt}/{Max})",
                    check.Name, check.RetryDelaySeconds, attempt + 1, check.RetryCount);
                    
                await Task.Delay(TimeSpan.FromSeconds(check.RetryDelaySeconds));
            }
        }
        
        return lastResult!;
    }
    
    internal async Task ProcessCheckResultAsync(Check check, CheckResult result)
    {
        // Store result
        await _resultRepository.AddAsync(result);
        await _resultRepository.SaveChangesAsync();
        
        var previousStatus = check.CurrentStatus;
        var previousFailureCount = check.CurrentFailureCount;
        
        // Update check status
        check.LastCheckAt = DateTime.UtcNow;
        check.NextCheckAt = DateTime.UtcNow.AddSeconds(check.IntervalSeconds);
        check.LastResponseTimeMs = result.ResponseTimeMs;
        check.LastStatusCode = result.HttpStatusCode?.ToString();
        check.LastError = result.ErrorMessage;
        
        // Update SSL info
        if (result.SslExpiresAt.HasValue)
        {
            check.SslExpiresAt = result.SslExpiresAt;
            check.SslIssuer = result.SslIssuer;
        }
        
        // Handle failure threshold
        if (result.Success)
        {
            check.CurrentFailureCount = 0;
            check.CurrentStatus = StatusType.Up;
        }
        else
        {
            check.CurrentFailureCount++;
            
            if (check.CurrentFailureCount >= check.FailureThreshold)
            {
                check.CurrentStatus = StatusType.Down;
            }
        }
        
        // Track status change
        if (previousStatus != check.CurrentStatus)
        {
            check.PreviousStatus = previousStatus;
            check.StatusChangedAt = DateTime.UtcNow;
            
            _logger.LogInformation("Check {CheckName} status changed: {OldStatus} -> {NewStatus}",
                check.Name, previousStatus, check.CurrentStatus);
            
            // Handle status change events
            await HandleCheckStatusChangeAsync(check, previousStatus, check.CurrentStatus, result);
        }
        
        _unitOfWork.Checks.Update(check);
        await _unitOfWork.SaveChangesAsync();
        
        // Broadcast update via SignalR
        await BroadcastCheckUpdateAsync(check, result);
    }
    
    private async Task HandleCheckStatusChangeAsync(Check check, StatusType previousStatus, StatusType newStatus, CheckResult result)
    {
        // Check went DOWN
        if (previousStatus != StatusType.Down && newStatus == StatusType.Down && check.ShouldAlert())
        {
            await _incidentService.CreateAutoIncidentAsync(
                "Check",
                check.Id,
                check.Name,
                $"Check Failed: {check.Name}",
                $"Check '{check.Name}' ({check.Type}) failed. Target: {check.Target}. Error: {result.ErrorMessage}",
                IncidentSeverity.High,
                check.CustomerId,
                check.OrganizationId
            );
        }
        // Check recovered
        else if (previousStatus == StatusType.Down && newStatus == StatusType.Up)
        {
            await _incidentService.AutoResolveIncidentsAsync("Check", check.Id, "Check is now passing");
        }
    }
    
    private async Task BroadcastCheckUpdateAsync(Check check, CheckResult result)
    {
        var update = new
        {
            checkId = check.Id,
            checkName = check.Name,
            checkType = check.Type.ToString(),
            currentStatus = check.CurrentStatus.ToString(),
            responseTimeMs = result.ResponseTimeMs,
            lastCheckAt = check.LastCheckAt,
            success = result.Success,
            errorMessage = result.ErrorMessage
        };
        
        await _hubContext.BroadcastCheckResult(check.Id, check.CustomerId, update);
    }
    
    private static CheckResultDto MapToCheckResultDto(CheckResult result)
    {
        return new CheckResultDto
        {
            Id = result.Id,
            Success = result.Success,
            Status = result.Status,
            ResponseTimeMs = result.ResponseTimeMs,
            CheckedAt = result.CheckedAt,
            HttpStatusCode = result.HttpStatusCode,
            HttpStatusMessage = result.HttpStatusMessage,
            KeywordFound = result.KeywordFound,
            SslValid = result.SslValid,
            SslDaysUntilExpiry = result.SslDaysUntilExpiry,
            DnsResult = result.DnsResult,
            ErrorMessage = result.ErrorMessage,
            ErrorType = result.ErrorType,
            CheckedFrom = result.CheckedFrom
        };
    }
    
    #endregion
}
IncidentService Implementation
csharp// src/ERAMonitor.Infrastructure/Services/IncidentService.cs

using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using ERAMonitor.API.Hubs;
using ERAMonitor.Core.DTOs.Common;
using ERAMonitor.Core.DTOs.Incidents;
using ERAMonitor.Core.Entities;
using ERAMonitor.Core.Enums;
using ERAMonitor.Core.Exceptions;
using ERAMonitor.Core.Interfaces.Repositories;
using ERAMonitor.Core.Interfaces.Services;

namespace ERAMonitor.Infrastructure.Services;

public class IncidentService : IIncidentService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IIncidentTimelineRepository _timelineRepository;
    private readonly INotificationService _notificationService;
    private readonly IHubContext<MonitoringHub> _hubContext;
    private readonly IAuditService _auditService;
    private readonly ILogger<IncidentService> _logger;
    
    // SLA defaults (could be configurable per customer/organization)
    private const int DefaultSlaResponseMinutes = 30;
    private const int DefaultSlaResolutionHours = 4;
    
    public IncidentService(
        IUnitOfWork unitOfWork,
        IIncidentTimelineRepository timelineRepository,
        INotificationService notificationService,
        IHubContext<MonitoringHub> hubContext,
        IAuditService auditService,
        ILogger<IncidentService> logger)
    {
        _unitOfWork = unitOfWork;
        _timelineRepository = timelineRepository;
        _notificationService = notificationService;
        _hubContext = hubContext;
        _auditService = auditService;
        _logger = logger;
    }
    
    public async Task<PagedResponse<IncidentListItemDto>> GetPagedAsync(
        Guid organizationId,
        PagedRequest request,
        IncidentStatus? status = null,
        IncidentSeverity? severity = null,
        Guid? customerId = null,
        Guid? assignedToId = null,
        bool?RetryThis response paused because Claude reached its max length for a message. Hit continue to nudge Claude along.ContinueClaude can make mistakes. Please double-check responses.
        isOpen = null)
{
return await _unitOfWork.Incidents.GetPagedAsync(
organizationId,
request,
status,
severity,
customerId,
assignedToId,
isOpen
);
}
public async Task<IncidentDetailDto> GetByIdAsync(Guid id, Guid organizationId)
{
    var incident = await _unitOfWork.Incidents.GetDetailAsync(id, organizationId);
    
    if (incident == null)
    {
        throw new NotFoundException($"Incident with ID {id} not found");
    }
    
    return incident;
}

public async Task<IncidentDetailDto> CreateAsync(Guid organizationId, Guid userId, CreateIncidentRequest request)
{
    // Validate customer if provided
    if (request.CustomerId.HasValue)
    {
        var customer = await _unitOfWork.Customers.GetByIdAsync(request.CustomerId.Value);
        if (customer == null || customer.OrganizationId != organizationId)
        {
            throw new BusinessException("Invalid customer ID", "INVALID_CUSTOMER");
        }
    }
    
    // Get next incident number
    var incidentNumber = await _unitOfWork.Incidents.GetNextIncidentNumberAsync(organizationId);
    
    var incident = new Incident
    {
        OrganizationId = organizationId,
        CustomerId = request.CustomerId,
        IncidentNumber = incidentNumber,
        Title = request.Title,
        Description = request.Description,
        Severity = request.Severity,
        Status = IncidentStatus.New,
        Category = request.Category,
        IsAutoCreated = false,
        AssignedToId = request.AssignedToId,
        AssignedAt = request.AssignedToId.HasValue ? DateTime.UtcNow : null,
        Impact = request.Impact,
        AffectedUsersCount = request.AffectedUsersCount,
        FirstOccurredAt = DateTime.UtcNow,
        LastOccurredAt = DateTime.UtcNow,
        SlaResponseDue = DateTime.UtcNow.AddMinutes(GetSlaResponseMinutes(request.Severity)),
        SlaResolutionDue = DateTime.UtcNow.AddHours(GetSlaResolutionHours(request.Severity))
    };
    
    incident.CalculatePriorityScore();
    
    await _unitOfWork.Incidents.AddAsync(incident);
    
    // Add resources
    if (request.Resources?.Any() == true)
    {
        foreach (var resource in request.Resources)
        {
            var incidentResource = new IncidentResource
            {
                IncidentId = incident.Id,
                ResourceType = resource.ResourceType,
                ResourceId = resource.ResourceId,
                ResourceName = resource.ResourceName
            };
            await _unitOfWork.Context.Set<IncidentResource>().AddAsync(incidentResource);
        }
    }
    
    await _unitOfWork.SaveChangesAsync();
    
    // Add creation timeline entry
    await AddTimelineEntryAsync(incident.Id, userId, "Created", "Incident created", isSystem: true);
    
    if (request.AssignedToId.HasValue)
    {
        var assignee = await _unitOfWork.Users.GetByIdAsync(request.AssignedToId.Value);
        await AddTimelineEntryAsync(incident.Id, userId, "Assigned", 
            $"Assigned to {assignee?.FullName}", isSystem: true);
    }
    
    await _auditService.LogCreateAsync(incident, userId);
    
    _logger.LogInformation("Incident created: {IncidentId} - {Title}", incident.GetIncidentId(), incident.Title);
    
    // Send notifications
    await _notificationService.SendIncidentCreatedNotificationAsync(incident);
    
    // Broadcast via SignalR
    await _hubContext.BroadcastIncidentCreated(incident.CustomerId, new
    {
        incidentId = incident.Id,
        incidentNumber = incident.GetIncidentId(),
        title = incident.Title,
        severity = incident.Severity.ToString(),
        status = incident.Status.ToString()
    });
    
    return await GetByIdAsync(incident.Id, organizationId);
}

public async Task<IncidentDetailDto> UpdateAsync(Guid id, Guid organizationId, Guid userId, UpdateIncidentRequest request)
{
    var incident = await _unitOfWork.Incidents.GetByIdAsync(id);
    
    if (incident == null || incident.OrganizationId != organizationId)
    {
        throw new NotFoundException($"Incident with ID {id} not found");
    }
    
    var oldValues = new
    {
        incident.Title,
        incident.Description,
        incident.Severity,
        incident.Category,
        incident.AssignedToId
    };
    
    // Track severity change
    var severityChanged = incident.Severity != request.Severity;
    var oldSeverity = incident.Severity;
    
    // Track assignment change
    var assignmentChanged = incident.AssignedToId != request.AssignedToId;
    var oldAssignee = incident.AssignedToId;
    
    // Update fields
    incident.Title = request.Title;
    incident.Description = request.Description;
    incident.Severity = request.Severity;
    incident.Category = request.Category;
    incident.AssignedToId = request.AssignedToId;
    incident.Impact = request.Impact;
    incident.AffectedUsersCount = request.AffectedUsersCount;
    
    if (assignmentChanged && request.AssignedToId.HasValue)
    {
        incident.AssignedAt = DateTime.UtcNow;
    }
    
    incident.CalculatePriorityScore();
    
    _unitOfWork.Incidents.Update(incident);
    await _unitOfWork.SaveChangesAsync();
    
    // Add timeline entries for changes
    if (severityChanged)
    {
        await AddTimelineEntryAsync(incident.Id, userId, "SeverityChanged",
            $"Severity changed from {oldSeverity} to {request.Severity}",
            oldValue: oldSeverity.ToString(), newValue: request.Severity.ToString());
    }
    
    if (assignmentChanged)
    {
        var newAssignee = request.AssignedToId.HasValue
            ? await _unitOfWork.Users.GetByIdAsync(request.AssignedToId.Value)
            : null;
        await AddTimelineEntryAsync(incident.Id, userId, "Assigned",
            newAssignee != null ? $"Assigned to {newAssignee.FullName}" : "Unassigned");
    }
    
    await _auditService.LogUpdateAsync(incident, oldValues, userId);
    
    // Broadcast update
    await BroadcastIncidentUpdateAsync(incident);
    
    return await GetByIdAsync(id, organizationId);
}

public async Task<IncidentDetailDto> AcknowledgeAsync(Guid id, Guid organizationId, Guid userId, AcknowledgeIncidentRequest request)
{
    var incident = await _unitOfWork.Incidents.GetByIdAsync(id);
    
    if (incident == null || incident.OrganizationId != organizationId)
    {
        throw new NotFoundException($"Incident with ID {id} not found");
    }
    
    if (incident.Status != IncidentStatus.New)
    {
        throw new BusinessException("Only new incidents can be acknowledged", "INVALID_STATUS");
    }
    
    incident.Status = IncidentStatus.Acknowledged;
    incident.AcknowledgedById = userId;
    incident.AcknowledgedAt = DateTime.UtcNow;
    
    // Check SLA response breach
    if (incident.SlaResponseDue.HasValue && DateTime.UtcNow > incident.SlaResponseDue.Value)
    {
        incident.SlaResponseBreached = true;
    }
    
    _unitOfWork.Incidents.Update(incident);
    await _unitOfWork.SaveChangesAsync();
    
    var user = await _unitOfWork.Users.GetByIdAsync(userId);
    await AddTimelineEntryAsync(incident.Id, userId, "Acknowledged",
        $"Acknowledged by {user?.FullName}");
    
    if (!string.IsNullOrEmpty(request.Comment))
    {
        await AddTimelineEntryAsync(incident.Id, userId, "Comment", request.Comment);
    }
    
    await _auditService.LogAsync("Acknowledged", "Incident", incident.Id, incident.GetIncidentId(), userId: userId);
    await BroadcastIncidentUpdateAsync(incident);
    
    return await GetByIdAsync(id, organizationId);
}

public async Task<IncidentDetailDto> AssignAsync(Guid id, Guid organizationId, Guid userId, AssignIncidentRequest request)
{
    var incident = await _unitOfWork.Incidents.GetByIdAsync(id);
    
    if (incident == null || incident.OrganizationId != organizationId)
    {
        throw new NotFoundException($"Incident with ID {id} not found");
    }
    
    var assignee = await _unitOfWork.Users.GetByIdAsync(request.AssignToUserId);
    if (assignee == null || assignee.OrganizationId != organizationId)
    {
        throw new BusinessException("Invalid user ID", "INVALID_USER");
    }
    
    var oldAssignee = incident.AssignedToId;
    incident.AssignedToId = request.AssignToUserId;
    incident.AssignedAt = DateTime.UtcNow;
    
    _unitOfWork.Incidents.Update(incident);
    await _unitOfWork.SaveChangesAsync();
    
    await AddTimelineEntryAsync(incident.Id, userId, "Assigned",
        $"Assigned to {assignee.FullName}");
    
    if (!string.IsNullOrEmpty(request.Comment))
    {
        await AddTimelineEntryAsync(incident.Id, userId, "Comment", request.Comment);
    }
    
    // Notify assignee
    await _notificationService.SendIncidentAssignedNotificationAsync(incident, assignee);
    
    await _auditService.LogAsync("Assigned", "Incident", incident.Id, incident.GetIncidentId(),
        new { OldAssigneeId = oldAssignee }, new { NewAssigneeId = request.AssignToUserId }, userId);
    
    await BroadcastIncidentUpdateAsync(incident);
    
    return await GetByIdAsync(id, organizationId);
}

public async Task<IncidentDetailDto> UpdateStatusAsync(Guid id, Guid organizationId, Guid userId, UpdateStatusRequest request)
{
    var incident = await _unitOfWork.Incidents.GetByIdAsync(id);
    
    if (incident == null || incident.OrganizationId != organizationId)
    {
        throw new NotFoundException($"Incident with ID {id} not found");
    }
    
    var oldStatus = incident.Status;
    incident.Status = request.Status;
    
    _unitOfWork.Incidents.Update(incident);
    await _unitOfWork.SaveChangesAsync();
    
    await AddTimelineEntryAsync(incident.Id, userId, "StatusChanged",
        $"Status changed from {oldStatus} to {request.Status}",
        oldValue: oldStatus.ToString(), newValue: request.Status.ToString());
    
    if (!string.IsNullOrEmpty(request.Comment))
    {
        await AddTimelineEntryAsync(incident.Id, userId, "Comment", request.Comment);
    }
    
    await _auditService.LogAsync("StatusChanged", "Incident", incident.Id, incident.GetIncidentId(),
        new { OldStatus = oldStatus }, new { NewStatus = request.Status }, userId);
    
    await BroadcastIncidentUpdateAsync(incident);
    
    return await GetByIdAsync(id, organizationId);
}

public async Task<IncidentDetailDto> ResolveAsync(Guid id, Guid organizationId, Guid userId, ResolveIncidentRequest request)
{
    var incident = await _unitOfWork.Incidents.GetByIdAsync(id);
    
    if (incident == null || incident.OrganizationId != organizationId)
    {
        throw new NotFoundException($"Incident with ID {id} not found");
    }
    
    if (incident.Status == IncidentStatus.Resolved || incident.Status == IncidentStatus.Closed)
    {
        throw new BusinessException("Incident is already resolved or closed", "INVALID_STATUS");
    }
    
    var oldStatus = incident.Status;
    incident.Status = IncidentStatus.Resolved;
    incident.ResolvedById = userId;
    incident.ResolvedAt = DateTime.UtcNow;
    incident.ResolutionNotes = request.ResolutionNotes;
    incident.RootCause = request.RootCause;
    
    // Check SLA resolution breach
    if (incident.SlaResolutionDue.HasValue && DateTime.UtcNow > incident.SlaResolutionDue.Value)
    {
        incident.SlaResolutionBreached = true;
    }
    
    _unitOfWork.Incidents.Update(incident);
    await _unitOfWork.SaveChangesAsync();
    
    var user = await _unitOfWork.Users.GetByIdAsync(userId);
    await AddTimelineEntryAsync(incident.Id, userId, "Resolved",
        $"Resolved by {user?.FullName}. Resolution: {request.ResolutionNotes}");
    
    // Send resolution notification
    await _notificationService.SendIncidentResolvedNotificationAsync(incident);
    
    await _auditService.LogAsync("Resolved", "Incident", incident.Id, incident.GetIncidentId(), userId: userId);
    await BroadcastIncidentUpdateAsync(incident);
    
    _logger.LogInformation("Incident resolved: {IncidentId}", incident.GetIncidentId());
    
    return await GetByIdAsync(id, organizationId);
}

public async Task<IncidentDetailDto> CloseAsync(Guid id, Guid organizationId, Guid userId)
{
    var incident = await _unitOfWork.Incidents.GetByIdAsync(id);
    
    if (incident == null || incident.OrganizationId != organizationId)
    {
        throw new NotFoundException($"Incident with ID {id} not found");
    }
    
    if (incident.Status != IncidentStatus.Resolved)
    {
        throw new BusinessException("Only resolved incidents can be closed", "INVALID_STATUS");
    }
    
    incident.Status = IncidentStatus.Closed;
    incident.ClosedById = userId;
    incident.ClosedAt = DateTime.UtcNow;
    
    _unitOfWork.Incidents.Update(incident);
    await _unitOfWork.SaveChangesAsync();
    
    var user = await _unitOfWork.Users.GetByIdAsync(userId);
    await AddTimelineEntryAsync(incident.Id, userId, "Closed", $"Closed by {user?.FullName}");
    
    await _auditService.LogAsync("Closed", "Incident", incident.Id, incident.GetIncidentId(), userId: userId);
    await BroadcastIncidentUpdateAsync(incident);
    
    return await GetByIdAsync(id, organizationId);
}

public async Task<IncidentDetailDto> ReopenAsync(Guid id, Guid organizationId, Guid userId, string? reason = null)
{
    var incident = await _unitOfWork.Incidents.GetByIdAsync(id);
    
    if (incident == null || incident.OrganizationId != organizationId)
    {
        throw new NotFoundException($"Incident with ID {id} not found");
    }
    
    if (incident.Status != IncidentStatus.Resolved && incident.Status != IncidentStatus.Closed)
    {
        throw new BusinessException("Only resolved or closed incidents can be reopened", "INVALID_STATUS");
    }
    
    var oldStatus = incident.Status;
    incident.Status = IncidentStatus.InProgress;
    incident.ResolvedAt = null;
    incident.ResolvedById = null;
    incident.ClosedAt = null;
    incident.ClosedById = null;
    incident.OccurrenceCount++;
    incident.LastOccurredAt = DateTime.UtcNow;
    
    _unitOfWork.Incidents.Update(incident);
    await _unitOfWork.SaveChangesAsync();
    
    var user = await _unitOfWork.Users.GetByIdAsync(userId);
    await AddTimelineEntryAsync(incident.Id, userId, "Reopened",
        $"Reopened by {user?.FullName}. Reason: {reason ?? "Not specified"}");
    
    await _auditService.LogAsync("Reopened", "Incident", incident.Id, incident.GetIncidentId(), userId: userId);
    await BroadcastIncidentUpdateAsync(incident);
    
    return await GetByIdAsync(id, organizationId);
}

public async Task<IncidentDetailDto> EscalateAsync(Guid id, Guid organizationId, Guid userId, EscalateIncidentRequest request)
{
    var incident = await _unitOfWork.Incidents.GetByIdAsync(id);
    
    if (incident == null || incident.OrganizationId != organizationId)
    {
        throw new NotFoundException($"Incident with ID {id} not found");
    }
    
    var oldSeverity = incident.Severity;
    incident.Severity = request.NewSeverity;
    incident.CalculatePriorityScore();
    
    if (request.AssignToUserId.HasValue)
    {
        incident.AssignedToId = request.AssignToUserId;
        incident.AssignedAt = DateTime.UtcNow;
    }
    
    _unitOfWork.Incidents.Update(incident);
    await _unitOfWork.SaveChangesAsync();
    
    var user = await _unitOfWork.Users.GetByIdAsync(userId);
    await AddTimelineEntryAsync(incident.Id, userId, "Escalated",
        $"Escalated from {oldSeverity} to {request.NewSeverity} by {user?.FullName}. Reason: {request.Reason}",
        oldValue: oldSeverity.ToString(), newValue: request.NewSeverity.ToString());
    
    // Notify about escalation
    await _notificationService.SendIncidentEscalatedNotificationAsync(incident, request.Reason);
    
    await _auditService.LogAsync("Escalated", "Incident", incident.Id, incident.GetIncidentId(), userId: userId);
    await BroadcastIncidentUpdateAsync(incident);
    
    return await GetByIdAsync(id, organizationId);
}

public async Task<IncidentTimelineDto> AddCommentAsync(Guid id, Guid organizationId, Guid userId, AddCommentRequest request)
{
    var incident = await _unitOfWork.Incidents.GetByIdAsync(id);
    
    if (incident == null || incident.OrganizationId != organizationId)
    {
        throw new NotFoundException($"Incident with ID {id} not found");
    }
    
    var timeline = await AddTimelineEntryAsync(incident.Id, userId, "Comment", request.Content,
        isInternal: request.IsInternal);
    
    return await GetTimelineEntryAsync(timeline.Id);
}

public async Task<List<IncidentTimelineDto>> GetTimelineAsync(Guid id, Guid organizationId, bool includeInternal = true)
{
    var incident = await _unitOfWork.Incidents.GetByIdAsync(id);
    
    if (incident == null || incident.OrganizationId != organizationId)
    {
        throw new NotFoundException($"Incident with ID {id} not found");
    }
    
    return await _timelineRepository.GetByIncidentAsync(id, includeInternal);
}

public async Task<IncidentResourceDto> LinkResourceAsync(Guid id, Guid organizationId, LinkResourceRequest request)
{
    var incident = await _unitOfWork.Incidents.GetByIdAsync(id);
    
    if (incident == null || incident.OrganizationId != organizationId)
    {
        throw new NotFoundException($"Incident with ID {id} not found");
    }
    
    var resource = new IncidentResource
    {
        IncidentId = id,
        ResourceType = request.ResourceType,
        ResourceId = request.ResourceId,
        ResourceName = request.ResourceName
    };
    
    await _unitOfWork.Context.Set<IncidentResource>().AddAsync(resource);
    await _unitOfWork.SaveChangesAsync();
    
    return new IncidentResourceDto
    {
        Id = resource.Id,
        ResourceType = resource.ResourceType,
        ResourceId = resource.ResourceId,
        ResourceName = resource.ResourceName
    };
}

public async Task UnlinkResourceAsync(Guid id, Guid organizationId, Guid resourceId)
{
    var incident = await _unitOfWork.Incidents.GetByIdAsync(id);
    
    if (incident == null || incident.OrganizationId != organizationId)
    {
        throw new NotFoundException($"Incident with ID {id} not found");
    }
    
    var resource = await _unitOfWork.Context.Set<IncidentResource>()
        .FindAsync(resourceId);
    
    if (resource != null && resource.IncidentId == id)
    {
        _unitOfWork.Context.Set<IncidentResource>().Remove(resource);
        await _unitOfWork.SaveChangesAsync();
    }
}

public async Task<Guid> CreateAutoIncidentAsync(
    string sourceType,
    Guid sourceId,
    string sourceName,
    string title,
    string description,
    IncidentSeverity severity,
    Guid? customerId,
    Guid organizationId)
{
    // Check for existing open incident from same source
    var existingIncident = await _unitOfWork.Incidents.GetOpenBySourceAsync(organizationId, sourceType, sourceId);
    
    if (existingIncident != null)
    {
        // Update existing incident
        existingIncident.OccurrenceCount++;
        existingIncident.LastOccurredAt = DateTime.UtcNow;
        
        _unitOfWork.Incidents.Update(existingIncident);
        await _unitOfWork.SaveChangesAsync();
        
        await AddTimelineEntryAsync(existingIncident.Id, null, "Reoccurred",
            "Issue reoccurred", isSystem: true);
        
        _logger.LogInformation("Auto-incident updated (reoccurrence): {IncidentId}", existingIncident.GetIncidentId());
        
        return existingIncident.Id;
    }
    
    // Create new incident
    var incidentNumber = await _unitOfWork.Incidents.GetNextIncidentNumberAsync(organizationId);
    
    var incident = new Incident
    {
        OrganizationId = organizationId,
        CustomerId = customerId,
        IncidentNumber = incidentNumber,
        Title = title,
        Description = description,
        Severity = severity,
        Status = IncidentStatus.New,
        IsAutoCreated = true,
        SourceType = sourceType,
        SourceId = sourceId,
        SourceName = sourceName,
        FirstOccurredAt = DateTime.UtcNow,
        LastOccurredAt = DateTime.UtcNow,
        SlaResponseDue = DateTime.UtcNow.AddMinutes(GetSlaResponseMinutes(severity)),
        SlaResolutionDue = DateTime.UtcNow.AddHours(GetSlaResolutionHours(severity))
    };
    
    incident.CalculatePriorityScore();
    
    await _unitOfWork.Incidents.AddAsync(incident);
    
    // Add source as resource
    var resource = new IncidentResource
    {
        IncidentId = incident.Id,
        ResourceType = sourceType,
        ResourceId = sourceId,
        ResourceName = sourceName
    };
    await _unitOfWork.Context.Set<IncidentResource>().AddAsync(resource);
    
    await _unitOfWork.SaveChangesAsync();
    
    await AddTimelineEntryAsync(incident.Id, null, "Created",
        $"Auto-created from {sourceType}: {sourceName}", isSystem: true);
    
    _logger.LogInformation("Auto-incident created: {IncidentId} from {SourceType}:{SourceName}",
        incident.GetIncidentId(), sourceType, sourceName);
    
    // Send notification
    await _notificationService.SendIncidentCreatedNotificationAsync(incident);
    
    // Broadcast
    await _hubContext.BroadcastIncidentCreated(incident.CustomerId, new
    {
        incidentId = incident.Id,
        incidentNumber = incident.GetIncidentId(),
        title = incident.Title,
        severity = incident.Severity.ToString(),
        status = incident.Status.ToString(),
        isAutoCreated = true
    });
    
    return incident.Id;
}

public async Task AutoResolveIncidentsAsync(string sourceType, Guid sourceId, string resolutionNote)
{
    var openIncidents = await _unitOfWork.Incidents.GetOpenBySourceAsync(sourceType, sourceId);
    
    foreach (var incident in openIncidents)
    {
        incident.Status = IncidentStatus.Resolved;
        incident.ResolvedAt = DateTime.UtcNow;
        incident.ResolutionNotes = resolutionNote;
        
        if (incident.SlaResolutionDue.HasValue && DateTime.UtcNow > incident.SlaResolutionDue.Value)
        {
            incident.SlaResolutionBreached = true;
        }
        
        _unitOfWork.Incidents.Update(incident);
        
        await AddTimelineEntryAsync(incident.Id, null, "Resolved",
            $"Auto-resolved: {resolutionNote}", isSystem: true);
        
        _logger.LogInformation("Auto-incident resolved: {IncidentId}", incident.GetIncidentId());
        
        await _notificationService.SendIncidentResolvedNotificationAsync(incident);
    }
    
    await _unitOfWork.SaveChangesAsync();
}

public async Task<Dictionary<IncidentStatus, int>> GetStatusCountsAsync(Guid organizationId)
{
    return await _unitOfWork.Incidents.GetStatusCountsAsync(organizationId);
}

public async Task<Dictionary<IncidentSeverity, int>> GetSeverityCountsAsync(Guid organizationId)
{
    return await _unitOfWork.Incidents.GetSeverityCountsAsync(organizationId, onlyOpen: true);
}

public async Task<IncidentStatisticsDto> GetStatisticsAsync(Guid organizationId, DateTime from, DateTime to)
{
    return await _unitOfWork.Incidents.GetStatisticsAsync(organizationId, from, to);
}

#region Private Methods

private async Task<IncidentTimeline> AddTimelineEntryAsync(
    Guid incidentId,
    Guid? userId,
    string eventType,
    string? content,
    string? title = null,
    string? oldValue = null,
    string? newValue = null,
    bool isInternal = false,
    bool isSystem = false)
{
    var timeline = new IncidentTimeline
    {
        IncidentId = incidentId,
        UserId = userId,
        EventType = eventType,
        Title = title,
        Content = content,
        OldValue = oldValue,
        NewValue = newValue,
        IsInternal = isInternal,
        IsSystemGenerated = isSystem || !userId.HasValue
    };
    
    await _timelineRepository.AddAsync(timeline);
    await _unitOfWork.SaveChangesAsync();
    
    return timeline;
}

private async Task<IncidentTimelineDto> GetTimelineEntryAsync(Guid timelineId)
{
    var timeline = await _timelineRepository.GetByIdAsync(timelineId, t => t.User);
    
    return new IncidentTimelineDto
    {
        Id = timeline!.Id,
        EventType = timeline.EventType,
        Title = timeline.Title,
        Content = timeline.Content,
        OldValue = timeline.OldValue,
        NewValue = timeline.NewValue,
        IsInternal = timeline.IsInternal,
        IsSystemGenerated = timeline.IsSystemGenerated,
        User = timeline.User != null ? new UserSummaryDto
        {
            Id = timeline.User.Id,
            FullName = timeline.User.FullName,
            AvatarUrl = timeline.User.AvatarUrl
        } : null,
        CreatedAt = timeline.CreatedAt
    };
}

private async Task BroadcastIncidentUpdateAsync(Incident incident)
{
    await _hubContext.BroadcastIncidentUpdated(incident.Id, incident.CustomerId, new
    {
        incidentId = incident.Id,
        incidentNumber = incident.GetIncidentId(),
        title = incident.Title,
        severity = incident.Severity.ToString(),
        status = incident.Status.ToString(),
        updatedAt = incident.UpdatedAt
    });
}

private static int GetSlaResponseMinutes(IncidentSeverity severity)
{
    return severity switch
    {
        IncidentSeverity.Critical => 15,
        IncidentSeverity.High => 30,
        IncidentSeverity.Medium => 60,
        IncidentSeverity.Low => 240,
        IncidentSeverity.Info => 480,
        _ => DefaultSlaResponseMinutes
    };
}

private static int GetSlaResolutionHours(IncidentSeverity severity)
{
    return severity switch
    {
        IncidentSeverity.Critical => 1,
        IncidentSeverity.High => 4,
        IncidentSeverity.Medium => 8,
        IncidentSeverity.Low => 24,
        IncidentSeverity.Info => 72,
        _ => DefaultSlaResolutionHours
    };
}

#endregion
}

---

## 4.8 Background Jobs

### CheckSchedulerJob
```csharp
// src/ERAMonitor.BackgroundJobs/Jobs/CheckSchedulerJob.cs

using Hangfire;
using Microsoft.Extensions.Logging;
using ERAMonitor.Core.Entities;
using ERAMonitor.Core.Interfaces.Repositories;
using ERAMonitor.Core.Interfaces.Services;

namespace ERAMonitor.BackgroundJobs.Jobs;

public class CheckSchedulerJob
{
    private readonly ICheckRepository _checkRepository;
    private readonly ICheckService _checkService;
    private readonly ILogger<CheckSchedulerJob> _logger;
    
    public CheckSchedulerJob(
        ICheckRepository checkRepository,
        ICheckService checkService,
        ILogger<CheckSchedulerJob> logger)
    {
        _checkRepository = checkRepository;
        _checkService = checkService;
        _logger = logger;
    }
    
    [DisableConcurrentExecution(timeoutInSeconds: 120)]
    [AutomaticRetry(Attempts = 0)]
    public async Task ExecuteAsync()
    {
        _logger.LogDebug("Running check scheduler...");
        
        try
        {
            // Get checks that are due
            var dueChecks = await _checkRepository.GetDueChecksAsync(DateTime.UtcNow);
            
            _logger.LogDebug("Found {Count} checks due for execution", dueChecks.Count);
            
            foreach (var check in dueChecks)
            {
                // Queue individual check execution
                BackgroundJob.Enqueue<CheckExecutionJob>(job => job.ExecuteCheckAsync(check.Id));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in check scheduler");
            throw;
        }
    }
}
```

### CheckExecutionJob
```csharp
// src/ERAMonitor.BackgroundJobs/Jobs/CheckExecutionJob.cs

using Hangfire;
using Microsoft.Extensions.Logging;
using ERAMonitor.Core.Interfaces.Repositories;
using ERAMonitor.Core.Interfaces.Services;
using ERAMonitor.Infrastructure.Services;

namespace ERAMonitor.BackgroundJobs.Jobs;

public class CheckExecutionJob
{
    private readonly ICheckRepository _checkRepository;
    private readonly ICheckExecutorService _executorService;
    private readonly ICheckResultRepository _resultRepository;
    private readonly CheckService _checkService; // Use concrete type for internal method access
    private readonly ILogger<CheckExecutionJob> _logger;
    
    public CheckExecutionJob(
        ICheckRepository checkRepository,
        ICheckExecutorService executorService,
        ICheckResultRepository resultRepository,
        CheckService checkService,
        ILogger<CheckExecutionJob> logger)
    {
        _checkRepository = checkRepository;
        _executorService = executorService;
        _resultRepository = resultRepository;
        _checkService = checkService;
        _logger = logger;
    }
    
    [AutomaticRetry(Attempts = 1)]
    public async Task ExecuteCheckAsync(Guid checkId)
    {
        var check = await _checkRepository.GetByIdAsync(checkId);
        
        if (check == null)
        {
            _logger.LogWarning("Check {CheckId} not found, skipping", checkId);
            return;
        }
        
        if (!check.MonitoringEnabled || !check.IsActive || check.IsInMaintenance())
        {
            _logger.LogDebug("Check {CheckName} is disabled/inactive/maintenance, skipping", check.Name);
            return;
        }
        
        _logger.LogDebug("Executing check: {CheckName} ({CheckType})", check.Name, check.Type);
        
        try
        {
            // Execute with retry
            var result = await ExecuteWithRetryAsync(check);
            
            // Process result
            await _checkService.ProcessCheckResultAsync(check, result);
            
            _logger.LogDebug("Check {CheckName} completed: {Status} ({ResponseTime}ms)",
                check.Name, result.Status, result.ResponseTimeMs);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing check {CheckName}", check.Name);
        }
    }
    
    private async Task<Core.Entities.CheckResult> ExecuteWithRetryAsync(Core.Entities.Check check)
    {
        Core.Entities.CheckResult? lastResult = null;
        
        for (int attempt = 0; attempt <= check.RetryCount; attempt++)
        {
            lastResult = await _executorService.ExecuteCheckAsync(check);
            
            if (lastResult.Success)
            {
                return lastResult;
            }
            
            if (attempt < check.RetryCount)
            {
                _logger.LogDebug("Check {CheckName} failed attempt {Attempt}, retrying...",
                    check.Name, attempt + 1);
                await Task.Delay(TimeSpan.FromSeconds(check.RetryDelaySeconds));
            }
        }
        
        return lastResult!;
    }
}
```

### SlaMonitorJob
```csharp
// src/ERAMonitor.BackgroundJobs/Jobs/SlaMonitorJob.cs

using Hangfire;
using Microsoft.Extensions.Logging;
using ERAMonitor.Core.Enums;
using ERAMonitor.Core.Interfaces.Repositories;
using ERAMonitor.Core.Interfaces.Services;

namespace ERAMonitor.BackgroundJobs.Jobs;

public class SlaMonitorJob
{
    private readonly IIncidentRepository _incidentRepository;
    private readonly INotificationService _notificationService;
    private readonly ILogger<SlaMonitorJob> _logger;
    
    public SlaMonitorJob(
        IIncidentRepository incidentRepository,
        INotificationService notificationService,
        ILogger<SlaMonitorJob> logger)
    {
        _incidentRepository = incidentRepository;
        _notificationService = notificationService;
        _logger = logger;
    }
    
    [DisableConcurrentExecution(timeoutInSeconds: 60)]
    [AutomaticRetry(Attempts = 1)]
    public async Task ExecuteAsync()
    {
        _logger.LogDebug("Running SLA monitor...");
        
        try
        {
            var now = DateTime.UtcNow;
            var breachingIncidents = await _incidentRepository.GetSlaBreachingAsync(now);
            
            foreach (var incident in breachingIncidents)
            {
                bool updated = false;
                
                // Check response SLA breach
                if (!incident.SlaResponseBreached &&
                    incident.SlaResponseDue.HasValue &&
                    now > incident.SlaResponseDue.Value &&
                    incident.Status == IncidentStatus.New)
                {
                    incident.SlaResponseBreached = true;
                    updated = true;
                    
                    _logger.LogWarning("Incident {IncidentId} breached response SLA", incident.GetIncidentId());
                    
                    await _notificationService.SendSlaBreachNotificationAsync(incident, "Response");
                }
                
                // Check resolution SLA breach
                if (!incident.SlaResolutionBreached &&
                    incident.SlaResolutionDue.HasValue &&
                    now > incident.SlaResolutionDue.Value &&
                    incident.Status != IncidentStatus.Resolved &&
                    incident.Status != IncidentStatus.Closed)
                {
                    incident.SlaResolutionBreached = true;
                    updated = true;
                    
                    _logger.LogWarning("Incident {IncidentId} breached resolution SLA", incident.GetIncidentId());
                    
                    await _notificationService.SendSlaBreachNotificationAsync(incident, "Resolution");
                }
                
                if (updated)
                {
                    _incidentRepository.Update(incident);
                }
            }
            
            await _incidentRepository.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in SLA monitor");
            throw;
        }
    }
}
```

### CertificateExpiryCheckerJob
```csharp
// src/ERAMonitor.BackgroundJobs/Jobs/CertificateExpiryCheckerJob.cs

using Hangfire;
using Microsoft.Extensions.Logging;
using ERAMonitor.Core.Enums;
using ERAMonitor.Core.Interfaces.Repositories;
using ERAMonitor.Core.Interfaces.Services;

namespace ERAMonitor.BackgroundJobs.Jobs;

public class CertificateExpiryCheckerJob
{
    private readonly ICheckRepository _checkRepository;
    private readonly IIncidentService _incidentService;
    private readonly INotificationService _notificationService;
    private readonly ILogger<CertificateExpiryCheckerJob> _logger;
    
    public CertificateExpiryCheckerJob(
        ICheckRepository checkRepository,
        IIncidentService incidentService,
        INotificationService notificationService,
        ILogger<CertificateExpiryCheckerJob> logger)
    {
        _checkRepository = checkRepository;
        _incidentService = incidentService;
        _notificationService = notificationService;
        _logger = logger;
    }
    
    [DisableConcurrentExecution(timeoutInSeconds: 120)]
    [AutomaticRetry(Attempts = 1)]
    public async Task ExecuteAsync()
    {
        _logger.LogDebug("Running certificate expiry checker...");
        
        try
        {
            // Get checks with certificates expiring in the next 30 days
            var expiringChecks = await _checkRepository.GetExpiringCertificatesAsync(30);
            
            foreach (var check in expiringChecks)
            {
                if (!check.SslExpiresAt.HasValue || !check.AlertOnSslExpiry)
                {
                    continue;
                }
                
                var daysUntilExpiry = (check.SslExpiresAt.Value - DateTime.UtcNow).TotalDays;
                
                // Determine severity based on days until expiry
                IncidentSeverity severity;
                if (daysUntilExpiry <= 0)
                {
                    severity = IncidentSeverity.Critical;
                }
                else if (daysUntilExpiry <= check.SslExpiryCriticalDays)
                {
                    severity = IncidentSeverity.High;
                }
                else if (daysUntilExpiry <= check.SslExpiryWarningDays)
                {
                    severity = IncidentSeverity.Medium;
                }
                else
                {
                    continue; // Not yet in warning range
                }
                
                var title = daysUntilExpiry <= 0
                    ? $"SSL Certificate EXPIRED: {check.Name}"
                    : $"SSL Certificate Expiring: {check.Name}";
                
                var description = $"SSL certificate for {check.Target} " +
                    (daysUntilExpiry <= 0
                        ? $"has EXPIRED on {check.SslExpiresAt.Value:yyyy-MM-dd}"
                        : $"will expire in {(int)daysUntilExpiry} days ({check.SslExpiresAt.Value:yyyy-MM-dd})");
                
                await _incidentService.CreateAutoIncidentAsync(
                    "Check",
                    check.Id,
                    check.Name,
                    title,
                    description,
                    severity,
                    check.CustomerId,
                    check.OrganizationId
                );
                
                _logger.LogWarning("SSL certificate expiry alert for {CheckName}: {DaysUntilExpiry} days",
                    check.Name, (int)daysUntilExpiry);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in certificate expiry checker");
            throw;
        }
    }
}
```

---

## 4.9 Controllers

### ChecksController
```csharp
// src/ERAMonitor.API/Controllers/ChecksController.cs

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ERAMonitor.Core.DTOs.Common;
using ERAMonitor.Core.DTOs.Checks;
using ERAMonitor.Core.DTOs.Hosts;
using ERAMonitor.Core.Enums;
using ERAMonitor.Core.Interfaces.Services;
using ERAMonitor.API.Extensions;

namespace ERAMonitor.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ChecksController : ControllerBase
{
    private readonly ICheckService _checkService;
    private readonly ILogger<ChecksController> _logger;
    
    public ChecksController(ICheckService checkService, ILogger<ChecksController> logger)
    {
        _checkService = checkService;
        _logger = logger;
    }
    
    /// <summary>
    /// Get paginated list of checks
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<CheckListItemDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResponse<CheckListItemDto>>> GetChecks(
        [FromQuery] PagedRequest request,
        [FromQuery] CheckType? type = null,
        [FromQuery] StatusType? status = null,
        [FromQuery] Guid? customerId = null,
        [FromQuery] Guid? hostId = null)
    {
        var organizationId = User.GetOrganizationId();
        var result = await _checkService.GetPagedAsync(
            organizationId, request, type, status, customerId, hostId);
        return Ok(result);
    }
    
    /// <summary>
    /// Get check by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(CheckDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CheckDetailDto>> GetCheck(Guid id)
    {
        var organizationId = User.GetOrganizationId();
        var check = await _checkService.GetByIdAsync(id, organizationId);
        return Ok(check);
    }
    
    /// <summary>
    /// Create new check
    /// </summary>
    [HttpPost]
    [Authorize(Policy = "RequireOperatorRole")]
    [ProducesResponseType(typeof(CheckDetailDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CheckDetailDto>> CreateCheck([FromBody] CreateCheckRequest request)
    {
        var organizationId = User.GetOrganizationId();
        var check = await _checkService.CreateAsync(organizationId, request);
        return CreatedAtAction(nameof(GetCheck), new { id = check.Id }, check);
    }
    
    /// <summary>
    /// Update check
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Policy = "RequireOperatorRole")]
    [ProducesResponseType(typeof(CheckDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CheckDetailDto>> UpdateCheck(Guid id, [FromBody] UpdateCheckRequest request)
    {
        var organizationId = User.GetOrganizationId();
        var check = await _checkService.UpdateAsync(id, organizationId, request);
        return Ok(check);
    }
    
    /// <summary>
    /// Delete check
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Policy = "RequireAdminRole")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse>> DeleteCheck(Guid id)
    {
        var organizationId = User.GetOrganizationId();
        await _checkService.DeleteAsync(id, organizationId);
        return Ok(ApiResponse.Ok("Check deleted successfully"));
    }
    
    /// <summary>
    /// Toggle check monitoring
    /// </summary>
    [HttpPost("{id}/toggle-monitoring")]
    [Authorize(Policy = "RequireOperatorRole")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<bool>>> ToggleMonitoring(Guid id)
    {
        var organizationId = User.GetOrganizationId();
        var isEnabled = await _checkService.ToggleMonitoringAsync(id, organizationId);
        return Ok(ApiResponse<bool>.Ok(isEnabled, isEnabled ? "Monitoring enabled" : "Monitoring disabled"));
    }
    
    /// <summary>
    /// Set maintenance mode
    /// </summary>
    [HttpPost("{id}/maintenance")]
    [Authorize(Policy = "RequireOperatorRole")]
    [ProducesResponseType(typeof(CheckDetailDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<CheckDetailDto>> SetMaintenance(Guid id, [FromBody] SetMaintenanceRequest request)
    {
        var organizationId = User.GetOrganizationId();
        var check = await _checkService.SetMaintenanceAsync(id, organizationId, request);
        return Ok(check);
    }
    
    /// <summary>
    /// Run check immediately
    /// </summary>
    [HttpPost("{id}/run")]
    [Authorize(Policy = "RequireOperatorRole")]
    [ProducesResponseType(typeof(CheckResultDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<CheckResultDto>> RunCheck(Guid id)
    {
        var organizationId = User.GetOrganizationId();
        var result = await _checkService.RunCheckNowAsync(id, organizationId);
        return Ok(result);
    }
    
    /// <summary>
    /// Get check history
    /// </summary>
    [HttpGet("{id}/history")]
    [ProducesResponseType(typeof(CheckHistoryDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<CheckHistoryDto>> GetHistory(
        Guid id,
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null)
    {
        var organizationId = User.GetOrganizationId();
        var fromDate = from ?? DateTime.UtcNow.AddHours(-24);
        var toDate = to ?? DateTime.UtcNow;
        
        var history = await _checkService.GetHistoryAsync(id, organizationId, fromDate, toDate);
        return Ok(history);
    }
    
    /// <summary>
    /// Get check results
    /// </summary>
    [HttpGet("{id}/results")]
    [ProducesResponseType(typeof(List<CheckResultDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<CheckResultDto>>> GetResults(Guid id, [FromQuery] int limit = 100)
    {
        var organizationId = User.GetOrganizationId();
        var results = await _checkService.GetResultsAsync(id, organizationId, limit);
        return Ok(results);
    }
    
    /// <summary>
    /// Get check status counts
    /// </summary>
    [HttpGet("status-counts")]
    [ProducesResponseType(typeof(Dictionary<string, int>), StatusCodes.Status200OK)]
    public async Task<ActionResult<Dictionary<string, int>>> GetStatusCounts()
    {
        var organizationId = User.GetOrganizationId();
        var counts = await _checkService.GetStatusCountsAsync(organizationId);
        return Ok(counts.ToDictionary(k => k.Key.ToString(), v => v.Value));
    }
    
    /// <summary>
    /// Get check type counts
    /// </summary>
    [HttpGet("type-counts")]
    [ProducesResponseType(typeof(Dictionary<string, int>), StatusCodes.Status200OK)]
    public async Task<ActionResult<Dictionary<string, int>>> GetTypeCounts()
    {
        var organizationId = User.GetOrganizationId();
        var counts = await _checkService.GetTypeCountsAsync(organizationId);
        return Ok(counts.ToDictionary(k => k.Key.ToString(), v => v.Value));
    }
    
    /// <summary>
    /// Get checks with expiring SSL certificates
    /// </summary>
    [HttpGet("expiring-certificates")]
    [ProducesResponseType(typeof(List<CheckListItemDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<CheckListItemDto>>> GetExpiringCertificates([FromQuery] int days = 30)
    {
        var organizationId = User.GetOrganizationId();
        var checks = await _checkService.GetExpiringCertificatesAsync(organizationId, days);
        return Ok(checks);
    }
}
```

### IncidentsController
```csharp
// src/ERAMonitor.API/Controllers/IncidentsController.cs

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ERAMonitor.Core.DTOs.Common;
using ERAMonitor.Core.DTOs.Incidents;
using ERAMonitor.Core.Enums;
using ERAMonitor.Core.Interfaces.Services;
using ERAMonitor.API.Extensions;

namespace ERAMonitor.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class IncidentsController : ControllerBase
{
    private readonly IIncidentService _incidentService;
    private readonly ILogger<IncidentsController> _logger;
    
    public IncidentsController(IIncidentService incidentService, ILogger<IncidentsController> logger)
    {
        _incidentService = incidentService;
        _logger = logger;
    }
    
    /// <summary>
    /// Get paginated list of incidents
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<IncidentListItemDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResponse<IncidentListItemDto>>> GetIncidents(
        [FromQuery] PagedRequest request,
        [FromQuery] IncidentStatus? status = null,
        [FromQuery] IncidentSeverity? severity = null,
        [FromQuery] Guid? customerId = null,
        [FromQuery] Guid? assignedToId = null,
        [FromQuery] bool? isOpen = null)
    {
        var organizationId = User.GetOrganizationId();
        var result = await _incidentService.GetPagedAsync(
            organizationId, request, status, severity, customerId, assignedToId, isOpen);
        return Ok(result);
    }
    
    /// <summary>
    /// Get incident by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(IncidentDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IncidentDetailDto>> GetIncident(Guid id)
    {
        var organizationId = User.GetOrganizationId();
        var incident = await _incidentService.GetByIdAsync(id, organizationId);
        return Ok(incident);
    }
    
    /// <summary>
    /// Create new incident
    /// </summary>
    [HttpPost]
    [Authorize(Policy = "RequireOperatorRole")]
    [ProducesResponseType(typeof(IncidentDetailDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IncidentDetailDto>> CreateIncident([FromBody] CreateIncidentRequest request)
    {
        var organizationId = User.GetOrganizationId();
        var userId = User.GetUserId();
        var incident = await _incidentService.CreateAsync(organizationId, userId, request);
        return CreatedAtAction(nameof(GetIncident), new { id = incident.Id }, incident);
    }
    
    /// <summary>
    /// Update incident
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Policy = "RequireOperatorRole")]
    [ProducesResponseType(typeof(IncidentDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IncidentDetailDto>> UpdateIncident(Guid id, [FromBody] UpdateIncidentRequest request)
    {
        var organizationId = User.GetOrganizationId();
        var userId = User.GetUserId();
        var incident = await _incidentService.UpdateAsync(id, organizationId, userId, request);
        return Ok(incident);
    }
    
    /// <summary>
    /// Acknowledge incident
    /// </summary>
    [HttpPost("{id}/acknowledge")]
    [Authorize(Policy = "RequireOperatorRole")]
    [ProducesResponseType(typeof(IncidentDetailDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<IncidentDetailDto>> Acknowledge(Guid id, [FromBody] AcknowledgeIncidentRequest request)
    {
        var organizationId = User.GetOrganizationId();
        var userId = User.GetUserId();
        var incident = await _incidentService.AcknowledgeAsync(id, organizationId, userId, request);
        return Ok(incident);
    }
    
    /// <summary>
    /// Assign incident to user
    /// </summary>
    [HttpPost("{id}/assign")]
    [Authorize(Policy = "RequireOperatorRole")]
    [ProducesResponseType(typeof(IncidentDetailDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<IncidentDetailDto>> Assign(Guid id, [FromBody] AssignIncidentRequest request)
    {
        var organizationId = User.GetOrganizationId();
        var userId = User.GetUserId();
        var incident = await _incidentService.AssignAsync(id, organizationId, userId, request);
        return Ok(incident);
    }
    
    /// <summary>
    /// Update incident status
    /// </summary>
    [HttpPost("{id}/status")]
    [Authorize(Policy = "RequireOperatorRole")]
    [ProducesResponseType(typeof(IncidentDetailDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<IncidentDetailDto>> UpdateStatus(Guid id, [FromBody] UpdateStatusRequest request)
    {
        var organizationId = User.GetOrganizationId();
        var userId = User.GetUserId();
        var incident = await _incidentService.UpdateStatusAsync(id, organizationId, userId, request);
        return Ok(incident);
    }
    
    /// <summary>
    /// Resolve incident
    /// </summary>
    [HttpPost("{id}/resolve")]
    [Authorize(Policy = "RequireOperatorRole")]
    [ProducesResponseType(typeof(IncidentDetailDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<IncidentDetailDto>> Resolve(Guid id, [FromBody] ResolveIncidentRequest request)
    {
        var organizationId = User.GetOrganizationId();
        var userId = User.GetUserId();
        var incident = await _incidentService.ResolveAsync(id, organizationId, userId, request);
        return Ok(incident);
    }
    
    /// <summary>
    /// Close incident
    /// </summary>
    [HttpPost("{id}/close")]
    [Authorize(Policy = "RequireAdminRole")]
    [ProducesResponseType(typeof(IncidentDetailDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<IncidentDetailDto>> Close(Guid id)
    {
        var organizationId = User.GetOrganizationId();
        var userId = User.GetUserId();
        var incident = await _incidentService.CloseAsync(id, organizationId, userId);
        return Ok(incident);
    }
    
    /// <summary>
    /// Reopen incident
    /// </summary>
    [HttpPost("{id}/reopen")]
    [Authorize(Policy = "RequireOperatorRole")]
    [ProducesResponseType(typeof(IncidentDetailDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<IncidentDetailDto>> Reopen(Guid id, [FromQuery] string? reason = null)
    {
        var organizationId = User.GetOrganizationId();
        var userId = User.GetUserId();
        var incident = await _incidentService.ReopenAsync(id, organizationId, userId, reason);
        return Ok(incident);
    }
    
    /// <summary>
    /// Escalate incident
    /// </summary>
    [HttpPost("{id}/escalate")]
    [Authorize(Policy = "RequireOperatorRole")]
    [ProducesResponseType(typeof(IncidentDetailDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<IncidentDetailDto>> Escalate(Guid id, [FromBody] EscalateIncidentRequest request)
    {
        var organizationId = User.GetOrganizationId();
        var userId = User.GetUserId();
        var incident = await _incidentService.EscalateAsync(id, organizationId, userId, request);
        return Ok(incident);
    }
    
    /// <summary>
    /// Add comment to incident timeline
    /// </summary>
    [HttpPost("{id}/comments")]
    [ProducesResponseType(typeof(IncidentTimelineDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<IncidentTimelineDto>> AddComment(Guid id, [FromBody] AddCommentRequest request)
    {
        var organizationId = User.GetOrganizationId();
        var userId = User.GetUserId();
        var comment = await _incidentService.AddCommentAsync(id, organizationId, userId, request);
        return Ok(comment);
    }
    
    /// <summary>
    /// Get incident timeline
    /// </summary>
    [HttpGet("{id}/timeline")]
    [ProducesResponseType(typeof(List<IncidentTimelineDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<IncidentTimelineDto>>> GetTimeline(Guid id, [FromQuery] bool includeInternal = true)
    {
        var organizationId = User.GetOrganizationId();
        var isAdmin = User.IsAdmin();
        
        // Non-admins cannot see internal notes
        var showInternal = includeInternal && isAdmin;
        
        var timeline = await _incidentService.GetTimelineAsync(id, organizationId, showInternal);
        return Ok(timeline);
    }
    
    /// <summary>
    /// Link resource to incident
    /// </summary>
    [HttpPost("{id}/resources")]
    [Authorize(Policy = "RequireOperatorRole")]
    [ProducesResponseType(typeof(IncidentResourceDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<IncidentResourceDto>> LinkResource(Guid id, [FromBody] LinkResourceRequest request)
    {
        var organizationId = User.GetOrganizationId();
        var resource = await _incidentService.LinkResourceAsync(id, organizationId, request);
        return Ok(resource);
    }
    
    /// <summary>
    /// Unlink resource from incident
    /// </summary>
    [HttpDelete("{id}/resources/{resourceId}")]
    [Authorize(Policy = "RequireOperatorRole")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse>> UnlinkResource(Guid id, Guid resourceId)
    {
        var organizationId = User.GetOrganizationId();
        await _incidentService.UnlinkResourceAsync(id, organizationId, resourceId);
        return Ok(ApiResponse.Ok("Resource unlinked"));
    }
    
    /// <summary>
    /// Get incident status counts
    /// </summary>
    [HttpGet("status-counts")]
    [ProducesResponseType(typeof(Dictionary<string, int>), StatusCodes.Status200OK)]
    public async Task<ActionResult<Dictionary<string, int>>> GetStatusCounts()
    {
        var organizationId = User.GetOrganizationId();
        var counts = await _incidentService.GetStatusCountsAsync(organizationId);
        return Ok(counts.ToDictionary(k => k.Key.ToString(), v => v.Value));
    }
    
    /// <summary>
    /// Get open incidents by severity
    /// </summary>
    [HttpGet("severity-counts")]
    [ProducesResponseType(typeof(Dictionary<string, int>), StatusCodes.Status200OK)]
    public async Task<ActionResult<Dictionary<string, int>>> GetSeverityCounts()
    {
        var organizationId = User.GetOrganizationId();
        var counts = await _incidentService.GetSeverityCountsAsync(organizationId);
        return Ok(counts.ToDictionary(k => k.Key.ToString(), v => v.Value));
    }
    
    /// <summary>
    /// Get incident statistics
    /// </summary>
    [HttpGet("statistics")]
    [ProducesResponseType(typeof(IncidentStatisticsDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<IncidentStatisticsDto>> GetStatistics(
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null)
    {
        var organizationId = User.GetOrganizationId();
        var fromDate = from ?? DateTime.UtcNow.AddDays(-30);
        var toDate = to ?? DateTime.UtcNow;
        
        var stats = await _incidentService.GetStatisticsAsync(organizationId, fromDate, toDate);
        return Ok(stats);
    }
}
```

---

## 4.10 Job Registration Update
```csharp
// src/ERAMonitor.BackgroundJobs/Services/JobRegistrationService.cs (updated)

public void RegisterRecurringJobs()
{
    _logger.LogInformation("Registering recurring background jobs...");
    
    // ... existing jobs ...
    
    // Check scheduler - every 15 seconds
    RecurringJob.AddOrUpdate<CheckSchedulerJob>(
        "check-scheduler",
        job => job.ExecuteAsync(),
        "*/15 * * * * *" // Every 15 seconds
    );
    
    // SLA monitor - every minute
    RecurringJob.AddOrUpdate<SlaMonitorJob>(
        "sla-monitor",
        job => job.ExecuteAsync(),
        "* * * * *" // Every minute
    );
    
    // Certificate expiry checker - daily at 9 AM
    RecurringJob.AddOrUpdate<CertificateExpiryCheckerJob>(
        "certificate-expiry-checker",
        job => job.ExecuteAsync(),
        "0 9 * * *" // Daily at 9:00 AM
    );
    
    // Check results cleanup - daily at 4 AM
    RecurringJob.AddOrUpdate<CheckResultsCleanupJob>(
        "check-results-cleanup",
        job => job.ExecuteAsync(),
        "0 4 * * *" // Daily at 4:00 AM
    );
    
    _logger.LogInformation("Background jobs registered successfully");
}
```

---

## 4.11 Phase 4 Checklist
```markdown
# Phase 4 Completion Checklist

## Entities
- [ ] Check entity with HTTP/TCP/Ping/DNS settings
- [ ] CheckResult entity for time series data
- [ ] Incident entity with full lifecycle
- [ ] IncidentTimeline entity for activity log
- [ ] IncidentResource entity for linked resources

## Enums
- [ ] CheckType (HTTP, TCP, Ping, DNS, CustomHealth)
- [ ] IncidentStatus (New, Acknowledged, InProgress, Resolved, Closed)
- [ ] IncidentSeverity (Critical, High, Medium, Low, Info)

## DTOs
Check DTOs:
- [ ] CheckDto, CheckListItemDto, CheckDetailDto
- [ ] CreateCheckRequest, UpdateCheckRequest
- [ ] CheckResultDto, CheckHistoryDto
- [ ] HttpCheckSettingsDto, TcpCheckSettingsDto, DnsCheckSettingsDto

Incident DTOs:
- [ ] IncidentDto, IncidentListItemDto, IncidentDetailDto
- [ ] CreateIncidentRequest, UpdateIncidentRequest
- [ ] Action DTOs (Acknowledge, Assign, Resolve, Escalate, Comment)
- [ ] IncidentTimelineDto, IncidentResourceDto
- [ ] SlaInfoDto, IncidentStatisticsDto

## Services
- [ ] ICheckService with full CRUD and execution
- [ ] ICheckExecutorService with HTTP/TCP/Ping/DNS implementations
- [ ] IIncidentService with lifecycle management
- [ ] Auto-incident creation from failed checks
- [ ] Auto-resolve on recovery

## Repositories
- [ ] ICheckRepository
- [ ] ICheckResultRepository
- [ ] IIncidentRepository
- [ ] IIncidentTimelineRepository

## Controllers
- [ ] ChecksController with CRUD and run/history
- [ ] IncidentsController with lifecycle actions

## Background Jobs
- [ ] CheckSchedulerJob (every 15 seconds)
- [ ] CheckExecutionJob (per check)
- [ ] SlaMonitorJob (every minute)
- [ ] CertificateExpiryCheckerJob (daily)
- [ ] CheckResultsCleanupJob (daily)

## API Endpoints

Checks:
- [ ] GET /api/checks
- [ ] GET /api/checks/{id}
- [ ] POST /api/checks
- [ ] PUT /api/checks/{id}
- [ ] DELETE /api/checks/{id}
- [ ] POST /api/checks/{id}/toggle-monitoring
- [ ] POST /api/checks/{id}/maintenance
- [ ] POST /api/checks/{id}/run
- [ ] GET /api/checks/{id}/history
- [ ] GET /api/checks/{id}/results
- [ ] GET /api/checks/status-counts
- [ ] GET /api/checks/type-counts
- [ ] GET /api/checks/expiring-certificates

Incidents:
- [ ] GET /api/incidents
- [ ] GET /api/incidents/{id}
- [ ] POST /api/incidents
- [ ] PUT /api/incidents/{id}
- [ ] POST /api/incidents/{id}/acknowledge
- [ ] POST /api/incidents/{id}/assign
- [ ] POST /api/incidents/{id}/status
- [ ] POST /api/incidents/{id}/resolve
- [ ] POST /api/incidents/{id}/close
- [ ] POST /api/incidents/{id}/reopen
- [ ] POST /api/incidents/{id}/escalate
- [ ] POST /api/incidents/{id}/comments
- [ ] GET /api/incidents/{id}/timeline
- [ ] POST /api/incidents/{id}/resources
- [ ] DELETE /api/incidents/{id}/resources/{resourceId}
- [ ] GET /api/incidents/status-counts
- [ ] GET /api/incidents/severity-counts
- [ ] GET /api/incidents/statistics

## Features
- [ ] HTTP checks with SSL certificate validation
- [ ] Expected keyword verification
- [ ] TCP port connectivity
- [ ] Ping (ICMP) checks
- [ ] DNS resolution checks
- [ ] Retry logic with configurable attempts
- [ ] Failure threshold before alerting
- [ ] SLA tracking (response time, resolution time)
- [ ] Incident lifecycle management
- [ ] Incident timeline/activity log
- [ ] Auto-incident creation
- [ ] Auto-resolve on recovery
- [ ] Incident escalation
- [ ] SSL certificate expiry alerts

## Testing
- [ ] CheckExecutorService unit tests
- [ ] IncidentService unit tests
- [ ] Check scheduler integration tests
- [ ] SLA breach detection tests
```