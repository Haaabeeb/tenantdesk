
using TenantDesk.Application.Interfaces.Services;
using TenantDesk.Domain.Entities;

namespace TenantDesk.Application.Services;

public class BusinessTimeCalculator : IBusinessTimeCalculator
{
    private const int MaxDaySearchWindow = 365;

    public DateTime CalculateDueDate(
        DateTime startTime,
        int minutesToAllocate,
        IEnumerable<BusinessHour> businessHours)
    {
        if (minutesToAllocate <= 0)
            return startTime;

       
        var workingDaysMap = businessHours
            .Where(b => b.IsWorkingDay && b.OpenTime.HasValue && b.CloseTime.HasValue)
            .ToDictionary(b => b.DayOfWeek);

        
        if (workingDaysMap.Count == 0)
            return startTime.AddMinutes(minutesToAllocate);

        var currentPointer = startTime;
        var remainingTime = TimeSpan.FromMinutes(minutesToAllocate);

        for (var i = 0; i < MaxDaySearchWindow; i++)
        {
            if (!workingDaysMap.TryGetValue(currentPointer.DayOfWeek, out var todayHours) ||
                todayHours.OpenTime is not { } openTimeOnly ||
                todayHours.CloseTime is not { } closeTimeOnly)
            {
                // Non working day
                currentPointer = currentPointer.Date.AddDays(1);
                continue;
            }

            var openTime = openTimeOnly.ToTimeSpan();
            var closeTime = closeTimeOnly.ToTimeSpan();
            var currentTimeOfDay = currentPointer.TimeOfDay;

            // Snap pointer forward if current time is before opening time
            if (currentTimeOfDay < openTime)
            {
                currentPointer = currentPointer.Date.Add(openTime);
                currentTimeOfDay = openTime;
            }

            // If current time is on/after closing time move to next day
            if (currentTimeOfDay >= closeTime)
            {
                currentPointer = currentPointer.Date.AddDays(1);
                continue;
            }

            var availableTimeToday = closeTime - currentTimeOfDay;

            if (remainingTime <= availableTimeToday)
            {
                return currentPointer.Add(remainingTime);
            }

            remainingTime -= availableTimeToday;
            currentPointer = currentPointer.Date.AddDays(1);
        }

        throw new InvalidOperationException("Could not calculate SLA due date within the maximum search window (365 days). Check the tenant's business hour configuration.");
    }
}