using TenantDesk.Application.Models;

namespace TenantDesk.Application.Interfaces.Services;


public interface IEmailJobQueue
{
    
    ValueTask EnqueueAsync(EmailJob job, CancellationToken cancellationToken = default);

    ValueTask<EmailJob> ReadAsync(CancellationToken cancellationToken = default);
}
