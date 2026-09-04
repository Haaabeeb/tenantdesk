namespace TenantDesk.Application.Dto.Ticket;

// Used with: PUT /api/tickets/{id}
public record UpdateTicketDto(
    string Title,
    string? Description
);