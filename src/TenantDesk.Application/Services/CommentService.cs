using TenantDesk.Application.Common;
using TenantDesk.Application.Dto.Comment;
using TenantDesk.Application.Interfaces.Identity;
using TenantDesk.Application.Interfaces.Persistence.Filters;
using TenantDesk.Application.Mapping;
using TenantDesk.Domain.Entities;
using TenantDesk.Domain.Enums;

namespace TenantDesk.Application.Services;

public class CommentService(
    ICommentRepository _commentRepository,
    ITicketRepository _ticketRepository,
    ICurrentTenantService _currentTenantService,
    ICurrentUserService _currentUserService)
{
    public async Task<CommentResponseDto> CreateAsync(Guid ticketId, CreateCommentDto dto)
    {
        await EnsureCanAccessTicketAsync(ticketId);

        var tenantId = _currentTenantService.GetTenantId();
        var userId = _currentUserService.GetUserId();

        var comment = new Comment(tenantId, ticketId, userId, dto.Content);

        await _commentRepository.AddAsync(comment);

        return comment.ToDto();
    }

    public async Task<PagedResult<CommentResponseDto>> GetByTicketAsync(Guid ticketId, CommentFilter filter)
    {
        await EnsureCanAccessTicketAsync(ticketId);

        filter.TenantId = _currentTenantService.GetTenantId();
        filter.TicketId = ticketId;

        var result = await _commentRepository.GetFilteredAsync(filter);

        return new PagedResult<CommentResponseDto>
        {
            Items = result.Items.Select(c => c.ToDto()).ToList(),
            TotalCount = result.TotalCount,
            Page = result.Page,
            PageSize = result.PageSize
        };
    }

    public async Task<CommentResponseDto> UpdateAsync(Guid id, UpdateCommentDto dto)
    {
        var comment = await GetOwnedCommentAsync(id);

        var userId = _currentUserService.GetUserId();
        if (comment.UserId != userId)
            throw new UnauthorizedAccessException("You can only edit your own comments.");

        comment.UpdateContent(dto.Content);

        await _commentRepository.UpdateAsync(comment);

        return comment.ToDto();
    }

    public async Task DeleteAsync(Guid id)
    {
        var comment = await GetOwnedCommentAsync(id);

        var userId = _currentUserService.GetUserId();
        var role = _currentUserService.GetUserRole();

        if (comment.UserId != userId && role != UserRole.Admin)
            throw new UnauthorizedAccessException("You can only delete your own comments.");

        await _commentRepository.DeleteAsync(id);
    }

    
    private async Task<Comment> GetOwnedCommentAsync(Guid id)
    {
        var comment = await _commentRepository.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Comment with ID '{id}' not found.");

        if (comment.TenantId != _currentTenantService.GetTenantId())
            throw new UnauthorizedAccessException("You cannot access comments from another tenant.");

        return comment;
    }


    private async Task EnsureCanAccessTicketAsync(Guid ticketId)
    {
        var ticket = await _ticketRepository.GetByIdAsync(ticketId)
            ?? throw new KeyNotFoundException($"Ticket with ID '{ticketId}' not found.");

        if (ticket.TenantId != _currentTenantService.GetTenantId())
            throw new UnauthorizedAccessException("You cannot access tickets from another tenant.");

        var role = _currentUserService.GetUserRole();
        var userId = _currentUserService.GetUserId();

        if (role == UserRole.Customer && ticket.CreatedBy != userId)
            throw new UnauthorizedAccessException("You cannot access a ticket you did not create.");

        if (role == UserRole.Agent && ticket.AssignedTo != userId)
            throw new UnauthorizedAccessException("You can only access tickets assigned to you.");

        // Admin: no additional restriction.
    }
}