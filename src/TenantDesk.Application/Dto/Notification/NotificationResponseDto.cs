
using TenantDesk.Domain.Enums;

namespace TenantDesk.Application.Dto.Notification;

// Returned from: GET /api/users/{userId}/notifications
public record NotificationResponseDto(
    Guid NotificationId,
    Guid UserId,
    Guid? TicketId,
    NotificationType Type,
    string Message,
    bool IsRead,
    DateTime CreatedAt
);