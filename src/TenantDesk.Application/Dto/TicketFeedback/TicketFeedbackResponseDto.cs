namespace TenantDesk.Application.Dto.TicketFeedback;

// Returned from: GET/POST /api/tickets/{ticketId}/feedback
public record TicketFeedbackResponseDto(
    Guid TicketFeedbackId,
    Guid TicketId,
    int Rating,
    string? Comment,
    DateTime SubmittedAt
);
