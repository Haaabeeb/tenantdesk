
using TenantDesk.Domain.Entities;

namespace TenantDesk.Application.Interfaces.Services;

public interface IBusinessTimeCalculator
{
    DateTime CalculateDueDate(
        DateTime startTime,
        int minutesToAllocate,
        IEnumerable<BusinessHour> businessHours);
}