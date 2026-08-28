
using TenantDesk.Application.Common;
using TenantDesk.Application.Interfaces.Persistence.Filters;
using TenantDesk.Domain.Entities;

public interface ICommentRepository
{
    
    public Task<Comment> GetByIdAsync(Guid id);
    public Task<int> CountByTicketAsync(Guid ticketId);
    public Task AddAsync(Comment comment);
    public Task UpdateAsync(Comment commnet);
    public Task DeleteAsync(Guid id);
    Task<PagedResult<Comment>> GetFilteredAsync(CommentFilter filter);  




}