
using TenantDesk.Application.Common;
using TenantDesk.Application.Dto.Attachment;
using TenantDesk.Application.Interfaces.Identity;
using TenantDesk.Application.Interfaces.Persistence;
using TenantDesk.Application.Interfaces.Persistence.Filters;
using TenantDesk.Application.Mapping;
using TenantDesk.Domain.Entities;
using TenantDesk.Domain.Enums;

namespace TenantDesk.Application.Services;

public class AttachmentService(
    IAttachementRepository _attachmentRepository,
    ITicketRepository _ticketRepository,
    IFileStorageService _fileStorageService,
    ICurrentTenantService _currentTenantService,
    ICurrentUserService _currentUserService)
{
        public async Task<AttachmentResponseDto> CreateAsync(Guid ticketId, CreateAttachmentDto dto, Stream fileStream)
    {
        await EnsureCanAccessTicketAsync(ticketId);

        var tenantId = _currentTenantService.GetTenantId();
        var userId = _currentUserService.GetUserId();

        var storagePath = $"{tenantId}/{ticketId}/{Guid.NewGuid()}_{dto.FileName}";

        await _fileStorageService.UploadAsync(fileStream, storagePath);

        var attachment = new Attachment(
            tenantId, ticketId, dto.CommentId, userId,
            storagePath, dto.FileName, dto.FileSize);

        await _attachmentRepository.AddAsync(attachment);

        return attachment.ToDto();
    }

    public async Task<PagedResult<AttachmentResponseDto>> GetByTicketAsync(Guid ticketId, AttachmentFilter filter)
    {
        await EnsureCanAccessTicketAsync(ticketId);

        filter.TenantId = _currentTenantService.GetTenantId();
        filter.TicketId = ticketId;

        var result = await _attachmentRepository.GetFilteredAsync(filter);

        return new PagedResult<AttachmentResponseDto>
        {
            Items = result.Items.Select(a => a.ToDto()).ToList(),
            TotalCount = result.TotalCount,
            Page = result.Page,
            PageSize = result.PageSize
        };
    }

    public async Task DeleteAsync(Guid id)
    {
        var attachment = await _attachmentRepository.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Attachment with ID '{id}' not found.");

        if (attachment.TenantId != _currentTenantService.GetTenantId())
            throw new UnauthorizedAccessException("You cannot access attachments from another tenant.");

        var userId = _currentUserService.GetUserId();
        if (attachment.UploadedBy != userId)
            throw new UnauthorizedAccessException("You can only delete attachments you uploaded.");

        await _fileStorageService.DeleteAsync(attachment.StoragePath);
        await _attachmentRepository.DeleteAsync(id);
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