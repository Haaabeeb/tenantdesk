using Microsoft.EntityFrameworkCore;
using TenantDesk.Application.Common;
using TenantDesk.Application.Interfaces;
using TenantDesk.Application.Interfaces.Persistence.Filters;
using TenantDesk.Domain.Entities;
using TenantDesk.Infrastructure.Data;

namespace TenantDesk.Infrastructure.Repositories;

public class NotificationRepository(AppDbContext context) : INotificationRepository
{
    private readonly AppDbContext _context = context;

    public async Task<Notification?> GetByIdAsync(Guid id) =>
        await _context.Notifications.FirstOrDefaultAsync(n => n.NotificationId == id);

    public async Task<int> CountUnreadByUserAsync(Guid userId) =>
        await _context.Notifications.CountAsync(n => n.UserId == userId && !n.IsRead);

    public async Task AddAsync(Notification notification)
    {
        await _context.Notifications.AddAsync(notification);
        await _context.SaveChangesAsync();
    }

    public async Task MarkAsReadAsync(Guid id)
    {
        var notification = await GetByIdAsync(id);
        if (notification is not null)
        {
            notification.MarkAsRead();
            await _context.SaveChangesAsync();
        }
    }

    public async Task MarkAllAsReadAsync(Guid userId)
    {
        var unread = await _context.Notifications
            .Where(n => n.UserId == userId && !n.IsRead)
            .ToListAsync();

        foreach (var notification in unread)
        {
            notification.MarkAsRead();
        }

        if (unread.Count > 0)
        {
            await _context.SaveChangesAsync();
        }
    }

    public async Task DeleteOldNotificationsAsync(Guid tenantId, DateTime olderThan)
    {
        var oldNotifications = await _context.Notifications
            .Where(n => n.TenantId == tenantId && n.CreatedAt < olderThan)
            .ToListAsync();

        if (oldNotifications.Count > 0)
        {
            _context.Notifications.RemoveRange(oldNotifications);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<PagedResult<Notification>> GetFilteredAsync(NotificationFilter filter)
    {
        var query = _context.Notifications
            .AsNoTracking()
            .Where(n => n.TenantId == filter.TenantId && n.UserId == filter.UserId);

        if (filter.UnreadOnly == true)
            query = query.Where(n => !n.IsRead);

        query = query.OrderByDescending(n => n.CreatedAt);

        var total = await query.CountAsync();
        var items = await query
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync();

        return new PagedResult<Notification>
        {
            Items = items,
            TotalCount = total,
            Page = filter.Page,
            PageSize = filter.PageSize
        };
    }
}