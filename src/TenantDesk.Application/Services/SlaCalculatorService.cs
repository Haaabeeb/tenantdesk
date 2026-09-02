using TenantDesk.Application.Interfaces;
using TenantDesk.Application.Interfaces.Services;
using TenantDesk.Domain.Entities;

namespace TenantDesk.Application.Services;

public class SlaCalculatorService : ISlaCalculatorService
{
    private readonly ITicketRepository _ticketRepository;
    private readonly ISlaRepository _slaRepository;
    private readonly IBusinessHourRepository _businessHourRepository;
    private readonly IDateTimeProvider _dateTimeProvider;

    private const int MaxDaySearchWindow = 365;

    public SlaCalculatorService(ITicketRepository ticketRepository,ISlaRepository slaRepository,IBusinessHourRepository businessHourRepository,IDateTimeProvider dateTimeProvider)
    {
        _ticketRepository = ticketRepository;
        _slaRepository = slaRepository;
        _businessHourRepository = businessHourRepository;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<DateTime> CalculateResponseDueAsync(Guid ticketId)
    {
        var ticket = await GetTicketOrThrowAsync(ticketId);
        var sla = await GetSlaOrThrowAsync(ticket);

        return await CalculateDueDateAsync(ticket, sla.ResponseTime);
    }

    public async Task<DateTime> CalculateResolutionDueAsync(Guid ticketId)
    {
        var ticket = await GetTicketOrThrowAsync(ticketId);
        return await CalculateResolutionDueInternalAsync(ticket);
    }

    public async Task<bool> IsBreachedAsync(Guid ticketId)
    {
        var ticket = await GetTicketOrThrowAsync(ticketId);
        var resolutionDue = await CalculateResolutionDueInternalAsync(ticket);

        var comparisonTime = ticket.Status == TicketStatus.Closed
            ? ticket.UpdatedAt
            : _dateTimeProvider.UtcNow;

        return comparisonTime > resolutionDue;
    }

    private async Task<DateTime> CalculateResolutionDueInternalAsync(Ticket ticket)
    {
        var sla = await GetSlaOrThrowAsync(ticket);
        return await CalculateDueDateAsync(ticket, sla.ResolutionTime);
    }

    private async Task<DateTime> CalculateDueDateAsync(Ticket ticket, int minutesToAllocate)
    {
        var businessHours = await _businessHourRepository.GetByTenantAsync(ticket.TenantId);

        var workingDaysMap = businessHours
            .Where(b => b.IsWorkingDay)
            .ToDictionary(b => b.DayOfWeek);

        if (workingDaysMap.Count == 0)
            return ticket.CreatedAt.AddMinutes(minutesToAllocate);

        var currentPointer = ticket.CreatedAt;
        var remainingTime = TimeSpan.FromMinutes(minutesToAllocate);

        for (var i = 0; i < MaxDaySearchWindow; i++)
        {
            if (!workingDaysMap.TryGetValue(currentPointer.DayOfWeek, out var todayHours))
            {
                currentPointer = currentPointer.Date.AddDays(1);
                continue;
            }

            var openTime = todayHours.OpenTime!.Value.ToTimeSpan();
            var closeTime = todayHours.CloseTime!.Value.ToTimeSpan();
            var currentTimeOfDay = currentPointer.TimeOfDay;

            if (currentTimeOfDay < openTime)
            {
                currentPointer = currentPointer.Date.Add(openTime);
                currentTimeOfDay = openTime;
            }

            if (currentTimeOfDay >= closeTime)
            {
                currentPointer = currentPointer.Date.AddDays(1);
                continue;
            }

            var availableTimeToday = closeTime - currentTimeOfDay;

            if (remainingTime <= availableTimeToday)
                return currentPointer.Add(remainingTime);

            remainingTime -= availableTimeToday;
            currentPointer = currentPointer.Date.AddDays(1);
        }

        throw new InvalidOperationException("Could not calculate SLA due date within the maximum search window (365 days). Check the tenant's business hour configuration.");
    }

    private async Task<Ticket> GetTicketOrThrowAsync(Guid ticketId)
    {
        var ticket = await _ticketRepository.GetByIdAsync(ticketId);
        if (ticket is null)
            throw new KeyNotFoundException($"Ticket '{ticketId}' not found.");

        return ticket;
    }

    private async Task<Sla> GetSlaOrThrowAsync(Ticket ticket)
    {
        var sla = await _slaRepository.GetByIdAsync(ticket.SlaId);
        if (sla is null)
            throw new KeyNotFoundException($"SLA '{ticket.SlaId}' referenced by ticket '{ticket.TicketId}' was not found.");

        return sla;
    }
    


}