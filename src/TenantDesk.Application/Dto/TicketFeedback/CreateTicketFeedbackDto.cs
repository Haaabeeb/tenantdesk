namespace TenantDesk.Application.Dto.TicketFeedback;

// Used with: POST /api/tickets/{ticketId}/feedback
// TicketId comes from the route. Rating must be validated 1-5 (FluentValidation).
public record CreateTicketFeedbackDto(
    int Rating,
    string? Comment
);
