
namespace TenantDesk.Application.DTOs.BusinessHours;

// Sent to: PUT /api/business-hours/{businessHourId}
public record UpdateBusinessHourDto(
    TimeOnly? OpenTime,
    TimeOnly? CloseTime
);