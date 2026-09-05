
using System.Diagnostics.CodeAnalysis;
using AutoMapper;
using TenantDesk.Application.Common;
using TenantDesk.Application.Dto.Notification;
using TenantDesk.Application.Interfaces;
using TenantDesk.Application.Interfaces.Identity;
using TenantDesk.Application.Interfaces.Persistence.Filters;
using TenantDesk.Domain.Entities;

namespace TenantDesk.Application.Services;

public class NotificationService(
    INotificationRepository notificationRepository,
    ICurrentTenantService currentTenantService,
    ICurrentUserService currentUserService,
    IMapper mapper)
{
    private readonly INotificationRepository _notificationRepository = notificationRepository;
    private readonly ICurrentTenantService _currentTenantService = currentTenantService;
    private readonly ICurrentUserService _currentUserService = currentUserService;
    private readonly IMapper _mapper = mapper;

    public async Task<PagedResult<NotificationResponseDto>> GetFilteredForCurrentUserAsync(NotificationFilter filter)
    {
        
        filter.TenantId = _currentTenantService.GetTenantId();
        filter.UserId = _currentUserService.GetUserId();

        var result = await _notificationRepository.GetFilteredAsync(filter);

        return new PagedResult<NotificationResponseDto>
        {
            Items = _mapper.Map<List<NotificationResponseDto>>(result.Items),
            TotalCount = result.TotalCount,
            Page = result.Page,
            PageSize = result.PageSize
        };
    }

    public async Task<int> GetUnreadCountForCurrentUserAsync()
    {
        var userId = _currentUserService.GetUserId();
        return await _notificationRepository.CountUnreadByUserAsync(userId);
    }

    public async Task MarkAsReadAsync(Guid notificationId)
    {
        var notification = await _notificationRepository.GetByIdAsync(notificationId);
        EnsureExistsAndOwned(notification);

        await _notificationRepository.MarkAsReadAsync(notificationId);
    }

    public async Task MarkAllAsReadForCurrentUserAsync()
    {
        var userId = _currentUserService.GetUserId();
        await _notificationRepository.MarkAllAsReadAsync(userId);
    }

    public async Task DeleteOldNotificationsAsync(Guid tenantId, DateTime olderThan)
    {
        await _notificationRepository.DeleteOldNotificationsAsync(tenantId, olderThan);
    }

    private void EnsureExistsAndOwned([NotNull] Notification? notification)
    {
        if (notification is null)
            throw new KeyNotFoundException("Notification not found.");

        if (notification.TenantId != _currentTenantService.GetTenantId() ||
            notification.UserId != _currentUserService.GetUserId())
        {
            throw new UnauthorizedAccessException("You cannot access notifications belonging to another user.");
        }
    }
}