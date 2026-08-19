namespace TenantDesk.Application.Dto.Ticket;

// Used with: POST /api/tickets
// TenantId and CreatedBy are not included - taken from the authenticated user's context.
public record CreateTicketDto(
    Guid CategoryId,
    Guid SlaId,
    string Title,
    string? Description
);
