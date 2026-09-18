using System.Diagnostics.CodeAnalysis;
using AutoMapper;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using TenantDesk.Application.Dto.BusinessHour;
using TenantDesk.Application.Interfaces;
using TenantDesk.Application.Interfaces.Identity;
using TenantDesk.Domain.Entities;

namespace TenantDesk.Application.Services;

public class BusinessHourService
{
    private readonly IBusinessHourRepository _businessHourRepository;
    private readonly ICurrentTenantService _currentTenantService;
    private readonly IMapper _mapper;
    private readonly IMemoryCache _cache;
    private readonly ILogger<BusinessHourService> _logger;
    private readonly SemaphoreSlim _lock = new(1, 1);

    public BusinessHourService(IBusinessHourRepository businessHourRepository,ICurrentTenantService currentTenantService,IMapper mapper,
        IMemoryCache cache,ILogger<BusinessHourService> logger)
    {
        _businessHourRepository = businessHourRepository;
        _currentTenantService = currentTenantService;
        _mapper = mapper;
        _cache = cache;
        _logger = logger;
    }

    public async Task<BusinessHourResponseDto> CreateAsync(CreateBusinessHourDto dto)
    {
        var tenantId = _currentTenantService.GetTenantId();

        var existing = await _businessHourRepository.GetByDayAsync(tenantId, dto.DayOfWeek);
        if (existing is not null)
            throw new InvalidOperationException($"A business hour record for '{dto.DayOfWeek}' already exists for this tenant.");

        var businessHour = new BusinessHour(tenantId, dto.DayOfWeek, dto.OpenTime, dto.CloseTime);
        await _businessHourRepository.AddAsync(businessHour);

        InvalidateCache(tenantId);

        return _mapper.Map<BusinessHourResponseDto>(businessHour);
    }

    public async Task<BusinessHourResponseDto> GetByIdAsync(Guid id)
    {
        var businessHour = await _businessHourRepository.GetByIdAsync(id);
        EnsureExistsAndOwned(businessHour);
        return _mapper.Map<BusinessHourResponseDto>(businessHour);
    }

    public async Task<BusinessHourResponseDto> GetByDayAsync(DayOfWeek day)
    {
        var tenantId = _currentTenantService.GetTenantId();
        var businessHour = await _businessHourRepository.GetByDayAsync(tenantId, day);
        if (businessHour == null)
        {
            throw new KeyNotFoundException($"No business hour configured for '{day}'.");
        }
        return _mapper.Map<BusinessHourResponseDto>(businessHour);
    }

    public async Task<IEnumerable<BusinessHourResponseDto>> GetAllAsync()
    {
        var tenantId = _currentTenantService.GetTenantId();
        var cacheKey = $"business-hours:{tenantId}";

        if (_cache.TryGetValue(cacheKey, out IEnumerable<BusinessHourResponseDto>? cachedBusinessHours))
        {
            _logger.LogInformation(
                "Business hours cache HIT for tenant {TenantId}", tenantId);
            return cachedBusinessHours!;
        }

        _logger.LogInformation(
            "Business hours cache MISS for tenant {TenantId}, waiting for lock", tenantId);

        await _lock.WaitAsync();

        try
        {
            // Double-check the cache after acquiring the lock in case another
            // request already populated it while we were waiting.
            if (_cache.TryGetValue(cacheKey, out cachedBusinessHours))
            {
                _logger.LogInformation(
                    "Business hours cache HIT (after lock) for tenant {TenantId}", tenantId);
                return cachedBusinessHours!;
            }

            var businessHours = await _businessHourRepository.GetByTenantAsync(tenantId);
            var result = _mapper.Map<IEnumerable<BusinessHourResponseDto>>(businessHours);

            _cache.Set(cacheKey, result, TimeSpan.FromMinutes(10));

            _logger.LogInformation(
                "Business hours cache SET for tenant {TenantId}", tenantId);

            return result;
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<BusinessHourResponseDto> UpdateAsync(Guid id, UpdateBusinessHourDto dto)
    {
        var businessHour = await _businessHourRepository.GetByIdAsync(id);
        EnsureExistsAndOwned(businessHour);

        businessHour.UpdateHours(dto.OpenTime, dto.CloseTime);
        await _businessHourRepository.UpdateAsync(businessHour);

        InvalidateCache(businessHour.TenantId);

        return _mapper.Map<BusinessHourResponseDto>(businessHour);
    }

    public async Task DeleteAsync(Guid id)
    {
        var businessHour = await _businessHourRepository.GetByIdAsync(id);
        EnsureExistsAndOwned(businessHour);

        await _businessHourRepository.DeleteAsync(id);

        InvalidateCache(businessHour.TenantId);
    }

    private void InvalidateCache(Guid tenantId)
    {
        var cacheKey = $"business-hours:{tenantId}";
        _cache.Remove(cacheKey);
        _logger.LogInformation(
            "Business hours cache INVALIDATED for tenant {TenantId}", tenantId);
    }

    private void EnsureExistsAndOwned([NotNull] BusinessHour? businessHour)
    {
        if (businessHour is null)
            throw new KeyNotFoundException("Business hour record not found.");

        if (businessHour.TenantId != _currentTenantService.GetTenantId())
            throw new UnauthorizedAccessException("You cannot access resources belonging to another tenant.");
    }


}