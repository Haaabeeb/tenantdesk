using Microsoft.EntityFrameworkCore;
using TenantDesk.Application.Interfaces;
using TenantDesk.Domain.Entities;
using TenantDesk.Infrastructure.Data;

namespace TenantDesk.Infrastructure.Repositories;

public class TenantRepository(AppDbContext context) : ITenantRepository
{
    private readonly AppDbContext _context = context;

    public Task<Tenant?> GetByIdAsync(Guid id) =>
        _context.Tenants.FirstOrDefaultAsync(t => t.TenantId == id);

    public Task<Tenant?> GetBySubdomainAsync(string subdomain) =>
        _context.Tenants.FirstOrDefaultAsync(t => t.Subdomain == subdomain);

    public async Task<IEnumerable<Tenant>> GetAllAsync()
    {
        return await _context.Tenants.OrderBy(t => t.TenantName).ToListAsync();
    }

    public async Task<IEnumerable<Tenant>> GetActiveAsync()
    {
        return await _context.Tenants.Where(t => t.IsActive).
        OrderBy(t => t.TenantName).ToListAsync();
    }

    public async Task<IEnumerable<Tenant>> GetByPlanAsync(Guid planId)
    {
        return await _context.Tenants.Where(t => t.PlanId == planId).
        OrderBy(t => t.TenantName).ToListAsync();
    }

    public Task<bool> ExistsBySubdomainAsync(string subdomain) =>
        _context.Tenants.AnyAsync(t => t.Subdomain == subdomain);

    public async Task AddAsync(Tenant tenant)
    {
        await _context.Tenants.AddAsync(tenant);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Tenant tenant)
    {
        _context.Tenants.Update(tenant);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var tenant = await GetByIdAsync(id);

        if (tenant is not null)
        {
            _context.Tenants.Remove(tenant);
            await _context.SaveChangesAsync();
        }
    }
}

