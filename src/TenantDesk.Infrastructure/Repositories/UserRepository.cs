using Microsoft.EntityFrameworkCore;
using TenantDesk.Application.Common;
using TenantDesk.Application.Interfaces;
using TenantDesk.Application.Interfaces.Persistence;
using TenantDesk.Domain.Entities;
using TenantDesk.Infrastructure.Data;

namespace TenantDesk.Infrastructure.Repositories;

public class UserRepository(AppDbContext _context) : IUserRepository
{

    public Task<User?> GetByIdAsync(Guid id) =>
        _context.Users.FirstOrDefaultAsync(u => u.UserId == id);

    public Task<User?> GetByEmailAsync(string email, Guid tenantId) =>
        _context.Users.FirstOrDefaultAsync(u => u.Email == email && u.TenantId == tenantId);

    public Task<User?> GetByMicrosoftIdAsync(string msId) =>
        _context.Users.FirstOrDefaultAsync(u => u.MicrosoftUserId == msId);

    public Task<bool> ExistsByEmailAsync(Guid tenantId, string email) =>
        _context.Users.AnyAsync(u => u.TenantId == tenantId && u.Email == email);

    public async Task<PagedResult<User>> GetFilteredAsync(UserFilter filter)
    {
        var page = filter.Page < 1 ? 1 : filter.Page;
        var pageSize = filter.PageSize is < 1 or > 100 ? 20 : filter.PageSize;
        
        var query = _context.Users.Where(u => u.TenantId == filter.TenantId);

        if (filter.Role is not null)
            query = query.Where(u => u.Role == filter.Role);

        if (filter.IsActive is not null)
            query = query.Where(u => u.IsActive == filter.IsActive);

        if (!string.IsNullOrWhiteSpace(filter.SearchKeyword))
            query = query.Where(u => u.Email.Contains(filter.SearchKeyword));

        query = query.OrderByDescending(u => u.CreatedAt);
    
        var total = await query.CountAsync();
        var items = await query
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync();

        return new PagedResult<User>
        {
            Items = items,
            TotalCount = total,
            Page = filter.Page,
            PageSize = filter.PageSize
        };
    }

    public async Task AddAsync(User user)
    {
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(User user)
    {
        _context.Users.Update(user);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var user = await GetByIdAsync(id);
        if (user is not null)
        {
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
        }
    }
}