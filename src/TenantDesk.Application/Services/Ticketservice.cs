using AutoMapper;
using TenantDesk.Application.Common;
using TenantDesk.Application.Dto.Ticket;
using TenantDesk.Application.Interfaces;
using TenantDesk.Application.Interfaces.Identity;
using TenantDesk.Application.Interfaces.Persistence.Filters;
using TenantDesk.Application.Interfaces.Services;
using TenantDesk.Domain.Entities;
using TenantDesk.Domain.Enums;

namespace TenantDesk.Application.Services;

public class TicketService(
    ITicketRepository _ticketRepository,
    ICategoryRepository _categoryRepository,
    ISlaRepository _slaRepository,
    ICurrentTenantService _currentTenantService,
    ICurrentUserService _currentUserService,
    INotificationDispatcher _notificationDispatcher,
    IMapper _mapper)
{
    public async Task<TicketResponseDto> CreateAsync(CreateTicketDto dto)
    {
        var tenantId = _currentTenantService.GetTenantId();
        var userId = _currentUserService.GetUserId();

        var category = await _categoryRepository.GetByIdAsync(dto.CategoryId)
            ?? throw new KeyNotFoundException($"Category with ID '{dto.CategoryId}' not found.");

        if (category.TenantId != tenantId)
            throw new UnauthorizedAccessException("You cannot use a category from another tenant.");

        var sla = await _slaRepository.GetByIdAsync(dto.SlaId)
            ?? throw new KeyNotFoundException($"SLA with ID '{dto.SlaId}' not found.");

        if (sla.TenantId != tenantId)
            throw new UnauthorizedAccessException("You cannot use an SLA from another tenant.");

        var ticket = new Ticket(tenantId, dto.CategoryId, dto.SlaId, userId, dto.Title, dto.Description);

        await _ticketRepository.AddAsync(ticket);

        return _mapper.Map<TicketResponseDto>(ticket);
    }

    public async Task<TicketResponseDto> GetByIdAsync(Guid id)
    {
        var ticket = await GetOwnedTicketAsync(id);

        return _mapper.Map<TicketResponseDto>(ticket);
    }

    public async Task<PagedResult<TicketResponseDto>> GetFilteredAsync(TicketFilter filter)
    {
        filter.TenantId = _currentTenantService.GetTenantId();

        // Customers only ever see their own tickets, regardless of what they pass in the query.
        if (_currentUserService.GetUserRole() == UserRole.Customer)
            filter.CreatedBy = _currentUserService.GetUserId();

        var result = await _ticketRepository.GetFilteredAsync(filter);

        return new PagedResult<TicketResponseDto>
        {
            Items = result.Items.Select(t => _mapper.Map<TicketResponseDto>(t)).ToList(),
            TotalCount = result.TotalCount,
            Page = result.Page,
            PageSize = result.PageSize
        };
    }

    public async Task<TicketResponseDto> UpdateDetailsAsync(Guid id, UpdateTicketDto dto)
    {
        var ticket = await GetOwnedTicketAsync(id, agentOrAdminOnly: true);

        ticket.UpdateDetails(dto.Title, dto.Description);

        await _ticketRepository.UpdateAsync(ticket);

        await _notificationDispatcher.DispatchAsync(
            ticket.CreatedBy, NotificationType.TicketUpdated, $"Your ticket '{ticket.Title}' was updated.", ticket.TicketId);

        return _mapper.Map<TicketResponseDto>(ticket);
    }

    public async Task<TicketResponseDto> ChangeStatusAsync(Guid id, TicketStatus newStatus)
    {
        var ticket = await GetOwnedTicketAsync(id, agentOrAdminOnly: true);

        ticket.ChangeStatus(newStatus);

        await _ticketRepository.UpdateAsync(ticket);

        await _notificationDispatcher.DispatchAsync(
            ticket.CreatedBy, NotificationType.TicketUpdated, $"Your ticket '{ticket.Title}' status changed to {newStatus}.", ticket.TicketId);

        return _mapper.Map<TicketResponseDto>(ticket);
    }

    public async Task<TicketResponseDto> AssignAsync(Guid id, Guid assigneeUserId)
    {
        var ticket = await GetOwnedTicketAsync(id, agentOrAdminOnly: true);

        ticket.AssignTo(assigneeUserId);

        await _ticketRepository.UpdateAsync(ticket);

        await _notificationDispatcher.DispatchAsync(
            assigneeUserId, NotificationType.TicketAssigned, $"You have been assigned ticket '{ticket.Title}'.", ticket.TicketId);

        return _mapper.Map<TicketResponseDto>(ticket);
    }

    public async Task<TicketResponseDto> CloseAsync(Guid id)
    {
        // Agent/Admin can close any ticket assigned to them; a Customer can only close their own.
        var ticket = await GetOwnedTicketAsync(id);

        ticket.Close();

        await _ticketRepository.UpdateAsync(ticket);

        await _notificationDispatcher.DispatchAsync(
            ticket.CreatedBy, NotificationType.TicketUpdated, $"Your ticket '{ticket.Title}' was closed.", ticket.TicketId);

        return _mapper.Map<TicketResponseDto>(ticket);
    }

    private async Task<Ticket> GetOwnedTicketAsync(Guid id, bool agentOrAdminOnly = false)
    {
        var ticket = await _ticketRepository.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Ticket with ID '{id}' not found.");

        if (ticket.TenantId != _currentTenantService.GetTenantId())
            throw new UnauthorizedAccessException("You cannot access tickets from another tenant.");

        var role = _currentUserService.GetUserRole();
        var userId = _currentUserService.GetUserId();

        if (role == UserRole.Customer)
        {
            if (ticket.CreatedBy != userId)
                throw new UnauthorizedAccessException("You cannot access a ticket you did not create.");

            if (agentOrAdminOnly)
                throw new UnauthorizedAccessException("Only an agent or admin can perform this action.");
        }
        else if (role == UserRole.Agent && agentOrAdminOnly)
        {
            // Agent can only modify tickets assigned to them; Admin is exempt from this check entirely.
            if (ticket.AssignedTo != userId)
                throw new UnauthorizedAccessException("You can only modify tickets assigned to you.");
        }

        return ticket;
    }
}