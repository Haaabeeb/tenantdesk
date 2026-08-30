using TenantDesk.Domain.Enums;

namespace TenantDesk.Application.Dto.Sla;

// Returned from: GET/POST/PUT /api/slas/{slaId}
public record SlaResponseDto(
    Guid SlaId,
    PriorityLevel Priority,
    int ResponseTime,
    int ResolutionTime,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);
