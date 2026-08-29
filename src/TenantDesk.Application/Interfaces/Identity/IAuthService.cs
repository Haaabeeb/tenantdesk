using TenantDesk.Application.Dto.Auth;

namespace TenantDesk.Application.Interfaces.Identity;

public interface IAuthService
{
    Task<AuthResponseDto> LoginAsync(LoginDto dto);
    Task<AuthResponseDto> RegisterAsync(RegisterDto dto);
}