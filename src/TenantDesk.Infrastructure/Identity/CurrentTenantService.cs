using Microsoft.AspNetCore.Http;
using TenantDesk.Application.Interfaces.Identity;

namespace TenantDesk.Infrastructure.Services;

public class CurrentTenantService : ICurrentTenantService
{
    private readonly IHttpContextAccessor _accessor;

    public CurrentTenantService(IHttpContextAccessor accessor) => _accessor = accessor;

    public Guid GetTenantId()
    {
        
        var claim = _accessor.HttpContext?.User.FindFirst("tenant_id")?.Value
            ?? throw new UnauthorizedAccessException("Tenant context missing");
        return Guid.Parse(claim);
    }

    
    public string GetTenantSubdomain() =>
        _accessor.HttpContext?.Request.Host.Host.Split('.').FirstOrDefault()
            ?? throw new UnauthorizedAccessException("Cannot resolve subdomain");
}