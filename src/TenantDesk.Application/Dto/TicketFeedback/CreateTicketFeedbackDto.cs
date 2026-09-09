using System.ComponentModel.DataAnnotations;

namespace TenantDesk.Application.Dto.TicketFeedback;

// Used with: POST /api/tickets/{ticketId}/feedback
// TicketId comes from the route. Rating must be between 1 and 5.
public record CreateTicketFeedbackDto(
    [Required(ErrorMessage = "Rating is required.")]
    [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5.")]
    int Rating,

    [StringLength(1000, ErrorMessage = "Feedback comment cannot exceed 1000 characters.")]
    string? Comment
);
