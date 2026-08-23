using System.Security.Claims;
using TenantDesk.Domain.Entities;

namespace TenantDesk.Application.Interfaces.Identity;

public interface IAuthService
{
    Task<bool> ValidateTokenAsync(string token);
    Task<User> GetOrCreateUserFromClaimsAsync(ClaimsPrincipal principal);
}