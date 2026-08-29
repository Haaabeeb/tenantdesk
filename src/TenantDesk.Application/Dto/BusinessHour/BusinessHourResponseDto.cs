
namespace TenantDesk.Application.Dto.BusinessHour;

// Returned from: GET/POST/PUT /api/business-hours/{businessHourId}
public record BusinessHourResponseDto(
    Guid BusinessHourId,
    DayOfWeek DayOfWeek,
    TimeOnly? OpenTime,
    TimeOnly? CloseTime,
    bool IsWorkingDay
);
