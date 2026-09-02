
using Microsoft.EntityFrameworkCore;
using TenantDesk.Application.Interfaces;
using TenantDesk.Domain.Entities;
using TenantDesk.Infrastructure.Data;

namespace TenantDesk.Infrastructure.Repositories;

public class BusinessHourRepository : IBusinessHourRepository
{
    private readonly AppDbContext _context;

     public BusinessHourRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<BusinessHour>> GetByTenantAsync(Guid tenantId)
        {
            return await _context.BusinessHours
                .AsNoTracking()
                .Where(b => b.TenantId == tenantId)
                .OrderBy(b => b.DayOfWeek)
                .ToListAsync();
        }

        public async Task<BusinessHour?> GetByDayAsync(Guid tenantId, DayOfWeek day)
        {
            return await _context.BusinessHours
                .FirstOrDefaultAsync(b => b.TenantId == tenantId && b.DayOfWeek == day);
        }

        public async Task<BusinessHour?> GetByIdAsync(Guid id)
        {
            return await _context.BusinessHours
                .FirstOrDefaultAsync(b => b.BusinessHourId == id);
        }

        public async Task<bool> IsWithinBusinessHoursAsync(Guid tenantId, DateTime time)
        {
            var businessHour = await _context.BusinessHours
                .AsNoTracking()
                .FirstOrDefaultAsync(b => b.TenantId == tenantId && b.DayOfWeek == time.DayOfWeek);

            if (businessHour is null || !businessHour.IsWorkingDay)
                return false;

            var timeOnly = TimeOnly.FromDateTime(time);

            return timeOnly >= businessHour.OpenTime && timeOnly < businessHour.CloseTime;
        }

        public async Task AddAsync(BusinessHour businessHour)
        {
            await _context.BusinessHours.AddAsync(businessHour);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(BusinessHour businessHour)
        {
            _context.BusinessHours.Update(businessHour);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var businessHour = await _context.BusinessHours.FirstOrDefaultAsync(b => b.BusinessHourId == id);
            if (businessHour is not null)
            {
                _context.BusinessHours.Remove(businessHour);
                await _context.SaveChangesAsync();
            }
        }


}
