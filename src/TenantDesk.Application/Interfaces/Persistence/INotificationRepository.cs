
using TenantDesk.Domain.Entities;

namespace TenantDesk.Application.Interfaces;

public interface INotificationRepository
{
    Task<Notification?> GetByIdAsync(Guid id);
    Task<IEnumerable<Notification>> GetAllByUserAsync(Guid userId);
    Task<IEnumerable<Notification>> GetUnreadByUserAsync(Guid userId);
    Task<int> CountUnreadByUserAsync(Guid userId);
    Task AddAsync(Notification notification);
    Task MarkAsReadAsync(Guid id);
    Task MarkAllAsReadAsync(Guid userId);
    Task DeleteOldNotificationsAsync(Guid tenantId, DateTime olderThan);
}
