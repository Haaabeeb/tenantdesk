using System.ComponentModel.DataAnnotations;

namespace TenantDesk.Application.Dto.Plan;

// Sent to: POST /api/plans
public record CreatePlanDto(
    [Required]
    [StringLength(50, MinimumLength = 2)]
    string Name,
    [Range(1, int.MaxValue, ErrorMessage = "RateLimitPerMinute must be greater than zero.")]
    int RateLimitPerMinute
);