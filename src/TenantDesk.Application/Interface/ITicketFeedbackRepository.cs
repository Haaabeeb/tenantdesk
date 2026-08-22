using TenantDesk.Domain.Entities;

public interface ITicketFeedbackRepository
{
    public Task<TicketFeedback?> GetByIdAsync(Guid id);
    public Task<TicketFeedback?> GetByTicketAsync(Guid ticketId);
    public Task<IEnumerable<TicketFeedback>> GetAllByTenantAsync(Guid tenantId);
    public Task<double> GetAverageRatingByTenantAsync(Guid tenantId);
    public Task<double> GetAverageRatingByAgentAsync(Guid agentId);
    public Task<IEnumerable<TicketFeedback>> GetLowRatedFeedbacksAsync(Guid tenantId, int maxRating);
    public Task AddAsync(TicketFeedback feedback);
    public Task<bool> ExistsForTicketAsync(Guid ticketId);
}