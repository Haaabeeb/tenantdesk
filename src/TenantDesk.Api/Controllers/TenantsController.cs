using Microsoft.AspNetCore.Mvc;
using TenantDesk.Application.Dto.Tenant;
using TenantDesk.Application.Services;

namespace TenantDesk.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TenantsController(TenantService tenantService) : ControllerBase
{
    private readonly TenantService _tenantService = tenantService;

    // POST: api/tenants
    [HttpPost]
    public async Task<ActionResult<TenantResponseDto>> Create(CreateTenantDto dto)
    {
        var tenant = await _tenantService.CreateAsync(dto);

        return CreatedAtAction(nameof(GetById),new { id = tenant.TenantId },tenant);
    }

    // GET: api/tenants
    [HttpGet]
    public async Task<ActionResult<IEnumerable<TenantListDto>>> GetAll()
    {
        var tenants = await _tenantService.GetAllAsync();

        return Ok(tenants);
    }

    // GET: api/tenants/{id}
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<TenantResponseDto>> GetById(Guid id)
    {
        var tenant = await _tenantService.GetByIdAsync(id);

        return Ok(tenant);
    }

    // PUT: api/tenants/{id}
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<TenantResponseDto>> Update(
        Guid id,
        UpdateTenantDto dto)
    {
        var tenant = await _tenantService.UpdateAsync(id, dto);

        return Ok(tenant);
    }

    // PUT: api/tenants/{id}/status
    [HttpPut("{id:guid}/status")]
    public async Task<IActionResult> ChangeStatus( Guid id,ChangeTenantStatusDto dto)
    {
        await _tenantService.ChangeStatusAsync(id, dto);

        return NoContent();
    }

    // PUT: api/tenants/{id}/plan
    [HttpPut("{id:guid}/plan")]
    public async Task<IActionResult> ChangePlan(Guid id,ChangeTenantPlanDto dto)
    {
        await _tenantService.ChangePlanAsync(id, dto);

        return NoContent();
    }

    // DELETE: api/tenants/{id}
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _tenantService.DeleteAsync(id);

        return NoContent();
    }


}

