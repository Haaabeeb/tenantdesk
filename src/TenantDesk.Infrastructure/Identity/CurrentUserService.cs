using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using TenantDesk.Application.Interfaces.Identity;
using TenantDesk.Domain.Enums;

namespace TenantDesk.Infrastructure.Services;

public class CurrentUserService(IHttpContextAccessor accessor) : ICurrentUserService
{
    private readonly IHttpContextAccessor _accessor = accessor;

    public Guid GetUserId()
    {
       
        var claim = _accessor.HttpContext?.User.FindFirst("local_user_id")?.Value
            ?? throw new UnauthorizedAccessException("User not authenticated");
        return Guid.Parse(claim);
    }

    public UserRole GetUserRole()
    {
        var claim = _accessor.HttpContext?.User.FindFirst(ClaimTypes.Role)?.Value
            ?? throw new UnauthorizedAccessException("Role claim missing");
        return Enum.Parse<UserRole>(claim);
    }

    public string GetUserEmail() =>
        _accessor.HttpContext?.User.FindFirst(ClaimTypes.Email)?.Value
            ?? throw new UnauthorizedAccessException("Email claim missing");

   
    public bool IsAuthenticated() =>
        _accessor.HttpContext?.User.Identity?.IsAuthenticated ?? false;
}