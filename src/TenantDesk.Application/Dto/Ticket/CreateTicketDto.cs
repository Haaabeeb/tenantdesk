using System.ComponentModel.DataAnnotations;

namespace TenantDesk.Application.Dto.Ticket;

// Used with: POST /api/tickets
// TenantId and CreatedBy are not included - taken from the authenticated user's context.
public record CreateTicketDto(
    [Required(ErrorMessage = "Category ID is required.")]
    Guid CategoryId,

    [Required(ErrorMessage = "SLA ID is required.")]
    Guid SlaId,

    [Required(ErrorMessage = "Title is required.")]
    [StringLength(200, MinimumLength = 3, ErrorMessage = "Title must be between 3 and 200 characters.")]
    string Title,

    [StringLength(5000, ErrorMessage = "Description cannot exceed 5000 characters.")]
    string? Description
);
