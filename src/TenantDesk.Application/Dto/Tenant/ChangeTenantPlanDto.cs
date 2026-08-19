
namespace TenantDesk.Application.DTOs.Tenants;

// Sent to: PATCH /api/tenants/{tenantId}/plan
public record ChangeTenantPlanDto(
    Guid PlanId
);
