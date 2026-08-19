namespace TenantDesk.Application.Dto.Comment;

// Returned from: GET/POST /api/tickets/{ticketId}/comments
public record CommentResponseDto(
    Guid CommentId,
    Guid TicketId,
    Guid UserId,
    string Content,
    DateTime CreatedAt
);
