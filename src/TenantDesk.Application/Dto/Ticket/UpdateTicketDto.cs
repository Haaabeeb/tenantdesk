using System.ComponentModel.DataAnnotations;

namespace TenantDesk.Application.Dto.Ticket;

// Used with: PUT /api/tickets/{id}
public record UpdateTicketDto(
    [Required(ErrorMessage = "Title is required.")]
    [StringLength(200, MinimumLength = 3, ErrorMessage = "Title must be between 3 and 200 characters.")]
    string Title,

    [StringLength(5000, ErrorMessage = "Description cannot exceed 5000 characters.")]
    string? Description
);