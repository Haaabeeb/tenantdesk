
using TenantDesk.Domain.Entities;

public interface ICommentRepository
{
    
    public Task<Comment> GetByIdAsync(Guid id);
    public Task<IEnumerable<Comment>> GetBYUserAsync(Guid ticketId);
    public Task<IEnumerable<Comment>> GetByUserAsync(Guid userId);
    public Task<IEnumerable<Comment>> GetRecentBYTicketAsync(Guid ticketId, int count);
    public Task<int> CountByTicketAsync(Guid ticketId);
    public Task AddAsync(Comment comment);
    public Task UpdateAsync(Comment commnet);
    public Task DeleteAsync(Guid id);





}