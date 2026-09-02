
using Microsoft.EntityFrameworkCore;
using TenantDesk.Application.Interfaces;
using TenantDesk.Domain.Entities;
using TenantDesk.Domain.Enums;
using TenantDesk.Infrastructure.Data;

namespace TenantDesk.Infrastructure.Repositories;

public class SlaRepository : ISlaRepository
{
    private readonly AppDbContext _context;

    public SlaRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Sla?> GetByIdAsync(Guid id)
    {
        return await _context.Slas.FirstOrDefaultAsync(s => s.SlaId == id);
    }

    public async Task<Sla?> GetByTenantAndPriorityAsync(Guid tenantId,PriorityLevel priority)
    {
        return await _context.Slas
            .FirstOrDefaultAsync(s => s.TenantId == tenantId && s.Priority == priority);
    }

    public async Task<IEnumerable<Sla>> GetAllByTenantAsync(Guid tenantId)
    {
        return await _context.Slas.AsNoTracking()
            .Where(s => s.TenantId == tenantId).ToListAsync();
    }

    public async Task AddAsync(Sla sla)
    {
        await _context.Slas.AddAsync(sla);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Sla sla)
    {
        _context.Slas.Update(sla);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var sla = await _context.Slas
            .FirstOrDefaultAsync(s => s.SlaId == id);

        if (sla is null)
            return;

        _context.Slas.Remove(sla);
        await _context.SaveChangesAsync();
    }


}
