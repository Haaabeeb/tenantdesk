using System.Data;
using System.Runtime.CompilerServices;

public interface IEmailService
{
    Task SendAsync(string to , string subject , string body);
    Task SendTicketAssignedEmailAsync(string to , Guid ticketId);
    Task SendSlaBreachWarningEmailAsync(string to,Guid ticketId,DateTime breachTime);
    


    
}