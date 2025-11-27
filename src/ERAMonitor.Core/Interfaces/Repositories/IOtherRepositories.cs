using ERAMonitor.Core.Entities;

namespace ERAMonitor.Core.Interfaces.Repositories;

// public interface IHostRepository : IRepository<Host> { }
// public interface IServiceRepository : IRepository<Service> { }
public interface ICheckRepository : IRepository<Check> { }
public interface ICheckResultRepository : IRepository<CheckResult> { }
public interface IIncidentRepository : IRepository<Incident> { }
public interface INotificationRepository : IRepository<Notification> { }

