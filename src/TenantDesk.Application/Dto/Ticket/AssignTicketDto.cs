namespace TenantDesk.Application.Dto.Ticket;

// Used with: PATCH /api/tickets/{id}/assign
public record AssignTicketDto(
    Guid AssignedTo
);