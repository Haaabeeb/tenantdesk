
namespace TenantDesk.Application.Dto.Tenant;

// Sent to: PATCH /api/tenants/{tenantId}/plan
public record ChangeTenantPlanDto(
    Guid PlanId
);
