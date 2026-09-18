using Microsoft.EntityFrameworkCore;
using TenantDesk.Application.Common;
using TenantDesk.Application.Interfaces.Persistence.Filters;
using TenantDesk.Domain.Entities;
using TenantDesk.Domain.Enums;
using TenantDesk.Infrastructure.Data;

namespace TenantDesk.Infrastructure.Repositories;

public class TicketRepository(AppDbContext _context) : ITicketRepository
{
    public Task<Ticket?> GetByIdAsync(Guid id) =>
        _context.Tickets.FirstOrDefaultAsync(t => t.TicketId == id);

    public Task<bool> ExistsAsync(Guid id) =>
        _context.Tickets.AnyAsync(t => t.TicketId == id);

    public Task<int> CountByStatusAsync(Guid tenantId, TicketStatus status) =>
        _context.Tickets.CountAsync(t => t.TenantId == tenantId && t.Status == status);

    public Task<int> CountAssignedToUserAsync(Guid userId) =>
        _context.Tickets.CountAsync(t => t.AssignedTo == userId);

    public Task<int> CountByTenantAsync(Guid tenantId) =>
        _context.Tickets.CountAsync(t => t.TenantId == tenantId);

    public async Task<PagedResult<Ticket>> GetFilteredAsync(TicketFilter filter)
    {
        var query = _context.Tickets.Where(t => t.TenantId == filter.TenantId);

        if (filter.Status is not null)
            query = query.Where(t => t.Status == filter.Status);

        if (filter.Priority is not null)
            query = query.Where(t => t.Sla.Priority == filter.Priority);

        if (filter.CategoryId is not null)
            query = query.Where(t => t.CategoryId == filter.CategoryId);

        if (filter.AssignedTo is not null)
            query = query.Where(t => t.AssignedTo == filter.AssignedTo);

        if (filter.CreatedBy is not null)
            query = query.Where(t => t.CreatedBy == filter.CreatedBy);

        if (filter.UnassignedOnly == true)
            query = query.Where(t => t.AssignedTo == null);

        if (!string.IsNullOrWhiteSpace(filter.SearchKeyword))
        {
            var keyword = filter.SearchKeyword.Trim();
            query = query.Where(t =>
                t.Title.Contains(keyword) ||
                (t.Description != null && t.Description.Contains(keyword)));
        }

        query = filter.SortBy switch
        {
            "Title" => filter.SortDescending ? query.OrderByDescending(t => t.Title) : query.OrderBy(t => t.Title),
            "Status" => filter.SortDescending ? query.OrderByDescending(t => t.Status) : query.OrderBy(t => t.Status),
            "UpdatedAt" => filter.SortDescending ? query.OrderByDescending(t => t.UpdatedAt) : query.OrderBy(t => t.UpdatedAt),
            _ => filter.SortDescending ? query.OrderByDescending(t => t.CreatedAt) : query.OrderBy(t => t.CreatedAt)
        };

        var total = await query.CountAsync();
        var items = await query
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync();

        return new PagedResult<Ticket>
        {
            Items = items,
            TotalCount = total,
            Page = filter.Page,
            PageSize 

= filter.PageSize
        };
    }

    public async Task AddAsync(Ticket ticket)
    {
        await _context.Tickets.AddAsync(ticket);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Ticket ticket)
    {
        _context.Tickets.Update(ticket);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var ticket = await GetByIdAsync(id);
        if (ticket is not null)
        {
            _context.Tickets.Remove(ticket);
            await _context.SaveChangesAsync();
        }
    }
}