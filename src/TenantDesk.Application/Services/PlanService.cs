using AutoMapper;
using TenantDesk.Application.Dto.Plan;
using TenantDesk.Application.Interfaces;
using TenantDesk.Domain.Entities;

namespace TenantDesk.Application.Services;

public class PlanService(
    IPlanRepository repository,
    IMapper mapper)
{
    private readonly IPlanRepository _repository = repository;
    private readonly IMapper _mapper = mapper;

    public async Task<PlanResponseDto> CreateAsync(CreatePlanDto dto)
    {
        var existingPlan = await _repository.GetByNameAsync(dto.Name);

        if (existingPlan is not null)
        {
            throw new InvalidOperationException(
                $"A plan with the name '{dto.Name}' already exists.");
        }

        var plan = new Plan(dto.Name, dto.RateLimitPerMinute);

        await _repository.AddAsync(plan);

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
    }

    public async Task<IEnumerable<PlanResponseDto>> GetAllAsync()
    {
        var plans = await _repository.GetAllAsync();

        return _mapper.Map<IEnumerable<PlanResponseDto>>(plans);
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