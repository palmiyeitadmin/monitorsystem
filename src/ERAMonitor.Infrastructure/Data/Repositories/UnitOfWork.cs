using ERAMonitor.Core.Interfaces.Repositories;
using ERAMonitor.Infrastructure.Repositories;

namespace ERAMonitor.Infrastructure.Data.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;
    
    public IUserRepository Users { get; }
    public IUserSessionRepository UserSessions { get; }
    public ICustomerRepository Customers { get; }
    public ILocationRepository Locations { get; }
    public IHostRepository Hosts { get; }
    public IHostMetricRepository HostMetrics { get; }
    public IHostDiskRepository HostDisks { get; }
    public IServiceRepository Services { get; }
    public IServiceStatusHistoryRepository ServiceStatusHistory { get; }
    public ICheckRepository Checks { get; }
    public ICheckResultRepository CheckResults { get; }
    public IIncidentRepository Incidents { get; }
    public INotificationRepository Notifications { get; }
    public IAuditLogRepository AuditLogs { get; }
    
    // Lazy loaded repositories
    private IReportRepository? _reports;
    private IReportExecutionRepository? _reportExecutions;
    private IDashboardRepository? _dashboards;
    private IDashboardWidgetRepository? _dashboardWidgets;
    private IStatusPageRepository? _statusPages;
    private IStatusPageComponentRepository? _statusPageComponents;
    private IStatusPageComponentGroupRepository? _statusPageComponentGroups;
    private IStatusPageSubscriberRepository? _statusPageSubscribers;

    public UnitOfWork(
        ApplicationDbContext context,
        IUserRepository users,
        IUserSessionRepository userSessions,
        ICustomerRepository customers,
        ILocationRepository locations,
        IHostRepository hosts,
        IHostMetricRepository hostMetrics,
        IHostDiskRepository hostDisks,
        IServiceRepository services,
        IServiceStatusHistoryRepository serviceStatusHistory,
        ICheckRepository checks,
        ICheckResultRepository checkResults,
        IIncidentRepository incidents,
        INotificationRepository notifications,
        IAuditLogRepository auditLogs)
    {
        _context = context;
        Users = users;
        UserSessions = userSessions;
        Customers = customers;
        Locations = locations;
        Hosts = hosts;
        HostMetrics = hostMetrics;
        HostDisks = hostDisks;
        Services = services;
        ServiceStatusHistory = serviceStatusHistory;
        Checks = checks;
        CheckResults = checkResults;
        Incidents = incidents;
        Notifications = notifications;
        AuditLogs = auditLogs;
    }
    
    public IReportRepository Reports => _reports ??= new ReportRepository(_context);
    public IReportExecutionRepository ReportExecutions => _reportExecutions ??= new ReportExecutionRepository(_context);
    public IDashboardRepository Dashboards => _dashboards ??= new DashboardRepository(_context);
    public IDashboardWidgetRepository DashboardWidgets => _dashboardWidgets ??= new DashboardWidgetRepository(_context);
    public IStatusPageRepository StatusPages => _statusPages ??= new StatusPageRepository(_context);
    public IStatusPageComponentRepository StatusPageComponents => _statusPageComponents ??= new StatusPageComponentRepository(_context);
    public IStatusPageComponentGroupRepository StatusPageComponentGroups => _statusPageComponentGroups ??= new StatusPageComponentGroupRepository(_context);
    public IStatusPageSubscriberRepository StatusPageSubscribers => _statusPageSubscribers ??= new StatusPageSubscriberRepository(_context);
    
    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }
    
    public async Task BeginTransactionAsync()
    {
        await _context.Database.BeginTransactionAsync();
    }
    
    public async Task CommitTransactionAsync()
    {
        await _context.Database.CommitTransactionAsync();
    }
    
    public async Task RollbackTransactionAsync()
    {
        await _context.Database.RollbackTransactionAsync();
    }
    
    public void Dispose()
    {
        _context.Dispose();
    }
}
