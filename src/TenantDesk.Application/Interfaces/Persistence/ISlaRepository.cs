
using TenantDesk.Domain.Entities;
using TenantDesk.Domain.Enums;

namespace TenantDesk.Application.Interfaces;

public interface ISlaRepository
{
    Task<Sla?> GetByIdAsync(Guid id);
    Task<Sla?> GetByTenantAndPriorityAsync(Guid tenantId, PriorityLevel priority);
    Task<IEnumerable<Sla>> GetAllByTenantAsync(Guid tenantId);
    Task AddAsync(Sla sla);
    Task UpdateAsync(Sla sla);
    Task DeleteAsync(Guid id);
}
