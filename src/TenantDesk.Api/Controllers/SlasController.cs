

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TenantDesk.Application.Dto.Sla;
using TenantDesk.Application.Services;
using TenantDesk.Domain.Enums;

namespace TenantDesk.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class SlasController : ControllerBase
{
    private readonly SlaService _slaService;

    public SlasController(SlaService slaService)
    {
        _slaService = slaService;
    }

    // POST: api/slas
    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<ActionResult<SlaResponseDto>> Create([FromBody] CreateSlaDto dto)
    {
        var result = await _slaService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = result.SlaId }, result);
    }

    // GET: api/slas/{id}
    [Authorize(Roles = "Admin,Agent")]
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<SlaResponseDto>> GetById(Guid id)
    {
        var result = await _slaService.GetByIdAsync(id);
        return Ok(result);
    }

    // GET: api/slas
    [Authorize(Roles = "Admin,Agent")]
    [HttpGet]
    public async Task<ActionResult<IEnumerable<SlaResponseDto>>> GetAll()
    {
        var result = await _slaService.GetAllAsync();
        return Ok(result);
    }

    // GET: api/slas/priority/{priority}
    [Authorize(Roles = "Admin,Agent")]
    [HttpGet("priority/{priority}")]
    public async Task<ActionResult<SlaResponseDto>> GetByPriority(PriorityLevel priority)
    {
        var result = await _slaService.GetByPriorityAsync(priority);
        return Ok(result);
    }

    // PUT: api/slas/{id}
    [Authorize(Roles = "Admin")]
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<SlaResponseDto>> Update(Guid id, [FromBody] UpdateSlaTimesDto dto)
    {
        var result = await _slaService.UpdateAsync(id, dto);
        return Ok(result);
    }

    // DELETE: api/slas/{id}
    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _slaService.DeleteAsync(id);
        return NoContent();
    }
}