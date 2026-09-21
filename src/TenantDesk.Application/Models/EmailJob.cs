namespace TenantDesk.Application.Models;


public sealed class EmailJob
{
   public Guid TenantId { get; init; }

    public Guid TicketId { get; init; }

    public string To { get; init; } = string.Empty;

    public string Subject { get; init; } = string.Empty;

    public string Body { get; init; } = string.Empty;

    public DateTime EnqueuedAt { get; init; } = DateTime.UtcNow;
}
