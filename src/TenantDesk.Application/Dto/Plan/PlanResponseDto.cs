
namespace TenantDesk.Application.DTOs.Plans;

// Returned from: GET/POST /api/plans
public record PlanResponseDto(
    Guid PlanId,
    string Name,
    int RateLimitPerMinute,
    DateTime CreatedAt
);