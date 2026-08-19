
namespace TenantDesk.Application.DTOs.Tenants;

// Returned from: GET/POST/PUT /api/tenants/{tenantId}
public record TenantResponseDto(
    Guid TenantId,
    Guid PlanId,
    string TenantName,
    string Subdomain,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);