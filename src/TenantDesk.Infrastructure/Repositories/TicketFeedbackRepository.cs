using Microsoft.EntityFrameworkCore;
using TenantDesk.Application.Interfaces.Persistence;
using TenantDesk.Domain.Entities;
using TenantDesk.Infrastructure.Data;

namespace TenantDesk.Infrastructure.Repositories;

public class TicketFeedbackRepository(AppDbContext _context) : ITicketFeedbackRepository
{
    public Task<TicketFeedback?> GetByIdAsync(Guid id) =>
        _context.TicketFeedbacks.FirstOrDefaultAsync(f => f.TicketFeedbackId == id);

    public Task<TicketFeedback?> GetByTicketAsync(Guid ticketId) =>
        _context.TicketFeedbacks.FirstOrDefaultAsync(f => f.TicketId == ticketId);

    public async Task<IEnumerable<TicketFeedback>> GetAllByTenantAsync(Guid tenantId) =>
        await _context.TicketFeedbacks.Where(f => f.TenantId == tenantId).ToListAsync();

    public async Task<double> GetAverageRatingByTenantAsync(Guid tenantId)
    {
        var ratings = await _context.TicketFeedbacks
            .Where(f => f.TenantId == tenantId)
            .Select(f => f.Rating)
            .ToListAsync();

        return ratings.Count == 0 ? 0.0 : ratings.Average();
    }

    public async Task<double> GetAverageRatingByAgentAsync(Guid agentId)
    {
        var ratings = await (
            from f in _context.TicketFeedbacks
            join t in _context.Tickets on f.TicketId equals t.TicketId
            where t.AssignedTo == agentId
            select f.Rating
        ).ToListAsync();

        return ratings.Count == 0 ? 0.0 : ratings.Average();
    }

    public async Task<IEnumerable<TicketFeedback>> GetLowRatedFeedbacksAsync(Guid tenantId, int maxRating) =>
        await _context.TicketFeedbacks
            .Where(f => f.TenantId == tenantId && f.Rating <= maxRating)
            .ToListAsync();

    public async Task AddAsync(TicketFeedback feedback)
    {
        await _context.TicketFeedbacks.AddAsync(feedback);
        await _context.SaveChangesAsync();
    }

    public Task<bool> ExistsForTicketAsync(Guid ticketId) =>
        _context.TicketFeedbacks.AnyAsync(f => f.TicketId == ticketId);
}
