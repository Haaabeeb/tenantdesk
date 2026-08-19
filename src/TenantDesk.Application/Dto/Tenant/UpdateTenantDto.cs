namespace TenantDesk.Application.Dto.Tenant;

public record UpdateTenantDto(
    string TenantName,
    string Subdomain,
    bool IsActive
);