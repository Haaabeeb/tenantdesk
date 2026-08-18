
using System;
using TenantDesk.Domain.Enums;

namespace TenantDesk.Domain.Entities;

public class Notification
{
    public Guid NotificationId { get; private set; }
    public Guid TenantId { get; private set; }
    public Guid UserId { get; private set; }
    public Guid? TicketId { get; private set; }
    public NotificationType Type { get; private set; }
    public string Message { get; private set; } = null!;
    public bool IsRead { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public Tenant Tenant { get; private set; } = null!;
    public User User { get; private set; } = null!;
    public Ticket? Ticket { get; private set; }

    private Notification() { }

    public Notification(Guid tenantId, Guid userId, NotificationType type, string message, Guid? ticketId = null)
    {
    
        if (tenantId == Guid.Empty)
            throw new ArgumentException("Tenant ID cannot be empty.", nameof(tenantId));

        if (userId == Guid.Empty)
            throw new ArgumentException("User ID cannot be empty.", nameof(userId));

        if (string.IsNullOrWhiteSpace(message))
            throw new ArgumentException("Notification message cannot be null or empty.", nameof(message));

        TenantId = tenantId;
        UserId = userId;
        Type = type;
        Message = message.Trim();
        TicketId = ticketId;
        IsRead = false; 
        CreatedAt = DateTime.UtcNow;
    }

    public void MarkAsRead()
    {
        if (!IsRead)
        {
            IsRead = true;
        }
    }
}