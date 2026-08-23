
using TenantDesk.Domain.Entities;
using TenantDesk.Domain.Enums;

namespace TenantDesk.Application.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id);
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByMicrosoftIdAsync(string msId);
    Task<IEnumerable<User>> GetAllByTenantAsync(Guid tenantId);
    Task<IEnumerable<User>> GetByRoleAsync(Guid tenantId, UserRole role);
    Task<IEnumerable<User>> GetActiveAgentsAsync(Guid tenantId);
    Task<IEnumerable<User>> GetActiveAdminsAsync(Guid tenantId);
    Task<bool> ExistsByEmailAsync(Guid tenantId, string email);
    Task AddAsync(User user);
    Task UpdateAsync(User user);
    Task DeleteAsync(Guid id);
}
