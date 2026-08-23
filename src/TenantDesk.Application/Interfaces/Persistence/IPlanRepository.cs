
using TenantDesk.Domain.Entities;

namespace TenantDesk.Application.Interfaces;

public interface IPlanRepository
{
    Task<Plan?> GetByIdAsync(Guid id);
    Task<IEnumerable<Plan>> GetAllAsync();
    Task<Plan?> GetByNameAsync(string name);
    Task AddAsync(Plan plan);
    Task UpdateAsync(Plan plan);
    Task DeleteAsync(Guid id);
}
