using TenantDesk.Application.Dto.Tenant;
using TenantDesk.Application.Interfaces;
using TenantDesk.Domain.Entities;

namespace TenantDesk.Application.Services;

public class TenantService
{
    private readonly ITenantRepository _tenantRepository;
    private readonly IPlanRepository _planRepository;

    public TenantService(ITenantRepository tenantRepository,IPlanRepository planRepository)
    {
        _tenantRepository = tenantRepository;
        _planRepository = planRepository;
    }

    public async Task<TenantResponseDto> CreateAsync(CreateTenantDto dto)
    {
        var plan = await _planRepository.GetByIdAsync(dto.PlanId);

        if (plan is null)
            throw new KeyNotFoundException("Plan not found.");

        var subdomainExists = await _tenantRepository.ExistsBySubdomainAsync(dto.Subdomain.Trim().ToLowerInvariant());

        if (subdomainExists)
            throw new InvalidOperationException("Subdomain is already in use.");

        var tenant = new Tenant(dto.PlanId,dto.TenantName,dto.Subdomain);

        await _tenantRepository.AddAsync(tenant);

        return MapToResponseDto(tenant);
    }

    public async Task<TenantResponseDto?> GetByIdAsync(Guid id)
    {
        var tenant = await _tenantRepository.GetByIdAsync(id);

        if (tenant is null){
            throw new KeyNotFoundException($"Tenant with ID '{id}' not found.");
        }

        return MapToResponseDto(tenant);
    }
    public async Task<IEnumerable<TenantListDto>> GetAllAsync()
    {
        var tenants = await _tenantRepository.GetAllAsync();

        return tenants.Select(t => new TenantListDto(t.TenantId,t.TenantName,t.Subdomain,t.IsActive));
    }

    public async Task<TenantResponseDto> UpdateAsync(Guid id,UpdateTenantDto dto)
    {
        var tenant = await _tenantRepository.GetByIdAsync(id);

        if (tenant is null)
            throw new KeyNotFoundException("Tenant not found.");

        var normalizedSubdomain = dto.Subdomain.Trim().ToLowerInvariant();

        if (!string.Equals(tenant.Subdomain,normalizedSubdomain,StringComparison.OrdinalIgnoreCase))
        {
            var subdomainExists = await _tenantRepository.ExistsBySubdomainAsync(normalizedSubdomain);

            if (subdomainExists)
                throw new InvalidOperationException("Subdomain is already in use.");
        }

        tenant.Rename(dto.TenantName);
        tenant.ChangeSubdomain(normalizedSubdomain);

        await _tenantRepository.UpdateAsync(tenant);

        return MapToResponseDto(tenant);
    }

    public async Task ChangePlanAsync(Guid id,ChangeTenantPlanDto dto)
    {
        var tenant = await _tenantRepository.GetByIdAsync(id);

        if (tenant is null)
            throw new KeyNotFoundException("Tenant not found.");

        var plan = await _planRepository.GetByIdAsync(dto.PlanId);

        if (plan is null)
            throw new KeyNotFoundException("Plan not found.");

        tenant.ChangePlan(dto.PlanId);

        await _tenantRepository.UpdateAsync(tenant);
    }

    public async Task ChangeStatusAsync(Guid id,ChangeTenantStatusDto dto)
    {
        var tenant = await _tenantRepository.GetByIdAsync(id);

        if (tenant is null)
            throw new KeyNotFoundException("Tenant not found.");

        if (dto.IsActive)
            tenant.Activate();
        else
            tenant.Deactivate();

        await _tenantRepository.UpdateAsync(tenant);
    }

    public async Task DeleteAsync(Guid id)
    {
        var tenant = await _tenantRepository.GetByIdAsync(id);

        if (tenant is null)
            throw new KeyNotFoundException("Tenant not found.");

        await _tenantRepository.DeleteAsync(id);
    }

    private static TenantResponseDto MapToResponseDto(Tenant tenant)
    {
        return new TenantResponseDto(tenant.TenantId,tenant.PlanId,tenant.TenantName,tenant.Subdomain,
            tenant.IsActive,tenant.CreatedAt,tenant.UpdatedAt);
    }
}
