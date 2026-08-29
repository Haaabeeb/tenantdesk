
using TenantDesk.Application.Dto.Plan;
using TenantDesk.Application.Interfaces;
using TenantDesk.Domain.Entities;

namespace TenantDesk.Application.Services;

public class PlanService
{
    private readonly IPlanRepository _repository;

    public PlanService(IPlanRepository repository)
    {
        _repository = repository;
    }

    public async Task<PlanResponseDto> CreateAsync(CreatePlanDto dto)
    {
        var existingPlan = await _repository.GetByNameAsync(dto.Name);

        if (existingPlan is not null)
        {
            throw new InvalidOperationException($"A plan with the name '{dto.Name}' already exists.");
        }

        var plan = new Plan(dto.Name, dto.RateLimitPerMinute);
        
        await _repository.AddAsync(plan);

        return new PlanResponseDto(plan.PlanId, plan.Name, plan.RateLimitPerMinute, plan.CreatedAt);

    }

    public async Task<PlanResponseDto> ChangeRateLimitAsync(Guid planId, ChangePlanRateLimitDto dto)
    {
        var plan = await _repository.GetByIdAsync(planId);

        if (plan is null)
        {
            throw new KeyNotFoundException($"Plan with ID '{planId}' not found.");
        }

        plan.ChangeRateLimit(dto.RateLimitPerMinute);
       
        await _repository.UpdateAsync(plan);

        return new PlanResponseDto(plan.PlanId, plan.Name, plan.RateLimitPerMinute, plan.CreatedAt);
    }

    public async Task DeleteAsync(Guid planId)
    {
        var plan = await _repository.GetByIdAsync(planId);

        if (plan is null)
        {
            throw new KeyNotFoundException($"Plan with ID '{planId}' not found.");
        }

        await _repository.DeleteAsync(planId);
    }

    public async Task<IEnumerable<PlanResponseDto>> GetAllAsync()
    {
        var plans = await _repository.GetAllAsync();

        return plans.Select(plan => new PlanResponseDto(plan.PlanId, plan.Name, plan.RateLimitPerMinute, plan.CreatedAt));
    }

    public async Task<PlanResponseDto> GetByIdAsync(Guid planId)
    {
        var plan = await _repository.GetByIdAsync(planId);

        if (plan is null)
        {
            throw new KeyNotFoundException($"Plan with ID '{planId}' not found.");
        }

        return new PlanResponseDto(plan.PlanId, plan.Name, plan.RateLimitPerMinute, plan.CreatedAt);
    }

    public async Task<PlanResponseDto> GetByNameAsync(string name)
    {
        var plan = await _repository.GetByNameAsync(name);

        if (plan is null)
        {
            throw new KeyNotFoundException($"Plan with name '{name}' not found.");
        }

        return new PlanResponseDto(plan.PlanId, plan.Name, plan.RateLimitPerMinute, plan.CreatedAt);
    }





         














}
