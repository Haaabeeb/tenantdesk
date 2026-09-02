using System.Diagnostics.CodeAnalysis;
using AutoMapper;
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

    public BusinessHourService(IBusinessHourRepository businessHourRepository,ICurrentTenantService currentTenantService,IMapper mapper)
    {
        _businessHourRepository = businessHourRepository;
        _currentTenantService = currentTenantService;
        _mapper = mapper;
    }

    public async Task<BusinessHourResponseDto> CreateAsync(CreateBusinessHourDto dto)
    {
        var tenantId = _currentTenantService.GetTenantId();

        var existing = await _businessHourRepository.GetByDayAsync(tenantId, dto.DayOfWeek);
        if (existing is not null)
            throw new InvalidOperationException($"A business hour record for '{dto.DayOfWeek}' already exists for this tenant.");

        var businessHour = new BusinessHour(tenantId, dto.DayOfWeek, dto.OpenTime, dto.CloseTime);
        await _businessHourRepository.AddAsync(businessHour);

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
        var businessHours = await _businessHourRepository.GetByTenantAsync(tenantId);

        return _mapper.Map<IEnumerable<BusinessHourResponseDto>>(businessHours);
    }

    public async Task<BusinessHourResponseDto> UpdateAsync(Guid id, UpdateBusinessHourDto dto)
    {
        var businessHour = await _businessHourRepository.GetByIdAsync(id);
        EnsureExistsAndOwned(businessHour);

        businessHour.UpdateHours(dto.OpenTime, dto.CloseTime);
        await _businessHourRepository.UpdateAsync(businessHour);

        return _mapper.Map<BusinessHourResponseDto>(businessHour);
    }

    public async Task DeleteAsync(Guid id)
    {
        var businessHour = await _businessHourRepository.GetByIdAsync(id);
        EnsureExistsAndOwned(businessHour);

        await _businessHourRepository.DeleteAsync(id);
    }

    private void EnsureExistsAndOwned([NotNull] BusinessHour? businessHour)
    {
        if (businessHour is null)
            throw new KeyNotFoundException("Business hour record not found.");

        if (businessHour.TenantId != _currentTenantService.GetTenantId())
            throw new UnauthorizedAccessException("You cannot access resources belonging to another tenant.");
    }



}