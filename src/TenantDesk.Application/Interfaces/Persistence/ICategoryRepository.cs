using TenantDesk.Application.Common;
using TenantDesk.Application.Interfaces.Persistence;
using TenantDesk.Domain.Entities;

public interface ICategoryRepository
{
    
    public Task<Category?> GetByIdAsync(Guid id);
    public Task<bool> ExistsByNameAsync(Guid tenantId, string name);
    public Task<int> GetTicketCountByCategoryAsync(Guid categoryId);
    public Task AddAsync(Category category);
    public Task UpdateAsync(Category category);
    public Task DeleteAsync(Guid id);
    Task<PagedResult<Category>> GetFilteredAsync(CategoryFilter filter);
}