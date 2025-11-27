using ERAMonitor.Core.Entities;
using ERAMonitor.Core.Enums;
using Microsoft.EntityFrameworkCore;

namespace ERAMonitor.Infrastructure.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }
    
    public DbSet<Organization> Organizations => Set<Organization>();
    public DbSet<User> Users => Set<User>();
    public DbSet<UserSession> UserSessions => Set<UserSession>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<CustomerUser> CustomerUsers => Set<CustomerUser>();
    public DbSet<UserCustomerAssignment> UserCustomerAssignments => Set<UserCustomerAssignment>();
    public DbSet<Location> Locations => Set<Location>();
    public DbSet<Host> Hosts => Set<Host>();
    public DbSet<Service> Services => Set<Service>();
    public DbSet<Check> Checks => Set<Check>();
    public DbSet<Incident> Incidents => Set<Incident>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<HostDisk> HostDisks => Set<HostDisk>();
    public DbSet<HostMetric> HostMetrics => Set<HostMetric>();
    public DbSet<ServiceStatusHistory> ServiceStatusHistories => Set<ServiceStatusHistory>();
    public DbSet<CheckResult> CheckResults => Set<CheckResult>();
    public DbSet<IncidentTimeline> IncidentTimelines => Set<IncidentTimeline>();
    public DbSet<IncidentResource> IncidentResources => Set<IncidentResource>();
    public DbSet<NotificationRule> NotificationRules => Set<NotificationRule>();
    public DbSet<ScheduledReport> ScheduledReports => Set<ScheduledReport>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<SystemSetting> SystemSettings => Set<SystemSetting>();
    public DbSet<Report> Reports => Set<Report>();
    
    public DbSet<Dashboard> Dashboards => Set<Dashboard>();
    public DbSet<DashboardWidget> DashboardWidgets => Set<DashboardWidget>();
    public DbSet<ReportExecution> ReportExecutions => Set<ReportExecution>();
    public DbSet<StatusPage> StatusPages => Set<StatusPage>();
    public DbSet<StatusPageComponent> StatusPageComponents => Set<StatusPageComponent>();
    public DbSet<StatusPageComponentGroup> StatusPageComponentGroups => Set<StatusPageComponentGroup>();
    public DbSet<StatusPageSubscriber> StatusPageSubscribers => Set<StatusPageSubscriber>();
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Register Enums
        modelBuilder.HasPostgresEnum<UserRole>();
        modelBuilder.HasPostgresEnum<OsType>();
        modelBuilder.HasPostgresEnum<HostCategory>();
        modelBuilder.HasPostgresEnum<LocationCategory>();
        modelBuilder.HasPostgresEnum<CheckType>();
        modelBuilder.HasPostgresEnum<StatusType>();
        modelBuilder.HasPostgresEnum<IncidentStatus>();
        modelBuilder.HasPostgresEnum<IncidentSeverity>();
        modelBuilder.HasPostgresEnum<IncidentPriority>();
        modelBuilder.HasPostgresEnum<NotificationChannelType>();
        modelBuilder.HasPostgresEnum<NotificationStatus>();
        modelBuilder.HasPostgresEnum<NotificationEventType>();
        modelBuilder.HasPostgresEnum<NotificationPriority>();
        modelBuilder.HasPostgresEnum<OnCallRotationType>();
        modelBuilder.HasPostgresEnum<ServiceType>();
        
        // Phase 6 Enums
        modelBuilder.HasPostgresEnum<DashboardVisibility>();
        modelBuilder.HasPostgresEnum<WidgetType>();
        modelBuilder.HasPostgresEnum<ReportType>();
        modelBuilder.HasPostgresEnum<ReportSchedule>();
        modelBuilder.HasPostgresEnum<ReportTimeRange>();
        modelBuilder.HasPostgresEnum<ReportFormat>();
        modelBuilder.HasPostgresEnum<ReportExecutionStatus>();
        modelBuilder.HasPostgresEnum<StatusPageComponentType>();
        modelBuilder.HasPostgresEnum<StatusPageComponentStatus>();
        
        // Enable Extensions
        modelBuilder.HasPostgresExtension("uuid-ossp");
        modelBuilder.HasPostgresExtension("pgcrypto");
    }
}
