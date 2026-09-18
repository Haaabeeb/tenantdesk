using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TenantDesk.Application.Dto.Dashboard;
using TenantDesk.Application.Services;

namespace TenantDesk.Api.Controllers;

[ApiController]
[Route("api/dashboard")]
[Authorize(Roles = "Admin")]
public class DashboardController(
    DashboardService _dashboardService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<DashboardResponseDto>> Get()
    {
        var dashboard = await _dashboardService.GetAsync();

        return Ok(dashboard);
    }
}