namespace ERAMonitor.Core.Entities.Configurations;

public class EmailChannelConfig
{
    public string SmtpHost { get; set; } = string.Empty;
    public int SmtpPort { get; set; } = 587;
    public bool UseSsl { get; set; } = true;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FromAddress { get; set; } = string.Empty;
    public string FromName { get; set; } = string.Empty;
    public string? ReplyToAddress { get; set; }
}

public class SmsChannelConfig
{
    public string Provider { get; set; } = "Twilio"; // Twilio, Nexmo, etc.
    public string AccountSid { get; set; } = string.Empty;
    public string AuthToken { get; set; } = string.Empty;
    public string FromNumber { get; set; } = string.Empty;
}

public class WebhookChannelConfig
{
    public string Url { get; set; } = string.Empty;
    public string Method { get; set; } = "POST";
    public Dictionary<string, string> Headers { get; set; } = new();
    public string? SecretKey { get; set; } // For HMAC signature
    public bool IncludeSignature { get; set; } = true;
    public int TimeoutSeconds { get; set; } = 30;
    public int RetryCount { get; set; } = 3;
}

public class TelegramChannelConfig
{
    public string BotToken { get; set; } = string.Empty;
    public string ChatId { get; set; } = string.Empty;
    public bool ParseMode { get; set; } = true; // Use Markdown
    public bool DisableNotification { get; set; } = false;
}

public class SlackChannelConfig
{
    public string WebhookUrl { get; set; } = string.Empty;
    public string Channel { get; set; } = string.Empty;
    public string Username { get; set; } = "ERA Monitor";
    public string IconEmoji { get; set; } = ":warning:";
    public bool UseBlocks { get; set; } = true;
}

public class MsTeamsChannelConfig
{
    public string WebhookUrl { get; set; } = string.Empty;
    public string ThemeColor { get; set; } = "29ABE2"; // ERA brand color
}

public class PushoverChannelConfig
{
    public string ApiToken { get; set; } = string.Empty;
    public string UserKey { get; set; } = string.Empty;
    public string? Device { get; set; }
    public int Priority { get; set; } = 0; // -2 to 2
    public string? Sound { get; set; }
}
