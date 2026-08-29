
using Microsoft.EntityFrameworkCore;
using TenantDesk.Application.Interfaces;
using TenantDesk.Domain.Entities;
using TenantDesk.Infrastructure.Data;

namespace TenantDesk.Infrastructure.Repositories;

public class PlanRepository : IPlanRepository
{
  
    private readonly AppDbContext _context;

    public PlanRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task<Plan?> GetByIdAsync(Guid id) =>
    _context.Plans.FirstOrDefaultAsync(p => p.PlanId == id);
    
   public async Task<IEnumerable<Plan>> GetAllAsync()
   {
        return await _context.Plans
            .OrderBy(p => p.Name).ToListAsync();
    }

    public Task<Plan?> GetByNameAsync(string name) =>
        _context.Plans.FirstOrDefaultAsync(p => p.Name == name);

    public async Task AddAsync(Plan plan)
    {
        await _context.Plans.AddAsync(plan);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Plan plan)
    {
        _context.Plans.Update(plan);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var plan = await GetByIdAsync(id);
        if (plan is not null)
        {
            _context.Plans.Remove(plan);
            await _context.SaveChangesAsync();
        }
    }

}