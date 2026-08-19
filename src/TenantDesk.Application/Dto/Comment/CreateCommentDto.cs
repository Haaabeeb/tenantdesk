namespace TenantDesk.Application.Dto.Comment;

// Used with: POST /api/tickets/{ticketId}/comments
// TicketId comes from the route; UserId comes from the authenticated user's context.
public record CreateCommentDto(
    string Content
);
