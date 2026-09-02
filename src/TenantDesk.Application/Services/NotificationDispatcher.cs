
using TenantDesk.Application.Interfaces;
using TenantDesk.Application.Interfaces.Identity;
using TenantDesk.Application.Interfaces.Services;
using TenantDesk.Domain.Entities;
using TenantDesk.Domain.Enums;

namespace TenantDesk.Application.Services;

public class NotificationDispatcher(INotificationRepository notificationRepository,ICurrentTenantService currentTenantService) : INotificationDispatcher
{
    private readonly INotificationRepository _notificationRepository = notificationRepository;
    private readonly ICurrentTenantService _currentTenantService = currentTenantService;

    public async Task DispatchAsync(Guid userId, NotificationType type, string message, Guid? ticketId)
    {
        var tenantId = _currentTenantService.GetTenantId();
        
        var notification = new Notification(
            tenantId: tenantId,
            userId: userId,
            type: type,
            message: message,
            ticketId: ticketId);

        await _notificationRepository.AddAsync(notification);
    }
}