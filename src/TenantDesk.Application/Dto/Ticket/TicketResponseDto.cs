namespace TenantDesk.Application.Dto.Ticket;

// Returned from: GET/POST/PUT /api/tickets
public record TicketResponseDto(
    Guid TicketId,
    Guid TenantId,
    Guid CategoryId,
    Guid SlaId,
    Guid CreatedBy,
    Guid? AssignedTo,
    string Title,
    string? Description,
    TicketStatus Status,
    DateTime CreatedAt,
    DateTime UpdatedAt
);
