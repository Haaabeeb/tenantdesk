
namespace TenantDesk.Application.DTOs.Plans;

// Sent to: PATCH /api/plans/{planId}/rate-limit
public record ChangePlanRateLimitDto(
    int RateLimitPerMinute
);