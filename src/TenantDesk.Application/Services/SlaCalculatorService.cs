using TenantDesk.Application.Interfaces;
using TenantDesk.Application.Interfaces.Services;
using TenantDesk.Domain.Entities;

namespace TenantDesk.Application.Services;

public class SlaCalculatorService : ISlaCalculatorService
{
    private readonly ITicketRepository _ticketRepository;
    private readonly ISlaRepository _slaRepository;
    private readonly IBusinessHourRepository _businessHourRepository;
    private readonly IBusinessTimeCalculator _businessTimeCalculator;
    private readonly IDateTimeProvider _dateTimeProvider;

    public SlaCalculatorService(ITicketRepository ticketRepository,ISlaRepository slaRepository,IBusinessHourRepository businessHourRepository,IBusinessTimeCalculator businessTimeCalculator,IDateTimeProvider dateTimeProvider)
    {
        _ticketRepository = ticketRepository;
        _slaRepository = slaRepository;
        _businessHourRepository = businessHourRepository;
        _businessTimeCalculator = businessTimeCalculator;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<DateTime> CalculateResponseDueAsync(Guid ticketId)
    {
        var ticket = await GetTicketOrThrowAsync(ticketId);
        return await CalculateResponseDueInternalAsync(ticket);
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


    public async Task<bool> IsResponseBreachedAsync(Guid ticketId)
    {
        var ticket = await GetTicketOrThrowAsync(ticketId);
        var responseDue = await CalculateResponseDueInternalAsync(ticket);

        var comparisonTime = ticket.Status == TicketStatus.Open
            ? _dateTimeProvider.UtcNow
            : ticket.UpdatedAt;

        return comparisonTime > responseDue;
    }

    private async Task<DateTime> CalculateResponseDueInternalAsync(Ticket ticket)
    {
        var sla = await GetSlaOrThrowAsync(ticket);
        var businessHours = await _businessHourRepository.GetByTenantAsync(ticket.TenantId);

        return _businessTimeCalculator.CalculateDueDate(
            ticket.CreatedAt,
            sla.ResponseTime,
            businessHours);
    }

    private async Task<DateTime> CalculateResolutionDueInternalAsync(Ticket ticket)
    {
        var sla = await GetSlaOrThrowAsync(ticket);
        var businessHours = await _businessHourRepository.GetByTenantAsync(ticket.TenantId);

        return _businessTimeCalculator.CalculateDueDate(
            ticket.CreatedAt,
            sla.ResolutionTime,
            businessHours);
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

        if (sla.TenantId != ticket.TenantId)
            throw new InvalidOperationException($"SLA '{sla.SlaId}' does not belong to the same tenant as Ticket '{ticket.TicketId}'.");

        return sla;
    }


}