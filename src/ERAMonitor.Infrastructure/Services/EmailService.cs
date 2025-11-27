using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using ERAMonitor.Core.Configuration;
using ERAMonitor.Core.Interfaces.Services;

namespace ERAMonitor.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly SmtpSettings _smtpSettings;
    private readonly ILogger<EmailService> _logger;
    
    public EmailService(IOptions<SmtpSettings> smtpSettings, ILogger<EmailService> logger)
    {
        _smtpSettings = smtpSettings.Value;
        _logger = logger;
    }
    
    public async Task SendAsync(string to, string subject, string htmlBody, string? textBody = null)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_smtpSettings.FromName, _smtpSettings.FromEmail));
        message.To.Add(MailboxAddress.Parse(to));
        message.Subject = subject;
        
        var builder = new BodyBuilder
        {
            HtmlBody = htmlBody
        };
        
        if (!string.IsNullOrEmpty(textBody))
        {
            builder.TextBody = textBody;
        }
        
        message.Body = builder.ToMessageBody();
        
        using var client = new SmtpClient();
        
        try
        {
            await client.ConnectAsync(
                _smtpSettings.Host, 
                _smtpSettings.Port, 
                _smtpSettings.UseSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.None);
            
            if (!string.IsNullOrEmpty(_smtpSettings.Username))
            {
                await client.AuthenticateAsync(_smtpSettings.Username, _smtpSettings.Password);
            }
            
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
            
            _logger.LogInformation("Email sent successfully to {To}", to);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {To}", to);
            throw;
        }
    }
    
    public async Task SendTemplateAsync(string to, string templateName, Dictionary<string, string> parameters)
    {
        var template = GetEmailTemplate(templateName);
        
        foreach (var param in parameters)
        {
            template.Subject = template.Subject.Replace($"{{{{{param.Key}}}}}", param.Value);
            template.Body = template.Body.Replace($"{{{{{param.Key}}}}}", param.Value);
        }
        
        await SendAsync(to, template.Subject, template.Body);
    }
    
    public async Task SendWelcomeEmailAsync(string to, string fullName, string temporaryPassword)
    {
        var subject = "Welcome to ERA Monitor";
        var body = $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #29ABE2; color: white; padding: 20px; text-align: center; }}
        .content {{ padding: 20px; background-color: #f9f9f9; }}
        .button {{ display: inline-block; padding: 12px 24px; background-color: #29ABE2; color: white; text-decoration: none; border-radius: 4px; }}
        .footer {{ text-align: center; padding: 20px; font-size: 12px; color: #666; }}
        .credentials {{ background-color: #fff; padding: 15px; border-left: 4px solid #29ABE2; margin: 15px 0; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>Welcome to ERA Monitor</h1>
        </div>
        <div class='content'>
            <p>Hello {fullName},</p>
            <p>Your ERA Monitor account has been created. You can now access the monitoring dashboard.</p>
            
            <div class='credentials'>
                <p><strong>Your login credentials:</strong></p>
                <p>Email: {to}</p>
                <p>Temporary Password: <code>{temporaryPassword}</code></p>
            </div>
            
            <p>Please change your password after your first login.</p>
            
            <p style='text-align: center;'>
                <a href='https://monitor.eracloud.com.tr/login' class='button'>Login to ERA Monitor</a>
            </p>
        </div>
        <div class='footer'>
            <p>This email was sent by ERA Monitor. If you didn't request this, please ignore this email.</p>
            <p>&copy; {DateTime.UtcNow.Year} ERA Cloud. All rights reserved.</p>
        </div>
    </div>
</body>
</html>";
        
        await SendAsync(to, subject, body);
    }
    
    public async Task SendPasswordResetEmailAsync(string to, string fullName, string resetLink)
    {
        var subject = "Reset Your ERA Monitor Password";
        var body = $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #29ABE2; color: white; padding: 20px; text-align: center; }}
        .content {{ padding: 20px; background-color: #f9f9f9; }}
        .button {{ display: inline-block; padding: 12px 24px; background-color: #29ABE2; color: white; text-decoration: none; border-radius: 4px; }}
        .footer {{ text-align: center; padding: 20px; font-size: 12px; color: #666; }}
        .warning {{ background-color: #FFF3CD; padding: 10px; border-radius: 4px; margin: 15px 0; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>Password Reset Request</h1>
        </div>
        <div class='content'>
            <p>Hello {fullName},</p>
            <p>We received a request to reset your password for your ERA Monitor account.</p>
            
            <p style='text-align: center;'>
                <a href='{resetLink}' class='button'>Reset Password</a>
            </p>
            
            <div class='warning'>
                <p><strong>⚠️ This link expires in 1 hour.</strong></p>
            </div>
            
            <p>If you didn't request a password reset, please ignore this email or contact support if you have concerns.</p>
        </div>
        <div class='footer'>
            <p>This email was sent by ERA Monitor.</p>
            <p>&copy; {DateTime.UtcNow.Year} ERA Cloud. All rights reserved.</p>
        </div>
    </div>
</body>
</html>";
        
        await SendAsync(to, subject, body);
    }
    
    public async Task SendEmailVerificationAsync(string to, string fullName, string verificationLink)
    {
        var subject = "Verify Your ERA Monitor Email";
        var body = $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #29ABE2; color: white; padding: 20px; text-align: center; }}
        .content {{ padding: 20px; background-color: #f9f9f9; }}
        .button {{ display: inline-block; padding: 12px 24px; background-color: #29ABE2; color: white; text-decoration: none; border-radius: 4px; }}
        .footer {{ text-align: center; padding: 20px; font-size: 12px; color: #666; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>Verify Your Email</h1>
        </div>
        <div class='content'>
            <p>Hello {fullName},</p>
            <p>Please verify your email address by clicking the button below:</p>
            
            <p style='text-align: center;'>
                <a href='{verificationLink}' class='button'>Verify Email</a>
            </p>
            
            <p>This link expires in 24 hours.</p>
        </div>
        <div class='footer'>
            <p>&copy; {DateTime.UtcNow.Year} ERA Cloud. All rights reserved.</p>
        </div>
    </div>
</body>
</html>";
        
        await SendAsync(to, subject, body);
    }
    
    public async Task SendIncidentNotificationAsync(string to, string fullName, object incidentData)
    {
        // Implementation for incident notification
        await Task.CompletedTask;
    }
    
    private (string Subject, string Body) GetEmailTemplate(string templateName)
    {
        // In a real implementation, load templates from database or files
        return templateName switch
        {
            "welcome" => ("Welcome to ERA Monitor", ""),
            "password_reset" => ("Reset Your Password", ""),
            _ => ("ERA Monitor Notification", "")
        };
    }
}
