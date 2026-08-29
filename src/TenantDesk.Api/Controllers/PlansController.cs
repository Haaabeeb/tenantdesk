
using Microsoft.AspNetCore.Mvc;
using TenantDesk.Application.Services;
using TenantDesk.Application.Dto.Plan;

namespace TenantDesk.Api.Controllers;

[ApiController]
[Route("api/plans")]
public class PlansController : ControllerBase
{
    private readonly PlanService _planService;

    public PlansController(PlanService planService)
    {
        _planService = planService;
    }


    // POST: api/plans
    [HttpPost]
    public async Task<IActionResult> CreatePlan(CreatePlanDto dto)=>
        Ok(await _planService.CreateAsync(dto));

    
    [HttpGet("{planId}")]
    public async Task<IActionResult> GetPlanById(Guid planId) => 
        Ok(await _planService.GetByIdAsync(planId));
    
    [HttpGet]
    public async Task<IActionResult> GetAllPlans() => 
        Ok(await _planService.GetAllAsync());

    [HttpPut("{planId}/rate-limit")]
    public async Task<IActionResult> ChangeRateLimit(Guid planId, ChangePlanRateLimitDto dto) =>
        Ok(await _planService.ChangeRateLimitAsync(planId, dto));

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _planService.DeleteAsync(id);
        return NoContent();
    }





   

}