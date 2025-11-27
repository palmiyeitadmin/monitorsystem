namespace ERAMonitor.Core.Interfaces.Services;

public interface IEmailService
{
    Task SendAsync(string to, string subject, string htmlBody, string? textBody = null);
    Task SendTemplateAsync(string to, string templateName, Dictionary<string, string> parameters);
    Task SendWelcomeEmailAsync(string to, string fullName, string temporaryPassword);
    Task SendPasswordResetEmailAsync(string to, string fullName, string resetLink);
    Task SendEmailVerificationAsync(string to, string fullName, string verificationLink);
    Task SendIncidentNotificationAsync(string to, string fullName, object incidentData);
}
