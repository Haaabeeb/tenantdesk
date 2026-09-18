using System.ComponentModel.DataAnnotations;

namespace TenantDesk.Application.Dto.Ticket;

// Used with: PATCH /api/tickets/{id}/assign
public record AssignTicketDto(
    [Required(ErrorMessage = "Assigned user ID is required.")]
    Guid AssignedTo
);