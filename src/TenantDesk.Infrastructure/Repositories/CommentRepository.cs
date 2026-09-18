using Microsoft.EntityFrameworkCore;
using TenantDesk.Application.Common;
using TenantDesk.Application.Interfaces.Persistence.Filters;
using TenantDesk.Domain.Entities;
using TenantDesk.Infrastructure.Data;

namespace TenantDesk.Infrastructure.Repositories;

public class CommentRepository(AppDbContext _context) : ICommentRepository
{
    public Task<Comment?> GetByIdAsync(Guid id) =>
        _context.Comments.FirstOrDefaultAsync(c => c.CommentId == id);

    public Task<int> CountByTicketAsync(Guid ticketId) =>
        _context.Comments.CountAsync(c => c.TicketId == ticketId);

    public async Task<PagedResult<Comment>> GetFilteredAsync(CommentFilter filter)
    {
        var query = _context.Comments.Where(c => c.TenantId == filter.TenantId);

        if (filter.TicketId is not null)
            query = query.Where(c => c.TicketId == filter.TicketId);

        if (filter.UserId is not null)
            query = query.Where(c => c.UserId == filter.UserId);

        query = query.OrderByDescending(c => c.CreatedAt);

        var total = await query.CountAsync();
        var items = await query
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync();

        return new PagedResult<Comment>
        {
            Items = items,
            TotalCount = total,
            Page = filter.Page,
            PageSize = filter.PageSize
        };
    }

    public async Task AddAsync(Comment comment)
    {
        await _context.Comments.AddAsync(comment);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Comment comment)
    {
        _context.Comments.Update(comment);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var comment = await GetByIdAsync(id);
        if (comment is not null)
        {
            _context.Comments.Remove(comment);
            await _context.SaveChangesAsync();
        }
    }
}