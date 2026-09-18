using Microsoft.EntityFrameworkCore;
using TenantDesk.Application.Common;
using TenantDesk.Application.Interfaces.Persistence;
using TenantDesk.Application.Interfaces.Persistence.Filters;
using TenantDesk.Domain.Entities;
using TenantDesk.Infrastructure.Data;

namespace TenantDesk.Infrastructure.Repositories;

public class AttachmentRepository(AppDbContext _context) : IAttachementRepository
{
    public Task<Attachment?> GetByIdAsync(Guid id) =>
        _context.Attachments.FirstOrDefaultAsync(a => a.AttachmentId == id);

    public async Task<PagedResult<Attachment>> GetFilteredAsync(AttachmentFilter filter)
    {
        var query = _context.Attachments.Where(a => a.TenantId == filter.TenantId);

        if (filter.TicketId is not null)
            query = query.Where(a => a.TicketId == filter.TicketId);

        if (filter.CommentId is not null)
            query = query.Where(a => a.CommentId == filter.CommentId);

        if (filter.UploadedBy is not null)
            query = query.Where(a => a.UploadedBy == filter.UploadedBy);

        query = query.OrderByDescending(a => a.CreatedAt);

        var total = await query.CountAsync();
        var items = await query
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync();

        return new PagedResult<Attachment>
        {
            Items = items,
            TotalCount = total,
            Page = filter.Page,
            PageSize = filter.PageSize
        };
    }

    public async Task AddAsync(Attachment attachment)
    {
        await _context.Attachments.AddAsync(attachment);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var attachment = await GetByIdAsync(id);
        if (attachment is not null)
        {
            _context.Attachments.Remove(attachment);
            await _context.SaveChangesAsync();
        }
    }
}