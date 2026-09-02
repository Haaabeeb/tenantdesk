using Microsoft.AspNetCore.Mvc;
using TenantDesk.Application.Dto.Auth;
using TenantDesk.Application.Interfaces.Identity;

namespace TenantDesk.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(IAuthService authService) : ControllerBase
{
    private readonly IAuthService _authService = authService;

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponseDto>> Register(RegisterDto dto)
    {
        
        var result = await _authService.RegisterAsync(dto);
        return Ok(result);
    }

    
    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseDto>> Login(LoginDto dto)
    {
        var result = await _authService.LoginAsync(dto);
        return Ok(result);
    }
}