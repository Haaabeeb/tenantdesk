namespace TenantDesk.Application.Dto.Tenant;
 
public record TenantResponseDto(
    Guid TenantId,
    Guid PlanId,
    string TenantName,
    string Subdomain,
    bool IsActive,
    DateTime CreatedAt,
    DateTime UpdatedAt
);
 