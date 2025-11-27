PHASE 5: Incidents & Notifications (Days 18-23)5.1 Incident Management APIEndpoints:
GET    /api/incidents                      # List with filters
GET    /api/incidents/{id}                 # Get details with timeline
POST   /api/incidents                      # Create manual incident
PUT    /api/incidents/{id}                 # Update
DELETE /api/incidents/{id}                 # Delete (soft)
POST   /api/incidents/{id}/acknowledge     # Acknowledge
POST   /api/incidents/{id}/assign          # Assign to user
POST   /api/incidents/{id}/resolve         # Resolve
POST   /api/incidents/{id}/close           # Close
POST   /api/incidents/{id}/timeline        # Add timeline entry/note
GET    /api/incidents/{id}/timeline        # Get timeline

Filters:
- status (New, Acknowledged, InProgress, Resolved, Closed)
- severity (Critical, High, Medium, Low)
- customerId
- assignedToId
- dateFrom, dateTo
- search5.2 Incident Servicecsharp// IncidentService.cs

public class IncidentService : IIncidentService
{
    public async Task<Incident> CreateAutoIncident(
        IMonitoredResource resource, 
        string title, 
        IncidentSeverity severity)
    {
        // Check for existing open incident for this resource
        var existingIncident = await _incidentRepository.GetOpenIncidentForResource(
            resource.GetType().Name, 
            resource.Id
        );
        
        if (existingIncident != null)
        {
            // Add to existing incident timeline
            await AddTimelineEntry(existingIncident.Id, "ResourceStatusUpdate", 
                $"Status changed to {resource.CurrentStatus}");
            return existingIncident;
        }
        
        // Create new incident
        var incident = new Incident
        {
            OrganizationId = resource.OrganizationId,
            CustomerId = resource.CustomerId,
            Title = title,
            Description = $"Automatically created due to {resource.GetType().Name} status change",
            Status = IncidentStatus.New,
            Severity = severity,
            Priority = MapSeverityToPriority(severity),
            SourceType = resource.GetType().Name,
            SourceId = resource.Id,
            AffectedResources = JsonSerializer.Serialize(new[] {
                new { 
                    Type = resource.GetType().Name, 
                    Id = resource.Id, 
                    Name = resource.Name,
                    Status = resource.CurrentStatus.ToString()
                }
            })
        };
        
        await _incidentRepository.InsertAsync(incident);
        
        // Add creation timeline entry
        await AddTimelineEntry(incident.Id, "Created", 
            $"Incident automatically created for {resource.Name}");
        
        // Send notifications
        await _notificationService.SendIncidentCreatedNotification(incident);
        
        // Broadcast via SignalR
        await _hubContext.Clients.All.SendAsync("IncidentCreated", incident);
        
        return incident;
    }
    
    public async Task AcknowledgeIncident(Guid incidentId, Guid userId)
    {
        var incident = await _incidentRepository.GetByIdAsync(incidentId);
        var user = await _userRepository.GetByIdAsync(userId);
        
        incident.Status = IncidentStatus.Acknowledged;
        incident.AcknowledgedById = userId;
        incident.AcknowledgedAt = DateTime.UtcNow;
        
        // Check SLA
        var responseTime = (incident.AcknowledgedAt - incident.CreatedAt).Value.TotalMinutes;
        incident.ResponseSlaMet = responseTime <= incident.ResponseSlaMinutes;
        
        await _incidentRepository.UpdateAsync(incident);
        
        await AddTimelineEntry(incidentId, "Acknowledged", 
            $"Acknowledged by {user.FullName}", userId);
        
        await _hubContext.Clients.All.SendAsync("IncidentUpdated", incident);
    }
    
    public async Task ResolveIncident(Guid incidentId, Guid userId, ResolveIncidentRequest request)
    {
        var incident = await _incidentRepository.GetByIdAsync(incidentId);
        var user = await _userRepository.GetByIdAsync(userId);
        
        incident.Status = IncidentStatus.Resolved;
        incident.ResolvedById = userId;
        incident.ResolvedAt = DateTime.UtcNow;
        incident.RootCauseCategory = request.RootCauseCategory;
        incident.RootCauseDescription = request.RootCauseDescription;
        incident.ResolutionSteps = request.ResolutionSteps;
        incident.PreventiveActions = request.PreventiveActions;
        
        // Check SLA
        var resolutionTime = (incident.ResolvedAt - incident.CreatedAt).Value.TotalMinutes;
        incident.ResolutionSlaMet = resolutionTime <= incident.ResolutionSlaMinutes;
        
        await _incidentRepository.UpdateAsync(incident);
        
        await AddTimelineEntry(incidentId, "Resolved", 
            $"Resolved by {user.FullName}: {request.ResolutionSteps}", userId);
        
        await _notificationService.SendIncidentResolvedNotification(incident);
        
        await _hubContext.Clients.All.SendAsync("IncidentUpdated", incident);
    }
}5.3 Notification Servicecsharp// NotificationService.cs

