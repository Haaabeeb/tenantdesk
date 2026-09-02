using Microsoft.AspNetCore.Mvc;
using TenantDesk.Application.Common;
using TenantDesk.Application.Dto.User;
using TenantDesk.Application.Interfaces.Persistence;
using TenantDesk.Application.Services;

namespace TenantDesk.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController(UserService _userService) : ControllerBase
{

    

    // GET: api/users/{id}
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<UserResponseDto>> GetById(Guid id)
    {
        var user = await _userService.GetByIdAsync(id);

        return Ok(user);
    }

    // GET: api/users?role=Agent&isActive=true&page=1&pageSize=20
    [HttpGet]
    public async Task<ActionResult<PagedResult<UserResponseDto>>> GetFiltered([FromQuery] UserFilter filter)
    {
        var result = await _userService.GetFilteredAsync(filter);

        return Ok(result);
    }

    // PUT: api/users/{id}
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<UserResponseDto>> Update(Guid id, UpdateUserDto dto)
    {
        var user = await _userService.UpdateAsync(id, dto);

        return Ok(user);
    }

    // PUT: api/users/{id}/status
    [HttpPut("{id:guid}/status")]
    public async Task<IActionResult> ChangeStatus(Guid id, ChangeUserStatusDto dto)
    {
        await _userService.ChangeStatusAsync(id, dto);

        return NoContent();
    }

    // DELETE: api/users/{id}
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _userService.DeleteAsync(id);

        return NoContent();
    }
}