using ERAMonitor.Core.Enums;

namespace ERAMonitor.Core.Entities;

public class NotificationTemplate : BaseEntityWithOrganization
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    
    // Template type
    public NotificationEventType EventType { get; set; }
    public NotificationChannelType ChannelType { get; set; }
    
    // Content templates (support variables like {{host_name}}, {{status}})
    public string Subject { get; set; } = string.Empty; // For email
    public string Body { get; set; } = string.Empty;
    public string? HtmlBody { get; set; } // For email HTML version
    
    // Formatting
    public string? Format { get; set; } // json, markdown, plain
    
    // Status
    public bool IsActive { get; set; } = true;
    public bool IsDefault { get; set; } = false;
    
    // Metadata
    public string? Variables { get; set; } // JSON list of available variables
    
    // Helper to get variable list
    public List<string> GetVariables()
    {
        if (string.IsNullOrEmpty(Variables))
            return new List<string>();
        
        return System.Text.Json.JsonSerializer.Deserialize<List<string>>(Variables) ?? new List<string>();
    }
}
