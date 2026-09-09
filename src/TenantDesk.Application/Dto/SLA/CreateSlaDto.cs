using System.ComponentModel.DataAnnotations;
using TenantDesk.Domain.Enums;

namespace TenantDesk.Application.Dto.Sla;

// Sent to: POST /api/tenants/{tenantId}/slas
public record CreateSlaDto(
    PriorityLevel Priority,

    [Range(1, int.MaxValue, ErrorMessage = "ResponseTime must be greater than zero.")]
    int ResponseTime,

    [Range(1, int.MaxValue, ErrorMessage = "ResolutionTime must be greater than zero.")]
    int ResolutionTime
);

