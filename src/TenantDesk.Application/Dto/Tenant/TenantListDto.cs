
namespace TenantDesk.Application.Dto.Tenant;

// Returned from: GET /api/tenants
public record TenantListDto(
    Guid TenantId,
    string TenantName,
    string Subdomain,
    bool IsActive
);