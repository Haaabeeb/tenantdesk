using Microsoft.EntityFrameworkCore;
using TenantDesk.Application.Common;
using TenantDesk.Application.Interfaces;
using TenantDesk.Application.Interfaces.Persistence.Filters;
using TenantDesk.Domain.Entities;
using TenantDesk.Infrastructure.Data;

namespace TenantDesk.Infrastructure.Repositories;

public class NotificationRepository : INotificationRepository
{
    private readonly AppDbContext _context;

    public NotificationRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Notification?> GetByIdAsync(Guid id) =>
        await _context.Notifications.FirstOrDefaultAsync(n => n.NotificationId == id);

    public async Task AddAsync(Notification notification)
    {
        await _context.Notifications.AddAsync(notification);
        await _context.SaveChangesAsync();
    }

    public Task<int> CountUnreadByUserAsync(Guid userId) =>
        Task.FromResult(0);

    public Task MarkAsReadAsync(Guid id) =>
        Task.CompletedTask;

    public Task MarkAllAsReadAsync(Guid userId) =>
        Task.CompletedTask;

    public Task DeleteOldNotificationsAsync(Guid tenantId, DateTime olderThan) =>
        Task.CompletedTask;

    public Task<PagedResult<Notification>> GetFilteredAsync(NotificationFilter filter) =>
        throw new NotImplementedException("Phase 11: NotificationsController query side.");


}