using System;

namespace TenantDesk.Domain.Entities;

public class BusinessHour
{
    public Guid BusinessHourId { get; private set; }
    public Guid TenantId { get; private set; }
    public DayOfWeek DayOfWeek { get; private set; } 
    public TimeOnly? OpenTime { get; private set; } 
    public TimeOnly? CloseTime { get; private set; }
   
    public Tenant Tenant { get; private set; } = null!;

    public bool IsWorkingDay => OpenTime.HasValue && CloseTime.HasValue;

    private BusinessHour() { }

    public BusinessHour(Guid tenantId, DayOfWeek dayOfWeek, TimeOnly? openTime, TimeOnly? closeTime)
    {
        if (tenantId == Guid.Empty)
            throw new ArgumentException("Tenant ID cannot be empty.", nameof(tenantId));

        ValidateTimes(openTime, closeTime);
        TenantId = tenantId;
        DayOfWeek = dayOfWeek;
        OpenTime = openTime;
        CloseTime = closeTime;
        
    }

    public void UpdateHours(TimeOnly? openTime, TimeOnly? closeTime)
    {
        ValidateTimes(openTime, closeTime);
        OpenTime = openTime;
        CloseTime = closeTime;
    }

    private static void ValidateTimes(TimeOnly? openTime, TimeOnly? closeTime)
    {

        if (openTime.HasValue != closeTime.HasValue)
            throw new ArgumentException("Both open and close time must be provided together, or both left empty for a day off.");

        if (openTime.HasValue && closeTime.HasValue && openTime >= closeTime)
            throw new ArgumentException("Open time must be strictly before close time.");
    }
}