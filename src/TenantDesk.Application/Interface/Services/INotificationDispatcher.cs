using TenantDesk.Domain.Enums;

namespace TenantDesk.Application.Interfaces.Services;

public interface INotificationDispatcher
{
    Task DispatchAsync(Guid userId, NotificationType type, string message, Guid? ticketId);
}