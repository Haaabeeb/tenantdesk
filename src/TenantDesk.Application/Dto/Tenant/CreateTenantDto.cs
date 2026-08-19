
namespace TenantDesk.Application.Dto.Tenant;
 
public record CreateTenantDto(
    Guid PlanId,
    string TenantName,
    string Subdomain
);
 