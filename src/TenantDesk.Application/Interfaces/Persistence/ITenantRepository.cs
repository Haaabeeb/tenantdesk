
using TenantDesk.Domain.Entities;

namespace TenantDesk.Application.Interfaces;

public interface ITenantRepository
{
    Task<Tenant?> GetByIdAsync(Guid id);
    Task<Tenant?> GetBySubdomainAsync(string subdomain);
    Task<IEnumerable<Tenant>> GetAllAsync();
    Task<IEnumerable<Tenant>> GetActiveAsync();
    Task<IEnumerable<Tenant>> GetByPlanAsync(Guid planId);
    Task<bool> ExistsBySubdomainAsync(string subdomain);
    Task AddAsync(Tenant tenant);
    Task UpdateAsync(Tenant tenant);
    Task DeleteAsync(Guid id);
}