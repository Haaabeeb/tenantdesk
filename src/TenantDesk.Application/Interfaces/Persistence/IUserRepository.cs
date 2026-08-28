
using TenantDesk.Application.Common;
using TenantDesk.Application.Interfaces.Persistence;
using TenantDesk.Domain.Entities;
using TenantDesk.Domain.Enums;

namespace TenantDesk.Application.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id);
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByMicrosoftIdAsync(string msId);
    Task<bool> ExistsByEmailAsync(Guid tenantId, string email);
    Task AddAsync(User user);
    Task UpdateAsync(User user);
    Task DeleteAsync(Guid id);
    Task<PagedResult<User>> GetFilteredAsync(UserFilter filter);
}
