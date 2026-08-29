
namespace TenantDesk.Application.Dto.Plan;

// Returned from: GET/POST /api/plans
public record PlanResponseDto(
    Guid PlanId,
    string Name,
    int RateLimitPerMinute,
    DateTime CreatedAt
);