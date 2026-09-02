
using Microsoft.Extensions.Logging;
using TenantDesk.Application.Interfaces.Services;

namespace TenantDesk.Infrastructure.Services;

public class EmailService(ILogger<EmailService> logger) : IEmailService
{
    private readonly ILogger<EmailService> _logger = logger;

    public Task SendAsync(string to, string subject, string body)
    {
        _logger.LogInformation("[Email Stub] Sending email to '{To}' | Subject: '{Subject}' | Body: '{Body}'",to, subject, body);

        return Task.CompletedTask;
    }

    public Task SendTicketAssignedEmailAsync(string to, Guid ticketId)
    {
        _logger.LogInformation("[Email Stub] Ticket assigned notification sent to '{To}' for Ticket ID: '{TicketId}'",to, ticketId);

        return Task.CompletedTask;
    }

    public Task SendSlaBreachWarningEmailAsync(string to, Guid ticketId, DateTime breachTime)
    {
        _logger.LogInformation("[Email Stub] SLA Breach Warning sent to '{To}' for Ticket ID: '{TicketId}' | Breach Time: {BreachTime:u}",to, ticketId, breachTime);

        return Task.CompletedTask;
    }
}