
namespace TenantDesk.Application.Dto.BusinessHour;

// Sent to: PUT /api/business-hours/{businessHourId}
public record UpdateBusinessHourDto(
    TimeOnly? OpenTime,
    TimeOnly? CloseTime
);