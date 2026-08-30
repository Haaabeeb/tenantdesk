
namespace TenantDesk.Application.Dto.Sla;

// Sent to: PUT /api/slas/{slaId}/times
public record UpdateSlaTimesDto(
    int ResponseTime,
    int ResolutionTime
);
