using System;
using System.Threading.Tasks;

namespace ERAMonitor.Core.Interfaces.Services;

public interface IRealTimeService
{
    Task BroadcastHostUpdateAsync(object hostUpdate, Guid? customerId = null);
    Task BroadcastIncidentCreatedAsync(object incident, Guid? customerId = null);
    Task BroadcastIncidentUpdatedAsync(object incident, Guid? customerId = null);
}
