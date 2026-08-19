
namespace TenantDesk.Application.DTOs.Tenants;

// Sent to: PUT /api/tenants/{tenantId}
public record UpdateTenantDto(
    string TenantName,
    string Subdomain
);