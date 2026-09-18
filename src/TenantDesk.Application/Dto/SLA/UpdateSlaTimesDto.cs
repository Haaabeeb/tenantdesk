using System.ComponentModel.DataAnnotations;

namespace TenantDesk.Application.Dto.Sla;

// Sent to: PUT /api/slas/{slaId}/times
public record UpdateSlaTimesDto(
    [Range(1, int.MaxValue, ErrorMessage = "ResponseTime must be greater than zero.")]
    int ResponseTime,

    [Range(1, int.MaxValue, ErrorMessage = "ResolutionTime must be greater than zero.")]
    int ResolutionTime
);