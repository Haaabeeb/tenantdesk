
namespace TenantDesk.Application.Dto.Tenant;


// Sent to: POST /api/tenants
public record CreateTenantDto(
    Guid PlanId,
    string TenantName,
    string Subdomain
);
