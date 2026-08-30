
namespace TenantDesk.Application.Dto.BusinessHour;

// Sent to: POST /api/tenants/{tenantId}/business-hours
public record CreateBusinessHourDto(
    DayOfWeek DayOfWeek,
    TimeOnly? OpenTime,
    TimeOnly? CloseTime
);
