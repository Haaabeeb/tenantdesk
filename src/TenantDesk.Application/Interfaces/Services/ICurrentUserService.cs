// Interfaces/Services/ICurrentUserService.cs
using TenantDesk.Domain.Enums;

namespace TenantDesk.Application.Interfaces.Services;

public interface ICurrentUserService
{
    Guid GetUserId();
    UserRole GetUserRole();
    string GetUserEmail();
    bool IsAuthenticated();
}