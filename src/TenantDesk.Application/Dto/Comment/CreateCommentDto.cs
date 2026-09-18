using System.ComponentModel.DataAnnotations;

namespace TenantDesk.Application.Dto.Comment;

// Used with: POST /api/tickets/{ticketId}/comments
// TicketId comes from the route; UserId comes from the authenticated user's context.
public record CreateCommentDto(
    [Required(ErrorMessage = "Comment content is required.")]
    [StringLength(4000, MinimumLength = 1, ErrorMessage = "Comment content cannot exceed 4000 characters.")]
    string Content
);