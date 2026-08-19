
namespace TenantDesk.Application.DTOs.BusinessHours;

// Sent to: POST /api/tenants/{tenantId}/business-hours
public record CreateBusinessHourDto(
    DayOfWeek DayOfWeek,
    TimeOnly? OpenTime,
    TimeOnly? CloseTime
);