
namespace TenantDesk.Application.Dto.Tenant;

// Sent to: PUT /api/tenants/{tenantId}
public record UpdateTenantDto(
    string TenantName,
    string Subdomain
);