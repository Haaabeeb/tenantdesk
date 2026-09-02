using Microsoft.EntityFrameworkCore;
using TenantDesk.Application.Common;
using TenantDesk.Application.Interfaces.Persistence;
using TenantDesk.Domain.Entities;
using TenantDesk.Infrastructure.Data;

namespace TenantDesk.Infrastructure.Repositories;

public class CategoryRepository(AppDbContext _context) : ICategoryRepository
{
    public Task<Category?> GetByIdAsync(Guid id) =>
        _context.Categories.FirstOrDefaultAsync(c => c.CategoryId == id);

    public Task<bool> ExistsByNameAsync(Guid tenantId, string name) =>
        _context.Categories.AnyAsync(c => c.TenantId == tenantId && c.CategoryName == name);

    public Task<int> GetTicketCountByCategoryAsync(Guid categoryId) =>
        _context.Tickets.CountAsync(t => t.CategoryId == categoryId);

    public async Task<PagedResult<Category>> GetFilteredAsync(CategoryFilter filter)
    {
        var query = _context.Categories.Where(c => c.TenantId == filter.TenantId);

        if (filter.IsActive is not null)
            query = query.Where(c => c.IsActive == filter.IsActive);

        query = query.OrderByDescending(c => c.CreatedAt);

        var total = await query.CountAsync();
        var items = await query
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync();

        return new PagedResult<Category>
        {
            Items = items,
            TotalCount = total,
            Page = filter.Page,
            PageSize = filter.PageSize
        };
    }

    public async Task AddAsync(Category category)
    {
        await _context.Categories.AddAsync(category);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Category category)
    {
        _context.Categories.Update(category);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var category = await GetByIdAsync(id);
        if (category is not null)
        {
            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
        }
    }
}