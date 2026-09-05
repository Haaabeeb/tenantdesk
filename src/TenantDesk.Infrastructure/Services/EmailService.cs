using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TenantDesk.Application.Interfaces.Services;

namespace TenantDesk.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SendAsync(string to, string subject, string body)
    {
        var smtpConfig = _configuration.GetSection("Smtp");
        var host = smtpConfig["Host"];
        var port = int.TryParse(smtpConfig["Port"], out var p) ? p : 587;
        var userName = smtpConfig["UserName"];
        var password = smtpConfig["Password"];
        var fromAddress = smtpConfig["FromAddress"] ?? "no-reply@tenantdesk.com";
        var fromName = smtpConfig["FromName"] ?? "TenantDesk Support";
        var enableSsl = bool.TryParse(smtpConfig["EnableSsl"], out var ssl) && ssl;

        // Fallback to logging if SMTP is not configured in development
        if (string.IsNullOrWhiteSpace(host))
        {
            _logger.LogWarning("[SMTP Unconfigured] Email to {To} with Subject '{Subject}': {Body}", to, subject, body);
            return;
        }

        try
        {
            using var message = new MailMessage
            {
                From = new MailAddress(fromAddress, fromName),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };
            message.To.Add(to);

            using var client = new SmtpClient(host, port)
            {
                EnableSsl = enableSsl,
                Credentials = !string.IsNullOrWhiteSpace(userName) 
                    ? new NetworkCredential(userName, password) 
                    : CredentialCache.DefaultNetworkCredentials
            };

            await client.SendMailAsync(message);
            _logger.LogInformation("Email successfully dispatched to {To} with subject '{Subject}'", to, subject);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {To}", to);
            // Do not rethrow to prevent breaking the primary user workflow (e.g., ticket assignment)
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