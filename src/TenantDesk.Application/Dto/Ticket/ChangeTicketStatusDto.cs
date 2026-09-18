using System.ComponentModel.DataAnnotations;
using TenantDesk.Domain.Enums;

namespace TenantDesk.Application.Dto.Ticket;

// Used with: PATCH /api/tickets/{id}/status
public record ChangeTicketStatusDto(
    [Required(ErrorMessage = "Ticket status is required.")]
    [EnumDataType(typeof(TicketStatus), ErrorMessage = "Invalid ticket status value.")]
    TicketStatus Status
);