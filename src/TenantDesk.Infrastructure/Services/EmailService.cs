using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;
using TenantDesk.Infrastructure.Settings;
using Microsoft.Extensions.Logging;
using TenantDesk.Application.Interfaces.Services;

namespace TenantDesk.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly SmtpSettings _smtpSettings;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IOptions<SmtpSettings> smtpSettings, ILogger<EmailService> logger)
    {
        _smtpSettings = smtpSettings.Value;
        _logger = logger;
    }

    
    public async Task SendAsync(string to, string subject, string body)
    {

        if (string.IsNullOrWhiteSpace(_smtpSettings.Host))
        {
            _logger.LogWarning("[SMTP Unconfigured] Email to {To} with Subject '{Subject}': {Body}", to, subject, body);
            return;
        }

        try
        {
            using var message = new MailMessage
            {
                From = new MailAddress(_smtpSettings.FromAddress, _smtpSettings.FromName),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };
            message.To.Add(to);

            using var client = new SmtpClient(_smtpSettings.Host, _smtpSettings.Port)
            {
                EnableSsl = _smtpSettings.EnableSsl,
                Credentials = !string.IsNullOrWhiteSpace(_smtpSettings.UserName) 
                    ? new NetworkCredential(_smtpSettings.UserName, _smtpSettings.Password) 
                    : CredentialCache.DefaultNetworkCredentials
            };

            await client.SendMailAsync(message);
            _logger.LogInformation("Email successfully dispatched to {To} with subject '{Subject}'", to, subject);
        }
        catch (Exception ex)
        {
           _logger.LogError(ex,"Failed to send email. To: {To}, Subject: {Subject}, SMTP Host: {Host}, Port: {Port}",
            to,subject,_smtpSettings.Host,_smtpSettings.Port );
        }
    }

    public Task SendTicketAssignedEmailAsync(string to, Guid ticketId)
    {
        var subject = $"Ticket #{ticketId.ToString()[..8]} Assigned to You";
        var body = $@"
            <h3>Hello,</h3>
            <p>You have been assigned a new ticket in <strong>TenantDesk</strong>.</p>
            <p><strong>Ticket Reference:</strong> {ticketId}</p>
            <p>Please log in to the portal to view the details and start resolution.</p>";

        return SendAsync(to, subject, body);
    }

    public Task SendSlaBreachWarningEmailAsync(string to, Guid ticketId, DateTime breachTime)
    {
        var subject = $"URGENT: SLA Breach Warning for Ticket #{ticketId.ToString()[..8]}";
        var body = $@"
            <h3 style='color: red;'>SLA Breach Warning</h3>
            <p>Ticket <strong>#{ticketId}</strong> is approaching or has exceeded its SLA target.</p>
            <p><strong>Target Due Time (UTC):</strong> {breachTime:yyyy-MM-dd HH:mm:ss} UTC</p>
            <p>Immediate action is required.</p>";

        return SendAsync(to, subject, body);
    }
}