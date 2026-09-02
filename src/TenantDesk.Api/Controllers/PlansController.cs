
using Microsoft.AspNetCore.Mvc;
using TenantDesk.Application.Services;
using TenantDesk.Application.Dto.Plan;

namespace TenantDesk.Api.Controllers;

[ApiController]
[Route("api/plans")]
public class PlansController(PlanService planService) : ControllerBase
{
    private readonly PlanService _planService = planService;

    // POST: api/plans
    [HttpPost]
    public async Task<IActionResult> CreatePlan(CreatePlanDto dto)=>
        Ok(await _planService.CreateAsync(dto));

    
    // GET: api/plans/{planId}
    [HttpGet("{planId}")]
    public async Task<IActionResult> GetPlanById(Guid planId) => 
        Ok(await _planService.GetByIdAsync(planId));
    
    // GET: api/plans
    [HttpGet]
    public async Task<IActionResult> GetAllPlans() => 
        Ok(await _planService.GetAllAsync());

    // PUT: api/plans/{planId}/rate-limit
    [HttpPut("{planId}/rate-limit")]
    public async Task<IActionResult> ChangeRateLimit(Guid planId, ChangePlanRateLimitDto dto) =>
        Ok(await _planService.ChangeRateLimitAsync(planId, dto));


    // DELETE: api/plans/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _planService.DeleteAsync(id);
        return NoContent();
    }





   

}