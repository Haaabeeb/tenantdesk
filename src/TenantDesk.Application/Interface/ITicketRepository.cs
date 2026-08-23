
using System.Data.Common;
using System.Formats.Tar;
using System.Transactions;
using TenantDesk.Domain.Entities;
using TenantDesk.Domain.Enums;

public interface ITicketRepository
{
    
    public Task<Ticket?> GetByIdAsync(Guid id);
    public Task<IEnumerable<Ticket>> GetAllByTenantAsync(Guid id);
    public Task <IEnumerable<Ticket>> GetByStatusAsync(Guid tenantId,TicketStatus status);
    public Task <IEnumerable<Ticket>> GetByPriorityAsync(Guid tenantId, PriorityLevel priority);
    public Task<IEnumerable<Ticket>> GetByAssignedUserAsync(Guid userId);
    public Task<IEnumerable<Ticket>> GetByCreatedUserAsync(Guid userId);
    public Task<IEnumerable<Ticket>> GetByCategoryAsync(Guid categoryId);
    public Task<IEnumerable<Ticket>> SearchByTitleAsync(Guid tenantId , string keyword);
    public Task<IEnumerable<Ticket>> GetUnassignedAsync(Guid tenantId);
    public Task<IEnumerable<Ticket>> GetOverdueBySlaAsync(Guid tenantId);
    Task<int> CountByStatusAsync(Guid tenantId, TicketStatus status);
    public Task<int> CountAssignedToUserAsync(Guid userId);
    public Task<IEnumerable<Ticket>> GetPagedAsync(Guid tenantId, int page, int pageSize);
    public Task<IEnumerable<Ticket>> GetRecentAsync(Guid tenantId ,int count );
    public Task<int> CountByTenantAsync(Guid tenantId);
    public Task <Ticket> AddAsync(Ticket ticket);
    public Task UpdateAsync(Ticket ticket);
    public Task DeleteAsync(Guid id);
    public Task<bool> ExistsAsync(Guid id);


}