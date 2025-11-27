using ERAMonitor.Core.Interfaces.Repositories;

namespace ERAMonitor.Core.Interfaces.Repositories;

public interface IUnitOfWork : IDisposable
{
    IUserRepository Users { get; }
    IUserSessionRepository UserSessions { get; }
    ICustomerRepository Customers { get; }
    ILocationRepository Locations { get; }
    IHostRepository Hosts { get; }
    IHostMetricRepository HostMetrics { get; }
    IHostDiskRepository HostDisks { get; }
    IServiceRepository Services { get; }
    IServiceStatusHistoryRepository ServiceStatusHistory { get; }
    ICheckRepository Checks { get; }
    ICheckResultRepository CheckResults { get; }
    IIncidentRepository Incidents { get; }
    INotificationRepository Notifications { get; }
    IReportRepository Reports { get; }
    IReportExecutionRepository ReportExecutions { get; }
    IDashboardRepository Dashboards { get; }
    IDashboardWidgetRepository DashboardWidgets { get; }
    IStatusPageRepository StatusPages { get; }
    IStatusPageComponentRepository StatusPageComponents { get; }
    IStatusPageComponentGroupRepository StatusPageComponentGroups { get; }
    IStatusPageSubscriberRepository StatusPageSubscribers { get; }
    IAuditLogRepository AuditLogs { get; }
    
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}
