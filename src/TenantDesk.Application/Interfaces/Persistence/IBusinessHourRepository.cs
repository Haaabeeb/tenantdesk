
using TenantDesk.Domain.Entities;

namespace TenantDesk.Application.Interfaces;

public interface IBusinessHourRepository
{
    Task<IEnumerable<BusinessHour>> GetByTenantAsync(Guid tenantId);
    Task<BusinessHour?> GetByDayAsync(Guid tenantId, DayOfWeek day);
    Task<bool> IsWithinBusinessHoursAsync(Guid tenantId, DateTime time);
    Task AddAsync(BusinessHour hour);
    Task UpdateAsync(BusinessHour hour);
    Task DeleteAsync(Guid id);
}
