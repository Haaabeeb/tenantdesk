
using TenantDesk.Application.Common;
using TenantDesk.Application.Interfaces.Persistence.Filters;
using TenantDesk.Domain.Entities;

namespace TenantDesk.Application.Interfaces;

public interface INotificationRepository
{
    Task<Notification?> GetByIdAsync(Guid id);
    Task<int> CountUnreadByUserAsync(Guid userId);
    Task AddAsync(Notification notification);
    Task MarkAsReadAsync(Guid id);
    Task MarkAllAsReadAsync(Guid userId);
    Task DeleteOldNotificationsAsync(Guid tenantId, DateTime olderThan);
    Task<PagedResult<Notification>> GetFilteredAsync(NotificationFilter filter);
    

}
