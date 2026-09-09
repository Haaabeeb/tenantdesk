using System.ComponentModel.DataAnnotations;

namespace TenantDesk.Application.Dto.Tenant;


// Sent to: POST /api/tenants
public record CreateTenantDto(
    Guid PlanId,

    [Required]
    [StringLength(100)]
    string TenantName,

    [Required]
    [StringLength(50)]
    string Subdomain
);
