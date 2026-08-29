
namespace TenantDesk.Application.Dto.Tenant;

// Sent to: PATCH /api/tenants/{tenantId}/status
public record ChangeTenantStatusDto(
    bool IsActive
);
