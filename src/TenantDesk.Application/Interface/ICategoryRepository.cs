using TenantDesk.Domain.Entities;

public interface ICategoryRepository
{
    public Task<Category?> GetByIdAsync(Guid id);
    public Task<IEnumerable<Category>> GetAllByTenantAsync(Guid tenantId);
    public Task<IEnumerable<Category>> GetActiveAsync(Guid tenantId);
    public Task<IEnumerable<Category>> GetInactivrAsync(Guid tenantId);
    public Task AddAsync(Category category);
    public Task UpdateAsync(Category category);
    public Task DeleteAsync(Guid id);
    public Task<bool> GetByNameAsync(Guid tenantId, string name);
    public Task<int> GetTicketCountByCategoryAsync(Guid categoryId);
    public Task<int> ExistsByNameAsync(Guid tenantId,string name);




}