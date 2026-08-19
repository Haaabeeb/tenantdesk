
namespace TenantDesk.Application.DTOs.BusinessHours;

// Returned from: GET/POST/PUT /api/business-hours/{businessHourId}
public record BusinessHourResponseDto(
    Guid BusinessHourId,
    DayOfWeek DayOfWeek,
    TimeOnly? OpenTime,
    TimeOnly? CloseTime,
    bool IsWorkingDay
);
