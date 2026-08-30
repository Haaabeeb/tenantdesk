
namespace TenantDesk.Application.Dto.Plan;

// Sent to: PATCH /api/plans/{planId}/rate-limit
public record ChangePlanRateLimitDto(
    int RateLimitPerMinute
);