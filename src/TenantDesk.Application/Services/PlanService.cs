using AutoMapper;
using TenantDesk.Application.Dto.Plan;
using TenantDesk.Application.Interfaces;
using TenantDesk.Domain.Entities;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace TenantDesk.Application.Services;

public class PlanService(
    IPlanRepository _repository,
    IMapper _mapper,
    IMemoryCache _cache,
    ILogger<PlanService> _logger)
{
    private const string PlansCacheKey = "plans";

    public async Task<PlanResponseDto> CreateAsync(CreatePlanDto dto)
    {
        var existingPlan = await _repository.GetByNameAsync(dto.Name);

        if (existingPlan is not null)
            throw new InvalidOperationException(
                $"A plan with the name '{dto.Name}' already exists.");

        var plan = new Plan(dto.Name, dto.RateLimitPerMinute);

        await _repository.AddAsync(plan);

        _cache.Remove(PlansCacheKey);

        _logger.LogInformation(
            "Plans cache INVALIDATED after creating plan {PlanName}",
            dto.Name);

        return _mapper.Map<PlanResponseDto>(plan);
    }

    public async Task<PlanResponseDto> ChangeRateLimitAsync(
        Guid planId,
        ChangePlanRateLimitDto dto)
    {
        var plan = await _repository.GetByIdAsync(planId);

        if (plan is null)
        {
            throw new KeyNotFoundException(
                $"Plan with ID '{planId}' not found.");
        }

        plan.ChangeRateLimit(dto.RateLimitPerMinute);

        await _repository.UpdateAsync(plan);

        _cache.Remove(PlansCacheKey);

        _logger.LogInformation(
            "Plans cache INVALIDATED after changing rate limit for plan {PlanId}",
            planId);

        return _mapper.Map<PlanResponseDto>(plan);
    }

    public async Task DeleteAsync(Guid planId)
    {
        var plan = await _repository.GetByIdAsync(planId);

        if (plan is null)
        {
            throw new KeyNotFoundException(
                $"Plan with ID '{planId}' not found.");
        }

        await _repository.DeleteAsync(planId);

        _cache.Remove(PlansCacheKey);

        _logger.LogInformation(
            "Plans cache INVALIDATED after deleting plan {PlanId}",
            planId);
    }

     private readonly SemaphoreSlim _lock = new(1, 1);

    public async Task<IEnumerable<PlanResponseDto>> GetAllAsync()
        {
            if (_cache.TryGetValue(PlansCacheKey, out IEnumerable<PlanResponseDto>? cachedPlans))
                return cachedPlans!;

            await _lock.WaitAsync();

            try
            {
                if (_cache.TryGetValue(PlansCacheKey, out cachedPlans))
                    return cachedPlans!;

                var plans = await _repository.GetAllAsync();

                var result = _mapper.Map<IEnumerable<PlanResponseDto>>(plans);

                _cache.Set(PlansCacheKey, result, TimeSpan.FromMinutes(10));

                return result;
            }
            finally
            {
                _lock.Release();
            }
        }


    public async Task<PlanResponseDto> GetByIdAsync(Guid planId)
    {
        var plan = await _repository.GetByIdAsync(planId);

        if (plan is null)
        {
            throw new KeyNotFoundException(
                $"Plan with ID '{planId}' not found.");
        }

        return _mapper.Map<PlanResponseDto>(plan);
    }

    public async Task<PlanResponseDto> GetByNameAsync(string name)
    {
        var plan = await _repository.GetByNameAsync(name);

        if (plan is null)
        {
            throw new KeyNotFoundException(
                $"Plan with name '{name}' not found.");
        }

        return _mapper.Map<PlanResponseDto>(plan);
    }
}