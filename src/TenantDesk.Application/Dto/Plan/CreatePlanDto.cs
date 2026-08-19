
namespace TenantDesk.Application.DTOs.Plans;

// Sent to: POST /api/plans
public record CreatePlanDto(
    string Name,
    int RateLimitPerMinute
);

