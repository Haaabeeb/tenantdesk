
namespace TenantDesk.Application.DTOs.Slas;

// Sent to: PUT /api/slas/{slaId}/times
public record UpdateSlaTimesDto(
    int ResponseTime,
    int ResolutionTime
);
