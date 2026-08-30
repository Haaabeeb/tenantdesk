using TenantDesk.Domain.Enums;

namespace TenantDesk.Application.Dto.Sla;

// Sent to: POST /api/tenants/{tenantId}/slas
public record CreateSlaDto(
    PriorityLevel Priority,
    int ResponseTime,
    int ResolutionTime
);

