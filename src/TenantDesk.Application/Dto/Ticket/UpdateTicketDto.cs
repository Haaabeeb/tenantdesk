namespace TenantDesk.Application.Dto.Ticket;

// Used with: PUT /api/tickets/{ticketId}
public record UpdateTicketDto(
    string Title,
    string? Description,
    TicketStatus Status,
    Guid? AssignedTo
);
