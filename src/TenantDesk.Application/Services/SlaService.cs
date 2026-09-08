using System.Diagnostics.CodeAnalysis;
using AutoMapper;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using TenantDesk.Application.Dto.Sla;
using TenantDesk.Application.Interfaces;
using TenantDesk.Application.Interfaces.Identity;
using TenantDesk.Domain.Entities;
using TenantDesk.Domain.Enums;

namespace TenantDesk.Application.Services;

public class SlaService
{
    private readonly ISlaRepository _slaRepository;
    private readonly ICurrentTenantService _currentTenantService;
    private readonly IMapper _mapper;
    private readonly IMemoryCache _cache;
    private readonly ILogger<SlaService> _logger;
    private readonly SemaphoreSlim _lock = new(1, 1);

    public SlaService(ISlaRepository slaRepository,ICurrentTenantService currentTenantService,IMapper mapper,
        IMemoryCache cache,ILogger<SlaService> logger)
    {
        _slaRepository = slaRepository;
        _currentTenantService = currentTenantService;
        _mapper = mapper;
        _cache = cache;
        _logger = logger;
    }

    public async Task<SlaResponseDto> CreateAsync(CreateSlaDto dto)
    {
        if (dto.ResponseTime > dto.ResolutionTime)
            throw new ArgumentException("Response time cannot be greater than resolution time.");

        var tenantId = _currentTenantService.GetTenantId();

        var existing = await _slaRepository.GetByTenantAndPriorityAsync(tenantId, dto.Priority);
        if (existing is not null)
            throw new InvalidOperationException($"An SLA for priority '{dto.Priority}' already exists for this tenant.");

        var sla = new Sla(tenantId, dto.Priority, dto.ResponseTime, dto.ResolutionTime);
        await _slaRepository.AddAsync(sla);

        InvalidateCache(tenantId);

        return _mapper.Map<SlaResponseDto>(sla);
    }

    public async Task<SlaResponseDto> GetByIdAsync(Guid id)
    {
        var sla = await _slaRepository.GetByIdAsync(id);
        EnsureExistsAndOwned(sla);
        return _mapper.Map<SlaResponseDto>(sla);
    }

    public async Task<SlaResponseDto> GetByPriorityAsync(PriorityLevel priority)
    {
        var tenantId = _currentTenantService.GetTenantId();
        var sla = await _slaRepository.GetByTenantAndPriorityAsync(tenantId, priority);
        if (sla == null)
        {
            throw new KeyNotFoundException($"No SLA configured for priority '{priority}'.");
        }
        return _mapper.Map<SlaResponseDto>(sla);
    }

    public async Task<IEnumerable<SlaResponseDto>> GetAllAsync()
    {
        var tenantId = _currentTenantService.GetTenantId();
        var cacheKey = $"slas:{tenantId}";

        if (_cache.TryGetValue(cacheKey, out IEnumerable<SlaResponseDto>? cachedSlas))
        {
            _logger.LogInformation(
                "SLAs cache HIT for tenant {TenantId}", tenantId);
            return cachedSlas!;
        }

        _logger.LogInformation(
            "SLAs cache MISS for tenant {TenantId}, waiting for lock", tenantId);

        await _lock.WaitAsync();

        try
        {
            // Double-check the cache after acquiring the lock in case another
            // request already populated it while we were waiting.
            if (_cache.TryGetValue(cacheKey, out cachedSlas))
            {
                _logger.LogInformation(
                    "SLAs cache HIT (after lock) for tenant {TenantId}", tenantId);
                return cachedSlas!;
            }

            var slas = await _slaRepository.GetAllByTenantAsync(tenantId);
            var result = _mapper.Map<IEnumerable<SlaResponseDto>>(slas);

            _cache.Set(cacheKey, result, TimeSpan.FromMinutes(10));

            _logger.LogInformation("SLAs cache SET for tenant {TenantId}", tenantId);

            return result;
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<SlaResponseDto> UpdateAsync(Guid id, UpdateSlaTimesDto dto)
    {
        if (dto.ResponseTime > dto.ResolutionTime)
            throw new ArgumentException("Response time cannot be greater than resolution time.");

        var sla = await _slaRepository.GetByIdAsync(id);
        EnsureExistsAndOwned(sla);

        sla.UpdateTimes(dto.ResponseTime, dto.ResolutionTime);
        await _slaRepository.UpdateAsync(sla);

        InvalidateCache(sla.TenantId);

        return _mapper.Map<SlaResponseDto>(sla);
    }

    public async Task DeleteAsync(Guid id)
    {
        var sla = await _slaRepository.GetByIdAsync(id);
        EnsureExistsAndOwned(sla);

        await _slaRepository.DeleteAsync(id);

        InvalidateCache(sla.TenantId);
    }

    private void InvalidateCache(Guid tenantId)
    {
        var cacheKey = $"slas:{tenantId}";
        _cache.Remove(cacheKey);
        _logger.LogInformation("SLAs cache INVALIDATED for tenant {TenantId}", tenantId);
    }

    private void EnsureExistsAndOwned([NotNull] Sla? sla)
    {
        if (sla is null)
            throw new KeyNotFoundException("SLA not found.");

        if (sla.TenantId != _currentTenantService.GetTenantId())
            throw new UnauthorizedAccessException("You cannot access resources belonging to another tenant.");
    }
}
