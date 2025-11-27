using ERAMonitor.Core.Entities;
using ERAMonitor.Core.Interfaces.Repositories;

namespace ERAMonitor.Infrastructure.Data.Repositories;

// public class HostRepository : Repository<Host>, IHostRepository
// {
//     public HostRepository(ApplicationDbContext context) : base(context) { }
// }
//
// public class ServiceRepository : Repository<Service>, IServiceRepository
// {
//     public ServiceRepository(ApplicationDbContext context) : base(context) { }
// }

public class CheckRepository : Repository<Check>, ICheckRepository
{
    public CheckRepository(ApplicationDbContext context) : base(context) { }
}

public class CheckResultRepository : Repository<CheckResult>, ICheckResultRepository
{
    public CheckResultRepository(ApplicationDbContext context) : base(context) { }
}

public class IncidentRepository : Repository<Incident>, IIncidentRepository
{
    public IncidentRepository(ApplicationDbContext context) : base(context) { }
}

public class NotificationRepository : Repository<Notification>, INotificationRepository
{
    public NotificationRepository(ApplicationDbContext context) : base(context) { }
}


