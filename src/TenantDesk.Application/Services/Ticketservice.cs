using AutoMapper;
using Microsoft.Extensions.Logging;
using TenantDesk.Application.Common;
using TenantDesk.Application.Dto.Ticket;
using TenantDesk.Application.Interfaces;
using TenantDesk.Application.Interfaces.Identity;
using TenantDesk.Application.Interfaces.Persistence.Filters;
using TenantDesk.Application.Interfaces.Services;
using TenantDesk.Application.Models;
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
    IUserRepository _userRepository,
    IEmailJobQueue _emailJobQueue,
    ILogger<TicketService> _logger,
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

        // --- Step 1: Persist an in-app notification for the ticket creator ---
        await _notificationDispatcher.DispatchAsync(
            userId,
            NotificationType.TicketCreated,
            $"Your ticket '{ticket.Title}' has been created successfully.",
            ticket.TicketId);

        // --- Step 2: Resolve creator email and enqueue an email delivery job ---
        // We look up the user record directly so the job carries a concrete email
        // address rather than relying on any HTTP-request-scoped context.
        var creator = await _userRepository.GetByIdAsync(userId);
        if (creator is not null)
        {
            var emailBody = string.Concat(
                "<p>Hello,</p>",
                "<p>Thank you for contacting us. Your ticket has been received and is now open.</p>",
                $"<p><strong>Ticket:</strong> {ticket.Title}</p>",
                "<p>We will get back to you as soon as possible.</p>",
                "<p>TenantDesk Support</p>");

            var emailJob = new EmailJob
            {
                TenantId = tenantId,
                TicketId = ticket.TicketId,
                To       = creator.Email,
                Subject  = $"Your ticket has been created: {ticket.Title}",
                Body     = emailBody
            };

            await _emailJobQueue.EnqueueAsync(emailJob);

            _logger.LogInformation(
                "Email job enqueued for TicketId: {TicketId}, TenantId: {TenantId}, To: {To}",
                ticket.TicketId, tenantId, creator.Email);
        }
        else
        {
            // Creator not found in DB — unusual, but we must not crash the request.
            _logger.LogWarning(
                "Could not find creator user {UserId} for TicketId: {TicketId}. Email job was NOT enqueued.",
                userId, ticket.TicketId);
        }

        // --- Step 3: Return HTTP response immediately (email is sent asynchronously) ---
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