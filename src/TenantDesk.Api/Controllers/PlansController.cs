using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using TenantDesk.Application.Services;
using TenantDesk.Application.Dto.Plan;


namespace TenantDesk.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/plans")]
public class PlansController(PlanService planService) : ControllerBase
{
    private readonly PlanService _planService = planService;

    // POST: api/plans
    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> CreatePlan(CreatePlanDto dto)
    {
        var plan = await _planService.CreateAsync(dto);

        return CreatedAtAction(
            nameof(GetPlanById),
            new { planId = plan.PlanId },
            plan);
    }

    // GET: api/plans/{planId}
    [Authorize(Roles = "Admin,Agent,Customer")]
    [HttpGet("{planId}")]
    public async Task<IActionResult> GetPlanById(Guid planId)
    {
        var plan = await _planService.GetByIdAsync(planId);

        return Ok(plan);
    }

    // GET: api/plans
    [Authorize(Roles = "Admin,Agent,Customer")]
    [HttpGet]
    public async Task<IActionResult> GetAllPlans()
    {
        var plans = await _planService.GetAllAsync();

        return Ok(plans);
    }

    // PUT: api/plans/{planId}/rate-limit
    [Authorize(Roles = "Admin")]
    [HttpPut("{planId}/rate-limit")]
    public async Task<IActionResult> ChangeRateLimit(
        Guid planId,
        ChangePlanRateLimitDto dto)
    {
        await _planService.ChangeRateLimitAsync(planId, dto);

        return NoContent();
    }

    // DELETE: api/plans/{id}
    [Authorize(Roles = "Admin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _planService.DeleteAsync(id);

        return NoContent();
    }


    
}