using TenantDesk.Domain.Enums;

namespace TenantDesk.Application.DTOs.Slas;

// Sent to: POST /api/tenants/{tenantId}/slas
public record CreateSlaDto(
    PriorityLevel Priority,
    int ResponseTime,
    int ResolutionTime
);

