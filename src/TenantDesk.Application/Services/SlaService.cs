using System.Diagnostics.CodeAnalysis;
using AutoMapper;
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

    public SlaService(ISlaRepository slaRepository,ICurrentTenantService currentTenantService,IMapper mapper)
    {
        _slaRepository = slaRepository;
        _currentTenantService = currentTenantService;
        _mapper = mapper;
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

        if (sla == null){
            throw new KeyNotFoundException($"No SLA configured for priority '{priority}'.");
        }

        return _mapper.Map<SlaResponseDto>(sla);
    }

    public async Task<IEnumerable<SlaResponseDto>> GetAllAsync()
    {
        var tenantId = _currentTenantService.GetTenantId();
        var slas = await _slaRepository.GetAllByTenantAsync(tenantId);

        return _mapper.Map<IEnumerable<SlaResponseDto>>(slas);
    }

    public async Task<SlaResponseDto> UpdateAsync(Guid id, UpdateSlaTimesDto dto)
    {
        if (dto.ResponseTime > dto.ResolutionTime)
            throw new ArgumentException("Response time cannot be greater than resolution time.");

        var sla = await _slaRepository.GetByIdAsync(id);
        EnsureExistsAndOwned(sla);

        sla.UpdateTimes(dto.ResponseTime, dto.ResolutionTime);
        await _slaRepository.UpdateAsync(sla);

        return _mapper.Map<SlaResponseDto>(sla);
    }

    public async Task DeleteAsync(Guid id)
    {
        var sla = await _slaRepository.GetByIdAsync(id);
        EnsureExistsAndOwned(sla);

        await _slaRepository.DeleteAsync(id);
    }

    private void EnsureExistsAndOwned([NotNull] Sla? sla)
    {
        if (sla is null)
            throw new KeyNotFoundException("SLA not found.");

        if (sla.TenantId != _currentTenantService.GetTenantId())
            throw new UnauthorizedAccessException("You cannot access resources belonging to another tenant.");
    }


}