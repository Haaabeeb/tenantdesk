using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using TenantDesk.Application.Dto.Dashboard;
using TenantDesk.Application.Interfaces.Identity;
using TenantDesk.Domain.Enums;

namespace TenantDesk.Application.Services;

public class DashboardService(
    ITicketRepository _ticketRepository,
    ICurrentTenantService _currentTenantService,
    IDistributedCache _cache,
    ILogger<DashboardService> _logger)
{
    private readonly SemaphoreSlim _lock = new(1, 1);

    public async Task<DashboardResponseDto> GetAsync()
    {
        var tenantId = _currentTenantService.GetTenantId();
        var cacheKey = $"dashboard:{tenantId}";

        var cachedDashboard = await _cache.GetStringAsync(cacheKey);
        if (cachedDashboard is not null)
        {
            _logger.LogInformation(
                "Dashboard cache HIT for tenant {TenantId}", tenantId);

            return JsonSerializer.Deserialize<DashboardResponseDto>(
                cachedDashboard)!;
        }

        _logger.LogInformation(
            "Dashboard cache MISS for tenant {TenantId}, waiting for lock", tenantId);

        await _lock.WaitAsync();

        try
        {
            // Double-check the cache after acquiring the lock in case another
            // request already populated it while we were waiting.
            cachedDashboard = await _cache.GetStringAsync(cacheKey);
            if (cachedDashboard is not null)
            {
                _logger.LogInformation(
                    "Dashboard cache HIT (after lock) for tenant {TenantId}", tenantId);

                return JsonSerializer.Deserialize<DashboardResponseDto>(
                    cachedDashboard)!;
            }

            var totalTickets = await _ticketRepository.CountByTenantAsync(tenantId);
            var openTickets = await _ticketRepository.CountByStatusAsync(
                tenantId, TicketStatus.Open);
            var assignedTickets = await _ticketRepository.CountByStatusAsync(
                tenantId, TicketStatus.Assigned);
            var inProgressTickets = await _ticketRepository.CountByStatusAsync(
                tenantId, TicketStatus.InProgress);
            var closedTickets = await _ticketRepository.CountByStatusAsync(
                tenantId, TicketStatus.Closed);

            var dashboard = new DashboardResponseDto(
                totalTickets,
                openTickets,
                assignedTickets,
                inProgressTickets,
                closedTickets
            );

            var serializedDashboard = JsonSerializer.Serialize(dashboard);

            await _cache.SetStringAsync(
                cacheKey,
                serializedDashboard,
                new DistributedCacheEntryOptions
                {
                    SlidingExpiration = TimeSpan.FromMinutes(5)
                });

            _logger.LogInformation(
                "Dashboard cache SET for tenant {TenantId}", tenantId);

            return dashboard;
        }
        finally
        {
            _lock.Release();
        }
    }
    
}