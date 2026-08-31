
using TenantDesk.Domain.Enums;

namespace TenantDesk.Application.Interfaces.Identity;

public interface ICurrentUserService
{
    Guid GetUserId();
    UserRole GetUserRole();
    string GetUserEmail();
    bool IsAuthenticated();
}