public class NotificationService : INotificationService
{
    private readonly IEmailSender _emailSender;
    private readonly ITelegramService _telegramService;
    private readonly IWebhookService _webhookService;
    private readonly INotificationRepository _notificationRepository;
    
    public async Task SendNotification(NotificationRequest request)
    {
        var notification = new Notification
        {
            OrganizationId = request.OrganizationId,
            TriggerType = request.TriggerType,
            TriggerId = request.TriggerId,
            Channel = request.Channel,
            Recipient = request.Recipient,
            Subject = request.Subject,
            Content = request.Content,
            ContentHtml = request.ContentHtml,
            Status = NotificationStatus.Pending
        };
        
        await _notificationRepository.InsertAsync(notification);
        
        try
        {
            switch (request.Channel)
            {
                case NotificationChannel.Email:
                    await _emailSender.SendAsync(
                        request.Recipient, 
                        request.Subject, 
                        request.ContentHtml ?? request.Content
                    );
                    break;
                    
                case NotificationChannel.Telegram:
                    var messageId = await _telegramService.SendMessageAsync(
                        request.Recipient, // Chat ID
                        request.Content
                    );
                    notification.ExternalMessageId = messageId;
                    break;
                    
                case NotificationChannel.Webhook:
                    await _webhookService.SendAsync(
                        request.Recipient, // Webhook URL
                        new {
                            type = request.TriggerType,
                            subject = request.Subject,
                            content = request.Content,
                            timestamp = DateTime.UtcNow
                        }
                    );
                    break;
                    
                case NotificationChannel.SMS:
                    // Implement SMS provider integration
                    break;
            }
            
            notification.Status = NotificationStatus.Sent;
            notification.SentAt = DateTime.UtcNow;
        }
        catch (Exception ex)
        {
            notification.Status = NotificationStatus.Failed;
            notification.ErrorMessage = ex.Message;
            notification.RetryCount++;
            notification.NextRetryAt = DateTime.UtcNow.AddMinutes(5 * notification.RetryCount);
        }
        
        await _notificationRepository.UpdateAsync(notification);
    }
    
    public async Task SendHostDownNotification(Host host)
    {
        // Get notification rules for this host/customer
        var rules = await _ruleRepository.GetActiveRulesForCondition("HostDown", host.CustomerId);
        
        foreach (var rule in rules)
        {
            var recipients = await GetRecipientsForRule(rule);
            
            foreach (var recipient in recipients)
            {
                await SendNotification(new NotificationRequest
                {
                    OrganizationId = host.OrganizationId,
                    TriggerType = "Host",
                    TriggerId = host.Id,
                    RuleId = rule.Id,
                    Channel = rule.Channel,
                    Recipient = recipient,
                    Subject = $"[CRITICAL] Host DOWN: {host.Name}",
                    Content = BuildHostDownMessage(host),
                    ContentHtml = BuildHostDownHtmlMessage(host)
                });
            }
        }
        
        // Also notify customer if configured
        var customer = await _customerRepository.GetByIdAsync(host.CustomerId);
        if (customer?.NotificationSettings?.Channels?.Email == true &&
            customer.NotificationSettings.NotifyOn.HostDown)
        {
            foreach (var email in customer.NotificationSettings.EmailRecipients)
            {
                await SendNotification(new NotificationRequest
                {
                    // ... customer notification
                });
            }
        }
    }
    
    private string BuildHostDownMessage(Host host)
    {
        return $@"🔴 CRITICAL ALERT

Host: {host.Name}
Status: DOWN
Hostname: {host.Hostname}
Location: {host.Location?.Name}
Customer: {host.Customer?.Name}

Last seen: {host.LastSeenAt:yyyy-MM-dd HH:mm:ss} UTC

This is an automated alert from ERA Monitor.";
    }
}5.4 Notification History APIEndpoints:
GET    /api/notifications                  # List with filters
GET    /api/notifications/{id}             # Get details
POST   /api/notifications/{id}/resend      # Resend failed notification
GET    /api/notifications/stats            # Get stats (sent, failed, etc.)

Filters:
- channel (Email, SMS, Telegram, Webhook)
- status (Pending, Sent, Delivered, Failed)
- triggerId
- customerId
- dateFrom, dateTo