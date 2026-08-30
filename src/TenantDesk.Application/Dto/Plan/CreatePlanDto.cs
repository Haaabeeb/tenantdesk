
namespace TenantDesk.Application.Dto.Plan;

// Sent to: POST /api/plans
public record CreatePlanDto(
    string Name,
    int RateLimitPerMinute
);

