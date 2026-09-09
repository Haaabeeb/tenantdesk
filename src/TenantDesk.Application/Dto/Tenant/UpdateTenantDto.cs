using System.ComponentModel.DataAnnotations;

namespace TenantDesk.Application.Dto.Tenant;

// Sent to: PUT /api/tenants/{tenantId}
public record UpdateTenantDto(
    [Required]
    [StringLength(100)]
    string TenantName,

    [Required]
    [StringLength(50)]
    string Subdomain
);