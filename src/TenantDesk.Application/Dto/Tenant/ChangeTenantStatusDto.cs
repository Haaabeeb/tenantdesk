
namespace TenantDesk.Application.DTOs.Tenants;

// Sent to: PATCH /api/tenants/{tenantId}/status
public record ChangeTenantStatusDto(
    bool IsActive
);
