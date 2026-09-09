using System.ComponentModel.DataAnnotations;

namespace TenantDesk.Application.Dto.Plan;

// Sent to: PATCH /api/plans/{planId}/rate-limit
public record ChangePlanRateLimitDto(
    [Range(1, int.MaxValue, ErrorMessage = "RateLimitPerMinute must be greater than zero.")]
    int RateLimitPerMinute
);