using TenantDesk.Domain.Entities;

namespace TenantDesk.Application.Interfaces.Identity;

public interface ITokenService
{
    string GenerateToken(User user);
}