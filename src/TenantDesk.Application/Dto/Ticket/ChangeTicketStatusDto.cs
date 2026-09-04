namespace TenantDesk.Application.Dto.Ticket;

// Used with: PATCH /api/tickets/{id}/status
public record ChangeTicketStatusDto(
    TicketStatus Status
);