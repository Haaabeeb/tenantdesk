
namespace TenantDesk.Application.DTOs.Tenants;

// Sent to: POST /api/tenants
public record CreateTenantDto(
    Guid PlanId,
    string TenantName,
    string Subdomain
);
