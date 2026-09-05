
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TenantDesk.Application.Common;
using TenantDesk.Application.Dto.Notification;
using TenantDesk.Application.Interfaces.Persistence.Filters;
using TenantDesk.Application.Services;

namespace TenantDesk.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NotificationsController(NotificationService notificationService) : ControllerBase
{
    private readonly NotificationService _notificationService = notificationService;

    // GET: api/notifications?unreadOnly=true&page=1&pageSize=20
    [HttpGet]
    public async Task<ActionResult<PagedResult<NotificationResponseDto>>> GetNotifications([FromQuery] NotificationFilter filter)
    {
        var result = await _notificationService.GetFilteredForCurrentUserAsync(filter);
        return Ok(result);
    }

    // GET: api/notifications/unread-count
    [HttpGet("unread-count")]
    public async Task<ActionResult<int>> GetUnreadCount()
    {
        var count = await _notificationService.GetUnreadCountForCurrentUserAsync();
        return Ok(new { unreadCount = count });
    }

    // PUT: api/notifications/{id}/read
    [HttpPut("{id:guid}/read")]
    public async Task<IActionResult> MarkAsRead(Guid id)
    {
        await _notificationService.MarkAsReadAsync(id);
        return NoContent();
    }

    // PUT: api/notifications/read-all
    [HttpPut("read-all")]
    public async Task<IActionResult> MarkAllAsRead()
    {
        await _notificationService.MarkAllAsReadForCurrentUserAsync();
        return NoContent();
    }


}