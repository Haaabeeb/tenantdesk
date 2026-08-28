
using System.Data.Common;
using System.Formats.Tar;
using System.Transactions;
using TenantDesk.Application.Common;
using TenantDesk.Application.Interfaces.Persistence.Filters;
using TenantDesk.Domain.Entities;
using TenantDesk.Domain.Enums;

public interface ITicketRepository
{
    
    public Task<Ticket?> GetByIdAsync(Guid id);
    Task<int> CountByStatusAsync(Guid tenantId, TicketStatus status);
    public Task<int> CountAssignedToUserAsync(Guid userId);
    public Task<int> CountByTenantAsync(Guid tenantId);
    public Task <Ticket> AddAsync(Ticket ticket);
    public Task UpdateAsync(Ticket ticket);
    public Task DeleteAsync(Guid id);
    public Task<bool> ExistsAsync(Guid id);
    Task<PagedResult<Ticket>> GetFilteredAsync(TicketFilter filter);


}