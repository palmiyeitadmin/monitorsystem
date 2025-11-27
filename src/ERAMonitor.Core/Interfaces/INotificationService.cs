using ERAMonitor.Core.DTOs;
using ERAMonitor.Core.Entities;

namespace ERAMonitor.Core.Interfaces;

public interface INotificationService
{
    Task SendNotification(NotificationRequest request);
    Task SendCheckDownNotification(Check check, CheckResult result);
    Task SendCheckUpNotification(Check check, CheckResult result);
    Task SendSslExpiryWarning(Check check, int daysRemaining);
    Task SendIncidentCreatedNotification(Incident incident);
    Task SendIncidentResolvedNotification(Incident incident);
    
    // Phase 3 Methods
    Task SendHostDownNotificationAsync(Host host);
    Task SendHostRecoveredNotificationAsync(Host host);
    Task SendServiceDownNotificationAsync(Service service, Host host);
    Task SendServiceRecoveredNotificationAsync(Service service, Host host);
}
