
using Microsoft.AspNetCore.Mvc;
using TenantDesk.Application.Dto.BusinessHour;
using TenantDesk.Application.Services;

namespace TenantDesk.Api.Controllers;

[ApiController]
[Route("api/business-hours")]
public class BusinessHoursController : ControllerBase
{
    private readonly BusinessHourService _businessHourService;

    public BusinessHoursController(BusinessHourService businessHourService)
    {
        _businessHourService = businessHourService;
    }

    // POST: api/business-hours
    [HttpPost]
    public async Task<ActionResult<BusinessHourResponseDto>> Create([FromBody] CreateBusinessHourDto dto)
    {
        var result = await _businessHourService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = result.BusinessHourId }, result);
    }

    // GET: api/business-hours/{id}
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<BusinessHourResponseDto>> GetById(Guid id)
    {
        var result = await _businessHourService.GetByIdAsync(id);
        return Ok(result);
    }

    // GET: api/business-hours
    [HttpGet]
    public async Task<ActionResult<IEnumerable<BusinessHourResponseDto>>> GetAll()
    {
        var result = await _businessHourService.GetAllAsync();
        return Ok(result);
    }

    // GET: api/business-hours/day/{day}
    [HttpGet("day/{day}")]
    public async Task<ActionResult<BusinessHourResponseDto>> GetByDay(DayOfWeek day)
    {
        var result = await _businessHourService.GetByDayAsync(day);
        return Ok(result);
    }

    // PUT: api/business-hours/{id}
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<BusinessHourResponseDto>> Update(Guid id, [FromBody] UpdateBusinessHourDto dto)
    {
        var result = await _businessHourService.UpdateAsync(id, dto);
        return Ok(result);
    }

    // DELETE: api/business-hours/{id}
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _businessHourService.DeleteAsync(id);
        return NoContent();
    }
}